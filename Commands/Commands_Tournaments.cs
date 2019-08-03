using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Sheets.v4.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.Core.Commands
{
    class TournamentCommands : ModuleBase<SocketCommandContext>
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "Tounraments";
        static readonly string SpreadsheetId = "1qDBctui1aTCzAKKGh0XxEaxQj4I5RXy6WOlJ9q0E_4k";
        static readonly string sheetParticipants = "Participants";
        static readonly string sheetSortedParts = "BySilphRank";
        static readonly string sheetGroup = "Group ";
        static SheetsService service;
        TextInfo properText = new CultureInfo("en-US", false).TextInfo;

        public int AddParticipant(string userName, ulong UserID, string timezone, string rank, string trainerCode)
        {
            timezone = $"=\"{timezone}\"";
            if(!dupParticipant(UserID))
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

                var range = $"{sheetParticipants}!B:G";
                var valueRange = new ValueRange();

                var ObjectList = new List<object>() { Convert.ToString(Convert.ToUInt64(UserID)), userName, timezone, properText.ToTitleCase(rank), 0, trainerCode};
                valueRange.Values = new List<IList<object>> { ObjectList };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = appendRequest.Execute();

                return 0;
            }
            else
            {
                return 1;
            }
        }

        public bool dupParticipant(ulong userID)
        {
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

            var range = $"{sheetParticipants}!B3:C1000";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    User.Add(Convert.ToUInt64(row[0]));
                }
            }

            for (int x = 0; x < User.Count; x++)
            {
                if (User[x] == userID)
                {
                    flag = true;
                }
            }

            return flag;
        }

        public string timezoneLookup(ulong userID)
        {
            List<UInt64> users = new List<UInt64>();

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

            var range = $"{sheetParticipants}!B3:D1000";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    if (Convert.ToUInt64(row[0]) == userID)
                    {
                        return row[2].ToString().Replace("\"", "");
                    }
                }
            }

            return "";
        }

        public bool createTournament(int roomInput)
        {
            List<UInt64> userID = new List<UInt64>();
            List<string> userName = new List<string>();
            List<string> externalRank = new List<string>();
            List<string> trainerCode = new List<string>();

            try
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

                var pullRange = $"{sheetSortedParts}!B3:G1000";
                var pullRequest = service.Spreadsheets.Values.Get(SpreadsheetId, pullRange);

                var pullResponse = pullRequest.Execute();
                var pullValues = pullResponse.Values;
                if (pullValues != null && pullValues.Count > 0)
                {
                    foreach (var row in pullValues)
                    {
                        userID.Add(Convert.ToUInt64(row[0]));
                        userName.Add(row[1].ToString());
                        externalRank.Add(row[3].ToString());
                        trainerCode.Add(row[5].ToString());
                    }
                }

                int numGroups = userID.Count / roomInput;
                int extraParts = userID.Count % roomInput;
                int roomSize = roomInput + (extraParts / 2);
                Console.WriteLine("Num Groups: " + numGroups + "\nExtras: " + extraParts + "\nRoom Size: " + roomSize);
                int currPart = 0;

                for (int x = 0; x <= numGroups; x++)
                {
                    if (currPart == userID.Count)
                    {
                        break;
                    }
                    var newSheet = new AddSheetRequest();
                    newSheet.Properties = new SheetProperties();
                    newSheet.Properties.Title = sheetGroup + (x + 1);
                    BatchUpdateSpreadsheetRequest buSheetRequest = new BatchUpdateSpreadsheetRequest();
                    buSheetRequest.Requests = new List<Request>();
                    buSheetRequest.Requests.Add(new Request
                    {
                        AddSheet = newSheet
                    });

                    var createSheet = service.Spreadsheets.BatchUpdate(buSheetRequest, SpreadsheetId).Execute();

                    var addRange = $"{sheetGroup + (x + 1)}!A:D";
                    var addRequest = new ValueRange();

                    for(int y = 0; y < roomSize; y++)
                    {
                        var ObjectList = new List<object>() { Convert.ToString(userID[currPart]), userName[currPart], externalRank[currPart], trainerCode[currPart] };
                        addRequest.Values = new List<IList<object>> { ObjectList };

                        var appendRequest = service.Spreadsheets.Values.Append(addRequest, SpreadsheetId, addRange);
                        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                        var appendResponse = appendRequest.Execute();

                        currPart++;
                        if (currPart == userID.Count)
                        {
                            break;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public List<ulong> getPartsFromRoom(int roomNumber)
        {
            List<UInt64> participants = new List<UInt64>();

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

            var range = $"{sheetGroup + (roomNumber + 1)}!A1:A100";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    participants.Add(Convert.ToUInt64(row[0]));
                }
            }

            return participants;
        }

        public List<string> getTrainerCodes(int roomNumber)
        {
            List<string> participants = new List<string>();

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

            var range = $"{sheetGroup + (roomNumber + 1)}!D1:D100";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    participants.Add(row[0].ToString());
                }
            }

            return participants;
        }

        public int getNumberOfRooms()
        {
            int rooms = 0;

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

            var sheetMeta = service.Spreadsheets.Get(SpreadsheetId).Execute();
            var sheets = sheetMeta.Sheets;
            for(int x = 0; x < sheets.Count; x++)
            {
                if(sheets[x].Properties.Title.Contains("Group"))
                {
                    rooms++;
                }
            }

            return rooms;
        }
    }
}
