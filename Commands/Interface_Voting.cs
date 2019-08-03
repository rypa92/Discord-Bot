using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using DiscordBot.Core.Commands;
using System.Globalization;

namespace DiscordBot.Core.Commands
{
    public class VotingInterface : ModuleBase<SocketCommandContext>
    {
        VoteCommands DbCommands = new VoteCommands();
        static TextInfo properText = new CultureInfo("en-US", false).TextInfo;

        [Command("hello"), Summary("Hello World!")]
        public async Task hello()
        {
            await Context.Channel.SendMessageAsync("Psst, don't tell anyone about this ..");
        }

        [Command("vote"), Summary("Vote for a Deep Dive.")]
        public async Task vote([Remainder]string input = "")
        {
            if(input == null || input == "")
            {
                await Context.Channel.SendMessageAsync(":x: Please make sure to include a valid Pokemon. Please see `#faq` for instructions.");
                return;
            }

            List<string> canidates = DbCommands.PokemonExists(input);
            int addResult = -1;
            bool completed = DbCommands.CheckForComplete(input);

            if(completed)
            {
                await Context.Channel.SendMessageAsync(":x: Wait a sec, it looks like you entered `" + input + "`\n" +
                                                       "which was already mark completed. Check YouTube to see if it's uploaded.\n" +
                                                       "If not, it may be coming in the next day or so!");
            }

            if (canidates.Count == 1)
            {
                input = canidates[0];
                addResult = DbCommands.AddVote(Context.User.Id, input);
            }
            else if (canidates.Count < 1)
            {
                await Context.Channel.SendMessageAsync(":x: Wait a sec, it looks like you entered `" + input + "`\n" +
                                                       "but I couldn't find that Pokemon. Are you sure it's availble this month?");
            }
            else if (canidates.Count > 1)
            {
                string allCan = "";
                for (int x = 0; x < canidates.Count; x++)
                {
                    if(canidates[x].Equals(input))
                    {
                        input = canidates[x];
                        addResult = DbCommands.AddVote(Context.User.Id, input);
                        break;
                    }
                    else
                    {
                        allCan += canidates[x] + "\n";
                    }
                }
                if(!allCan.Equals(""))
                {
                    await Context.Channel.SendMessageAsync(":x: Woah, you entered `" + input + "` but I found a few pokemon matching that name:```\n" +
                                                                           allCan + "```\nTry to enter the vote again and match one of these.");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(":x: I have no idea how we ended up here. Something went **REALLY** wrong.. @Atticus.Nair#1120");
            }


            if (addResult == 0 && !completed)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithAuthor("Deep Dive Bot", "https://img.pokemondb.net/sprites/omega-ruby-alpha-sapphire/dex/normal/yanma.png");
                Embed.WithColor(0,255,255);
                Embed.AddField("Your vote was cast for: ", input);

                //Form validation for image
                input = input.Replace("alola", "alolan").Replace(" ", "-").Replace("(", "").Replace(")", "").Replace("'", "").ToLower();
                if (input.Contains("castform"))
                {
                    input = "castform";
                }

                Embed.WithThumbnailUrl($"https://img.pokemondb.net/sprites/sun-moon/icon/{input}.png");
                
                await Context.Channel.SendMessageAsync("", false, Embed.Build());
            }            
        }

        [Command("results"), Summary("View top 5 results.")]
        public async Task results()
        {
            List<string> results = DbCommands.GetTopFive();
            string display = "The top five requested Pokemon:\n\n";
            for(int x = 0; x < 5; x++)
            {
                display += $"{x + 1} - {results[x + 1]}\n";
            }

            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithAuthor("Deep Dive Bot", "https://img.pokemondb.net/sprites/omega-ruby-alpha-sapphire/dex/normal/yanma.png");
            Embed.WithColor(0, 255, 255);
            Embed.WithDescription(display);

            await Context.Channel.SendMessageAsync("", false, Embed.Build());
        }

        [Command("complete"), Summary("Mark Deep Dive as completed.")]
        public async Task complete([Remainder]string input = "None")
        {
            if (!(Context.User.Id == 267871632536764416))
            {
                await Context.Channel.SendMessageAsync(":x: You are not the bot moderator!");
                return;
            }
            else
            {
                if(DbCommands.MarkComplete(properText.ToTitleCase(input)))
                {
                    await Context.Channel.SendMessageAsync(input + " has been marked complete!");
                }
                else if(DbCommands.CheckForComplete(properText.ToTitleCase(input)))
                {
                    await Context.Channel.SendMessageAsync(input + " wasn completed previously");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(input + " wasn't found ..");
                }
            }
        }

        [Command("rank"), Summary("I can't answer those questions ..")]
        public async Task rank()
        {
            
        }
    }
}