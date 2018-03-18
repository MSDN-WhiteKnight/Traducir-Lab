using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using StackExchange.Profiling;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ISOStringService
    {
        Task StoreNewStrings(TransifexString[] strings);

        Task RefreshCache();
    }
    public class SOStringService : ISOStringService
    {
        private IDbService _dbService { get; set; }
        private List<SOString> _strings { get; set; }
        public SOStringService(IDbService dbService)
        {
            _dbService = dbService;
        }

        private Task CreateTemporaryTable(DbConnection db)
        {
            return db.ExecuteAsync(@"
Drop Table If Exists dbo.ImportTable;
Create Table dbo.ImportTable
(
NormalizedKey  VarChar(255) Not Null,
[Key]          VarChar(255) Not Null,
Variant        VarChar(255) Null,
OriginalString NVarChar(Max) Not Null,
Translation    NVarChar(Max) Null,

Constraint PK_ImportTable Primary Key Clustered (NormalizedKey Asc)
)");
        }

        public async Task StoreNewStrings(TransifexString[] strings)
        {
            using(var db = _dbService.GetConnection())
            {
                await CreateTemporaryTable(db);
                using(MiniProfiler.Current.Step("Populate temp table"))
                {
                    var table = new DataTable();
                    table.Columns.Add("NormalizedKey", typeof(string));
                    table.Columns.Add("Key", typeof(string));
                    table.Columns.Add("Variant", typeof(string));
                    table.Columns.Add("OriginalString", typeof(string));
                    table.Columns.Add("Translation", typeof(string));

                    foreach (var s in strings)
                    {
                        table.Rows.Add(s.NormalizedKey, s.Key, s.Variant, s.Source, s.Translation);
                    }

                    var copyDb = (SqlConnection)db.InnerConnection;
                    using(var copy = new SqlBulkCopy(copyDb))
                    {
                        copy.DestinationTableName = "dbo.ImportTable";
                        foreach (DataColumn c in table.Columns)
                        {
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                        }

                        copyDb.Open();
                        copy.WriteToServer(table);
                    }
                }

                using(MiniProfiler.Current.Step("Delete strings"))
                {
                    await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select    s.Id, {=Deleted}, @now
From      Strings s
Left Join ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where     s.DeletionDate Is Null
And       feed.NormalizedKey Is Null;

Update    s
Set       s.DeletionDate = @now
From      Strings s
Left Join ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where     s.DeletionDate Is Null
And       feed.NormalizedKey Is Null;", new { now = DateTime.UtcNow, StringHistoryType.Deleted });
                }

                using(MiniProfiler.Current.Step("Update strings"))
                {
                    await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, Comment, CreationDate)
Select s.Id, {=Updated}, Concat('Key Updated from ', s.[Key], ' to ', feed.[Key]), @now
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.[Key] <> feed.[Key];

Update s
Set    s.[Key] = feed.[Key], s.Variant = feed.Variant
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.[Key] <> feed.[Key];", new { now = DateTime.UtcNow, StringHistoryType.Updated });
                }

                using(MiniProfiler.Current.Step("Undelete strings"))
                {
                    await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select s.Id, {=Undeleted}, @now
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.DeletionDate Is Not Null;

Update s
Set    s.DeletionDate = Null
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.DeletionDate Is Not Null;", new { now = DateTime.UtcNow, StringHistoryType.Undeleted });
                }

                using(MiniProfiler.Current.Step("Add new strings"))
                {
                    await db.ExecuteAsync(@"
Declare @NormalizedKeysToInsert Table (
    NormalizedKey VarChar(255) Not Null
);

Insert Into @NormalizedKeysToInsert
Select    feed.NormalizedKey
From      ImportTable feed
Left Join Strings s On s.NormalizedKey = feed.NormalizedKey
Where     s.NormalizedKey Is Null;

Insert Into Strings ([Key], NormalizedKey, Variant, OriginalString, Translation, CreationDate)
Select [Key], feed.NormalizedKey, feed.Variant, feed.OriginalString, feed.Translation, @now
From   ImportTable feed
Join   @NormalizedKeysToInsert s On s.NormalizedKey = feed.NormalizedKey;

Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select s.Id, {=Created}, @now
From   @NormalizedKeysToInsert i
Join   Strings s On s.NormalizedKey = i.NormalizedKey;", new { now = DateTime.UtcNow, StringHistoryType.Created });
                }
            }
        }

        public async Task RefreshCache()
        {
            const string sql = @"
Select Id, [Key], OriginalString, Translation, Variant, CreationDate
From   Strings
Where  DeletionDate Is Null;

Select ss.Id, ss.StringId, ss.Suggestion, ss.StateId, u.DisplayName CreatedBy, ss.CreationDate
From   StringSuggestions ss
Join   Strings s On s.Id = ss.StringId And s.DeletionDate Is Null
Join   Users u On ss.CreatedById = u.Id
Where  ss.StateId = {=Created}";

            using(MiniProfiler.Current.Step("Refreshing the strings cache"))
            using(var db = _dbService.GetConnection())
            using(var reader = await db.QueryMultipleAsync(sql, new { StringSuggestionState.Created }))
            {
                _strings = (await reader.ReadAsync<SOString>()).AsList();
                var suggestions = (await reader.ReadAsync<SOStringSuggestion>()).AsList();

                using(MiniProfiler.Current.Step("Attaching the suggestions to the strings"))
                {
                    var stringsById = _strings.ToDictionary(s => s.Id);
                    foreach (var g in suggestions.GroupBy(g => g.StringId))
                    {
                        if (stringsById.TryGetValue(g.Key, out var str))
                        {
                            str.Suggestions = g.ToArray();
                        }
                    }
                }
            }
        }
    }
}