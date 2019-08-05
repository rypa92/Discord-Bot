using System.Collections.Generic;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Globalization;
using System;

namespace DiscordBot.Core.Commands
{
    public class Interface_PokemonInfo : ModuleBase<SocketCommandContext>
    {
        PokemonInfo DbCommands = new PokemonInfo();
        static TextInfo properText = new CultureInfo("en-US", false).TextInfo;

        [Command("info"), Summary("Get info on a specific Pokemon")]
        public async Task info([Remainder]string input = "")
        {
            List<string> canidates = DbCommands.PokemonExists(input);

            if (canidates.Count == 1)
            {
                input = canidates[0];
                List<string> types = DbCommands.getTypes(input);
                List<string> stats = DbCommands.getStats(input);
                List<string> ranks = DbCommands.getRanks(input);
                List<string> fastMoves = DbCommands.getFastMoves(input);
                List<string> chargeMoves = DbCommands.getChargeMoves(input);

                EmbedBuilder results = makeEmbed(input, types, stats, ranks, fastMoves, chargeMoves);
                await Context.Channel.SendMessageAsync("", false, results.Build());
            }
            else if (canidates.Count < 1)
            {
                await Context.Channel.SendMessageAsync(":x: Wait a sec, it looks like you entered `" + input + "` but I couldn't find that Pokemon.");
            }
            else if (canidates.Count > 1)
            {
                string allCan = "";
                for (int x = 0; x < canidates.Count; x++)
                {
                    if (canidates[x].Equals(input))
                    {
                        input = canidates[x];
                        List<string> types = DbCommands.getTypes(input);
                        List<string> stats = DbCommands.getStats(input);
                        List<string> ranks = DbCommands.getRanks(input);
                        List<string> fastMoves = DbCommands.getFastMoves(input);
                        List<string> chargeMoves = DbCommands.getChargeMoves(input);
                        
                        EmbedBuilder results = makeEmbed(input, types, stats, ranks, fastMoves, chargeMoves);
                        await Context.Channel.SendMessageAsync("", false, results.Build());
                        break;
                    }
                    else
                    {
                        allCan += canidates[x] + "\n";
                    }
                }
                if (!allCan.Equals(""))
                {
                    await Context.Channel.SendMessageAsync(":x: Woah, you entered `" + input + "` but I found a few pokemon matching that name:```\n" +
                                                                           allCan + "```\nTry again with one of these.");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(":x: I have no idea how we ended up here. Something went **REALLY** wrong.. @Atticus.Nair#1120");
            }
        }

        public EmbedBuilder makeEmbed(string name, List<string> types, List<string> stats, List<string> ranks, List<string> fastMoves, List<string> chargeMoves)
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithAuthor("Deep Dive Bot", "https://img.pokemondb.net/sprites/omega-ruby-alpha-sapphire/dex/normal/yanma.png");
            Embed.WithColor(0, 255, 255);
            Embed.AddField("Here is some info on: ", properText.ToTitleCase(name));
            name = properText.ToLower(name).Replace("alola", "alolan").Replace(" ", "-").Replace("(", "").Replace(")", "").Replace("'", "").ToLower();
            if (name.Contains("castform"))
            {
                name = "castform";
            }
            Embed.WithThumbnailUrl($"https://img.pokemondb.net/sprites/sun-moon/icon/{properText.ToLower(name)}.png");
            Embed.AddField("Types: ", $"{types[0]} | {types[1]}");
            Embed.AddField("Base Stats: ", $"ATK: {stats[0]} | DEF: {stats[1]} | STA: {stats[2]}");
            Embed.AddField("Fast Moves: ", $"{fastMoves.Count/4}");
            for(int x = 0; x <= fastMoves.Count; x += 4)
            {
                if(fastMoves.Count <= x)
                {
                    break;
                }
                else
                {
                    Embed.AddField($"{fastMoves[x]}: ", $"EPT: **{fastMoves[x + 1]}** | DPT: **{fastMoves[x + 2]}** | DPT (STAB): **{fastMoves[x + 3]}**");
                }
            }
            Embed.AddField("Charge Moves: ", $"{chargeMoves.Count / 6}");
            for (int x = 0; x <= chargeMoves.Count; x += 6)
            {
                if (chargeMoves.Count <= x)
                {
                    break;
                }
                else
                {
                    Embed.AddField($"{chargeMoves[x]}: ", $"ENG: **{chargeMoves[x + 1]}** | DMG: **{chargeMoves[x + 2]}** | DPE: **{chargeMoves[x + 3]}** | DMG (STAB): **{chargeMoves[x + 4]}** | DPE (STAB): **{chargeMoves[x + 5]}**");
                }
            }
            return Embed;
        }
    }
}
