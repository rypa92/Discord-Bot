using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Sheets.v4.Data;
using System.Globalization;
using Discord.Commands;

namespace DiscordBot.Core.Commands
{
    class PokemonInfo : ModuleBase<SocketCommandContext>
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "DeepDiveTally";
        static readonly string SpreadsheetId = "1hldoZjNYNEl4CDFd8KQis307Vpsgm7fiIh7FBrUjppU";
        static readonly string sheetFreestyle = "Freestyle";
        static readonly string sheetFormula = "Formula";
        static readonly string sheetRowLookup = "RowLookup";
        static SheetsService service;
        static TextInfo properText = new CultureInfo("en-US", false).TextInfo;

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

            var range = $"{sheetFreestyle}!B4:B548";
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
                if (Name[x].Contains(properText.ToTitleCase(attemptedInput)))
                {
                    CheckedNames.Add(Name[x]);
                }
            }

            return CheckedNames;
        }

        public List<string> getTypes(string input)
        {
            List<string> types = new List<string>();
            int rowFound = 0;

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

            rowFound = getRow(input);

            var range = $"{sheetFreestyle}!C{rowFound}:D{rowFound}";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var col in values)
                {
                    types.Add(col[0].ToString());
                    types.Add(col[1].ToString());
                }
            }

            return types;
        }

        public List<string> getStats(string input)
        {
            List<string> stats = new List<string>();
            int rowFound = 0;

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

            rowFound = getRow(input);

            var range = $"{sheetFreestyle}!F{rowFound}:H{rowFound}";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var col in values)
                {
                    stats.Add(col[0].ToString());
                    stats.Add(col[1].ToString());
                    stats.Add(col[2].ToString());
                }
            }
            return stats;
        }

        public List<string> getRanks(string input)
        {
            List<string> ranks = new List<string>();
            int rowFound = 0;

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

            rowFound = getRow(input);

            var range = $"{sheetFreestyle}!V{rowFound}:X{rowFound}";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var col in values)
                {
                    ranks.Add(col[0].ToString());
                    ranks.Add(col[2].ToString());
                }
            }
            return ranks;
        }

        public List<string> getFastMoves(string input)
        {
            List<string> moves = new List<string>();
            int rowFound = 0;

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

            rowFound = getRow(input);

            var range = $"{sheetFormula}!B{rowFound}:BE{rowFound}";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var col in values)
                {
                    for(int x = 0; x < 60; x += 4)
                    {
                        if(col.Count <= x)
                        {
                            break;
                        }
                        else
                        {
                            //Console.WriteLine($"Move: {col[x + 0].ToString()} | {col[x + 1].ToString()} | {col[x + 2].ToString()} | {col[x + 3].ToString()}");
                            moves.Add(col[x + 0].ToString());
                            moves.Add(col[x + 1].ToString());
                            moves.Add(col[x + 2].ToString());
                            moves.Add(col[x + 3].ToString());
                        }
                    }
                }
            }
            return moves;
        }

        public List<string> getChargeMoves(string input)
        {
            List<string> moves = new List<string>();
            int rowFound = 0;

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

            rowFound = getRow(input);

            var range = $"{sheetFormula}!BG{rowFound}:GZ{rowFound}";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var col in values)
                {
                    for (int x = 0; x < 150; x += 6)
                    {
                        if (col.Count <= x)
                        {
                            break;
                        }
                        else
                        {
                            //Console.WriteLine($"Move: {col[x + 0].ToString()} | {col[x + 1].ToString()} | {col[x + 2].ToString()} | {col[x + 3].ToString()} | {col[x + 4].ToString()} | {col[x + 5].ToString()}");
                            moves.Add(col[x + 0].ToString());
                            moves.Add(col[x + 1].ToString());
                            moves.Add(col[x + 2].ToString());
                            moves.Add(col[x + 3].ToString());
                            moves.Add(col[x + 4].ToString());
                            moves.Add(col[x + 5].ToString());
                        }
                    }
                }
            }
            return moves;
        }

        public int getRow(string input)
        {
            int rowFound = 0;

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

            //Update row with the input pokemon
            var updateRange = "RowLookup!A2";
            var updateValueRange = new ValueRange();

            var updateObject = new List<object>() { input };
            updateValueRange.Values = new List<IList<object>> { updateObject };

            var updateRequest = service.Spreadsheets.Values.Update(updateValueRange, SpreadsheetId, updateRange);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse = updateRequest.Execute();

            //Get the value of the row that the pokemon now appears on
            var rangeRowLookup = $"RowLookup!B2";
            var requestRowLookup = service.Spreadsheets.Values.Get(SpreadsheetId, rangeRowLookup);

            var responseRowLookup = requestRowLookup.Execute();
            var valuesRowLookup = responseRowLookup.Values;
            if (valuesRowLookup != null && valuesRowLookup.Count > 0)
            {
                foreach (var row in valuesRowLookup)
                {
                    rowFound = int.Parse(row[0].ToString());
                }
            }

            return rowFound;
        }
    }
}
