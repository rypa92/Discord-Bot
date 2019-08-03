using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Sheets.v4.Data;
using System.Globalization;

namespace DiscordBot.Core.Commands
{
    public class VoteCommands
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "DeepDiveTally";
        static readonly string SpreadsheetId = "1-JtM5LQE8KGG6-ejwEQuDjqgmPYHV3Sm1nfjTMR3ic0";
        static readonly string sheetDirty = "Dirty";
        static readonly string sheetResults = "Results";
        static readonly string sheetNeat = "Neat";
        static readonly string sheetCompleted = "Completed";
        static SheetsService service;
        static TextInfo properText = new CultureInfo("en-US", false).TextInfo;

        public bool DuplicateVotes(string attemptedInput, ulong UserID)
        {
            List<string> Name = new List<string>();
            List<UInt64> User = new List<UInt64>();
            bool flag = false;

            GoogleCredential credential;
            using (var stream = new FileStream("discordbot_credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var range = $"{sheetDirty}!A:B";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    Name.Add(row[0].ToString().ToLower());
                    User.Add(Convert.ToUInt64(row[1]));
                }
            }

            for(int x = 0; x < Name.Count; x++)
            {
                if(Name[x] == attemptedInput && User[x] == UserID)
                {
                    flag = true;
                }
            }

            return flag;
        }

        public List<string> PokemonExists(string attemptedInput)
        {
            List<string> Name = new List<string>();
            List<string> CheckedNames = new List<string>();

            GoogleCredential credential;
            using (var stream = new FileStream("discordbot_credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var range = $"{sheetNeat}!A:A";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    Name.Add(row[0].ToString());
                }
            }

            for (int x = 0; x < Name.Count; x++)
            {
                if(Name[x].Contains(properText.ToTitleCase(attemptedInput)))
                {
                    CheckedNames.Add(Name[x]);
                }
            }

            return CheckedNames;
        }

        public int AddVote(ulong UserID, string input)
        {
            if(DuplicateVotes(input, UserID))
            {
                return 1;
            }
            else
            {
                GoogleCredential credential;
                using (var stream = new FileStream("discordbot_credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
                }
                service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var range = $"{sheetDirty}!A:B";
                var valueRange = new ValueRange();

                var ObjectList = new List<object>() { input, Convert.ToString(Convert.ToUInt64(UserID)) };
                valueRange.Values = new List<IList<object>> { ObjectList };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = appendRequest.Execute();

                return 0;
            }
        }

        public List<string> GetTopFive()
        {
            List<string> topFiveResults = new List<string>();
            GoogleCredential credential;
            using (var stream = new FileStream("discordbot_credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var range = $"{sheetResults}!A:B";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if(values != null && values.Count > 0)
            {
                foreach(var row in values)
                {
                    topFiveResults.Add(row[0].ToString() + " (" + row[1].ToString() + ")");
                }
            }
            return topFiveResults;

        }

        public bool MarkComplete(string attemptedInput)
        {
            List<string> Name = new List<string>();
            GoogleCredential credential;
            using (var stream = new FileStream("discordbot_credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var range = $"{sheetNeat}!A:A";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    Name.Add(row[0].ToString());
                }
            }

            if (Name.Contains(attemptedInput))
            {
                //Mark Pokemon in 'Neat' with 0//

                var updateRange = $"{sheetNeat}!B{Name.IndexOf(attemptedInput) + 1}";
                var updateValueRange = new ValueRange();

                var objectList = new List<object>() { "0" };
                updateValueRange.Values = new List<IList<object>> { objectList };

                var updateRequest = service.Spreadsheets.Values.Update(updateValueRange, SpreadsheetId, updateRange);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var updateResponse = updateRequest.Execute();

                //Add Pokemon in 'Complete' for checking later//
                
                var comrange = $"{sheetCompleted}!A:A";
                var valueRange = new ValueRange();

                var ObjectList = new List<object>() { attemptedInput };
                valueRange.Values = new List<IList<object>> { ObjectList };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, comrange);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = appendRequest.Execute();

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckForComplete(string attemptedInput)
        {
            List<string> Name = new List<string>();
            bool flag = false;

            GoogleCredential credential;
            using (var stream = new FileStream("discordbot_credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var range = $"{sheetCompleted}!A:A";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    Name.Add(row[0].ToString());
                }
            }

            for (int x = 0; x < Name.Count; x++)
            {
                if (Name[x] == attemptedInput)
                {
                    flag = true;
                }
            }

            return flag;
        }
    }
}
