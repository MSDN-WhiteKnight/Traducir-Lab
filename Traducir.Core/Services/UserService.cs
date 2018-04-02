using System.Threading.Tasks;
using Dapper;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Services
{
    public interface IUserService
    {
        Task UpsertUserAsync(User user);

        Task<User> GetUserAsync(int userId);
    }
    public class UserService : IUserService
    {
        private IDbService _dbService { get; }
        public UserService(IDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task UpsertUserAsync(User user)
        {
            using(var db = _dbService.GetConnection())
            {
                await db.ExecuteAsync(@"
Declare @wasReviewer Bit, @wasTrusted Bit;
Select @wasReviewer = IsReviewer, @wasTrusted = IsTrusted From Users Where Id = @Id;

If @wasReviewer Is Null
  -- It's an insert!
  Insert Into Users (Id, DisplayName, IsModerator, IsTrusted, IsReviewer, IsBanned, CreationDate, LastSeenDate)
  Values            (@Id, @DisplayName, @IsModerator, @IsModerator, @IsModerator, 0, @CreationDate, @LastSeenDate)
Else
  -- It's an update
  Update Users
  Set    DisplayName = @DisplayName,
         IsModerator = @IsModerator,
         IsReviewer = Case When @wasReviewer = 1 Then 1 Else @IsModerator End, -- if the user was a reviewer, keep them
                                                                               -- if they're now a mod, make them a reviewer
         IsTrusted = Case When @wasTrusted = 1 Then 1 Else @IsModerator End,
         LastSeenDate = @LastSeenDate
Where    Id = @Id", user);
            }
        }

        public async Task<User> GetUserAsync(int userId)
        {
            using(var db = _dbService.GetConnection())
            {
                return await db.QueryFirstOrDefaultAsync<User>(@"
Select Id, DisplayName, CreationDate, LastSeenDate,
       Case When IsBanned = 1 Then {=Banned}
            When IsReviewer = 1 Then {=Reviewer}
            When IsTrusted = 1 Then {=TrustedUser}
            Else {=User}
       End UserType
From   Users
Where  Id = @userId", new
                {
                    userId,
                    UserType.Banned,
                    UserType.Reviewer,
                    UserType.TrustedUser,
                    UserType.User
                });
            }
        }
    }
}