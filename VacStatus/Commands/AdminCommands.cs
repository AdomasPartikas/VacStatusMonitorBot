using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using VacStatus.Local;

namespace VacStatus.Commands
{

    //Komandos kurias gali pasaukti tik turinti administratoriaus galias
    [RequirePermissions(Permissions.Administrator)]
    class AdminCommands : BaseCommandModule
    {
        Logger log = new Logger();

        [Command("clearlist")]
        [Description("Ištrina visą watchlist sąrašą.")]
        public async Task ClearList(CommandContext ctx)
        {
            log.Log($"[{ctx.Member}] Iskviete 'ClearList' komanda.", Logger.LogType.Info);

            //Saugomas žmogus kuris iškvietė komandą
            var user = ctx.User;
            //Kliento interactivity module sukuriamas
            var client = ctx.Client;
            var interactivity = client.GetInteractivity();

            //Sukuriamas embed ir jam duodami properties
            var warningEmbed = new DiscordEmbedBuilder
            {
                Title = "Ar esate įsitikinęs?",
                Description = "Sutikdami ištrinsite visą sąraša stebimų žmonių.",
                Color = DiscordColor.Red
            };

            //Isiunciamas embed ir issaugomas i kintamaji kad turetu konteksta veliau
            var msg = await ctx.Channel.SendMessageAsync(embed: warningEmbed).ConfigureAwait(false);

            //Emoji naudojami kaip žinutės reakcija
            var confirm = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var cancel = DiscordEmoji.FromName(ctx.Client, ":x:");

            //Sukuriami emoji
            await msg.CreateReactionAsync(confirm).ConfigureAwait(false);
            await msg.CreateReactionAsync(cancel).ConfigureAwait(false);

            //Palaukiama
            await Task.Delay(1000);

            //Kintamasis kuris nusakyk kiek laiko lauks response is userio
            var timeout = TimeSpan.FromSeconds(5);

            //Sukuriamas interactivity module uzklausa ir jai duodami nustatymai
            var result = await interactivity.WaitForReactionAsync(x => 
                x.Message == msg &&
                x.User == user &&
                x.Emoji == confirm || 
                x.Emoji == cancel, timeoutoverride: timeout).ConfigureAwait(false);

            await Task.Delay(1000);

            //Jei laikas iseikvotas is "timeout" issiunciama sita zinute
            if(result.TimedOut)
            {
                var deletionEmbed = new DiscordEmbedBuilder
                {
                    Title = "Timed Out",
                    Color = DiscordColor.Orange
                };
                log.Log($"'ClearList' atsaukta: timed out", Logger.LogType.Info);
                log.Log($" ", Logger.LogType.Info);
                await ctx.Channel.SendMessageAsync(embed: deletionEmbed).ConfigureAwait(false);
            }

            //Jei dar laikas neiseikvotas
            if (!result.TimedOut)
            {
                //Funkcija tesiama jei paspaudziamas sis emoji
                if (result.Result.Emoji == confirm)
                {

                    var sql = new VacStatus.Functionality.MySql();
                    var currUsers = sql.CurrentPlayerCountInDatabase(true);
                    sql.ClearList();

                    var deletionEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Ištrinti {currUsers} žmonės",
                        Color = DiscordColor.Red
                    };

                    await ctx.Channel.SendMessageAsync(embed: deletionEmbed).ConfigureAwait(false);
                }
                //Funkcija atsaukiama jei paspaudziamas sis emoji
                if (result.Result.Emoji == cancel)
                {
                    var deletionEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Atšaukta",
                        Color = DiscordColor.Orange
                    };
                    log.Log($"'ClearList' atsaukta: naudotojas", Logger.LogType.Info);
                    log.Log($"------------------END------------------", Logger.LogType.Info);

                    await ctx.Channel.SendMessageAsync(embed: deletionEmbed).ConfigureAwait(false);
                }
            }
        }
    }
}
