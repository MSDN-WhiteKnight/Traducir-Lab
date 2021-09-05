/* Traducir Lab
 * Copyright (c) 2021,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight)
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Traducir.Core.Models;
using Traducir.Core.Services;

namespace Traducir.Core
{
    public static class History
    {
        public static async Task HistoryToText(SOStringService svc, SOString str, TextWriter target)
        {
            var suggestions = await svc.GetSuggestionsByString(str.Id);
            await target.WriteLineAsync("Key: " + str.Key);
            await target.WriteLineAsync("Created: " + str.CreationDate.ToString(CultureInfo.InvariantCulture));

            await target.WriteLineAsync("Original string: ");
            await target.WriteLineAsync(str.OriginalString);

            if (str.HasTranslation)
            {
                await target.WriteLineAsync("Current translation: ");
                await target.WriteLineAsync(str.Translation);
            }

            await target.WriteLineAsync();

            if (suggestions.Length == 0)
            {
                await target.WriteLineAsync("(No suggestions found)");
            }

            for (int i = 0; i < suggestions.Length; i++)
            {
                await target.WriteLineAsync("Suggestion #" + (i + 1).ToString(CultureInfo.InvariantCulture) + ": ");
                await target.WriteLineAsync(suggestions[i].Suggestion);

                await target.WriteLineAsync(
                    "Author: " + suggestions[i].CreatedByName + " (" +
                    suggestions[i].CreatedById.ToString(CultureInfo.InvariantCulture) + ")");

                await target.WriteLineAsync("State: " + suggestions[i].State.ToString());
                await target.WriteLineAsync();

                for (int j = 0; j < suggestions[i].Histories.Length; j++)
                {
                    var hist = suggestions[i].Histories[j];
                    await target.WriteAsync(hist.CreationDate.ToString(CultureInfo.InvariantCulture).PadRight(20, ' '));

                    await target.WriteAsync(hist.UserName);
                    await target.WriteAsync(" [ID:");
                    await target.WriteAsync(hist.UserId.ToString(CultureInfo.InvariantCulture));
                    await target.WriteAsync("] ");
                    await target.WriteAsync(hist.HistoryType.ToString());

                    if (!string.IsNullOrEmpty(hist.Comment))
                    {
                        await target.WriteAsync(" (");
                        await target.WriteAsync(hist.Comment);
                        await target.WriteAsync(')');
                    }

                    await target.WriteLineAsync();
                }

                await target.WriteLineAsync();
            }
        }
    }
}
