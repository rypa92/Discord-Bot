using System;
using System.Collections.Generic;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Linq;

namespace DiscordBot.Core.Commands
{
    public class Interface_Tournaments : ModuleBase<SocketCommandContext>
    {
        TournamentCommands DbCommands = new TournamentCommands();
        
        [Command("hello"), Summary("Hello World!")]
        public async Task hello()
        {
            await Context.Channel.SendMessageAsync("Psst, don't tell anyone about this ..");
        }

        [Command("join"), Summary("Join a tournament.")]
        public async Task join([Remainder]string input = "")
        {
            if (input == "")
            {
                await Context.Channel.SendMessageAsync("TODO: Put in command instructions..");
            }
            else
            {
                string[] inputs = input.Split(" ");
                if (Regex.IsMatch(inputs[0], @"^(?:Z|[+-](?:2[0-3]|[01][0-9]):[0-5][0-9])$"))
                {
                    if (Regex.IsMatch(inputs[2], @"^\d{4}-\d{4}-\d{4}$"))
                    {
                        int result = DbCommands.AddParticipant(Context.Message.Author.Username, Context.Message.Author.Id, inputs[0], inputs[1], inputs[2]);
                        if (result == 0)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} - You've been added to the tournament list.\n" +
                                                                   $"Please standby for the tournament to start.");
                        }
                        else if (result == 1)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} - Seems like you're already signed up for the tournament.\n" +
                                                                   $"Please standby for the tournament to start.");
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("No idea how we got here, but something broke..");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} - Seems like your trainer code was not entered correctly.\n" +
                                                               $"Please make sure to have your trainer code entered correctly, check #faq for the formatting.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} - Seems like your timezone was not entered correctly.\n" +
                                                           $"Please make sure to have your time zone entered correctly, check #faq for the formatting.");
                }
            }
        }

        [Command("time"), Summary("Check the time of another player.")]
        public async Task timezone([Remainder]SocketGuildUser user)
        {
            if(user != null)
            {
                if (!DbCommands.dupParticipant(user.Id))
                {
                    await Context.Channel.SendMessageAsync($":x: Doesn't look like {user.Username} has joined the tournament.");
                }
                else
                {
                    string targetTimeZone = DbCommands.timezoneLookup(user.Id);
                    DateTime dateTime = new DateTime();
                    dateTime = DateTime.UtcNow;
                    Console.WriteLine(dateTime.ToString("hh:mm tt"));

                    if (targetTimeZone.StartsWith("+"))
                    {
                        //Break part the time zone the user submitted
                        targetTimeZone.Remove(0);
                        string[] times = targetTimeZone.Split(":");
                        int hours = int.Parse(times[0]);
                        int minutes = int.Parse(times[1]);
                        //Calculate current time for the user
                        TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);

                        await Context.Channel.SendMessageAsync($"It is currently ~{dateTime.Add(timeSpan).ToString("hh:mm tt")} (+/- an hour for DST) " +
                                                               $"for {user.Username}, if that's an appropriate time, ping them (@) to see if they're available.");
                    }
                    else
                    {
                        //Break part the time zone the user submitted
                        targetTimeZone.Remove(0);
                        string[] times = targetTimeZone.Split(":");
                        int hours = int.Parse(times[0]);
                        int minutes = int.Parse(times[1]);
                        Console.WriteLine($"{hours} - {minutes}");
                        //Calculate current time for the user
                        TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);

                        await Context.Channel.SendMessageAsync($"It is currently ~{dateTime.Add(timeSpan).ToString("hh:mm tt")} (+/- an hour for DST) " +
                                                               $"for {user.Username}, if that's an appropriate time, ping them (@) to see if they're available.");
                    }
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync($":x: {Context.Message.Author.Mention} - You must specify another user to lookup their time zone.");
            }
        }

        [Command("makeTournament"), Summary("Create a tournament.")]
        public async Task makeTournament([Remainder]string input = "")
        {
            int roomSize = int.Parse(input);
            if(Context.Message.Author.Id == Convert.ToUInt64("267871632536764416"))
            {
                await Context.Guild.DownloadUsersAsync();
                await Context.Channel.SendMessageAsync("Atticus is creating a tournament ..");
                if(DbCommands.createTournament(roomSize))
                {
                    int numOfRooms = DbCommands.getNumberOfRooms();
                    await Context.Channel.SendMessageAsync($"Created {numOfRooms} rooms with ~{roomSize} people inside.");
                    for(int w = 0; w < numOfRooms; w++)
                    {
                        int players = 0;
                        await Context.Channel.SendMessageAsync($"Filling Room {w + 1} with it's players");
                        List<ulong> participants = DbCommands.getPartsFromRoom(w);
                        for (int x = 0; x < participants.Count; x++)
                        {
                            var tempUser = Context.Guild.GetUser(participants[x]);
                            string roleName = "Battle Group " + (w + 1);
                            SocketRole role = Context.Guild.Roles.FirstOrDefault(z => z.Name == roleName);
                            await (tempUser as SocketGuildUser).AddRoleAsync(role);
                            players++;
                        }
                        await Context.Channel.SendMessageAsync($"Room {w + 1} now has {players} players ready to go!");
                        string channelName = "battle-room-" + (w + 1);
                        var tempChannel = Context.Guild.Channels.FirstOrDefault(z => z.Name == channelName).Id;
                        List<string> codes = DbCommands.getTrainerCodes(w);
                        string roomMessage = "```Welcome to the tournament!```\n";
                        for(int p = 0; p < participants.Count; p++)
                        {
                            roomMessage += $"<@{participants[p].ToString()}> - {codes[p]}\n";
                        }
                        roomMessage += "From this point, add everyone onto your friends list and start the exchange of gifts. I highly recommend " +
                                       "giving each person in this list the same nickname in Pokemon GO and searching for that nickname when you want " +
                                       "to send gifts that day. The @Tournament_Mods will be distributing the check in codes for thier rooms once " +
                                       "we get those set up on the Silph.gg website.\n" +
                                       "@Atticus.Nair will manually assign the Mod for each room, if a Mod is participating in that room, then they " +
                                       "are NOT allowed to be the staff representative for the room.\n" +
                                       "One thing that was special about the first tournament here was that each room is sorted by your Silph Rank and then " +
                                       "your time zone. Therefore working with other players this go around could be easier because the time zones are sorted" +
                                       "to have you close to someone in terms of time difference.\n";
                        await Context.Guild.GetTextChannel(tempChannel).SendMessageAsync(roomMessage);
                        roomMessage = "As this is our first attempt at this, there possibly could have been some bugs here, but I hope that everything worked " +
                                       "correctly. Once we get to the stage of battling, I did set up a `!time` command to help figure out the time it is (approx) " +
                                       "for your opponent. Simply do !time <username> and it will calculate the time it is for them. Becuase of Day Light Savings " +
                                       "it is next to impossible to calculate the exact time without asking each person WAY more information than I wanted to. " +
                                       "So keep in mind this is an approximation on their time.\n" +
                                       "IF there is any commands that you guys think would be helpful, please let me know and I would be happy think try and " +
                                       "implement them before everything get's fully started. I believe that should be it for now, make sure you add everyone in " +
                                       "this lobby and be sure to keep up on the gifts.";
                        await Context.Guild.GetTextChannel(tempChannel).SendMessageAsync(roomMessage);
                    }                    
                    await Context.Channel.SendMessageAsync("Tournament was created successfully.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Something broke ..");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("Pardon, only the Main Man can start a tournament.");
            }
        }

        [Command("rank"), Summary("idk")]
        public async Task rank([Remainder]string input = "")
        {
            ;
        }
    }
}