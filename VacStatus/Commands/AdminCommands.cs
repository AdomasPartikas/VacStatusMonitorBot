using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using VacStatus.Functionality;

namespace VacStatus.Commands
{
    //Komandos kurias gali pasaukti tik turinti administratoriaus galias
    [RequirePermissions(Permissions.Administrator)]
    class AdminCommands : BaseCommandModule
    {
        [Command("clearlist")]
        [Description("Ištrina visą watchlist sąrašą.")]
        public async Task ClearList(CommandContext ctx)
        {
            //Saugomas žmogus kuris iškvietė komandą
            var user = ctx.User;
            var client = ctx.Client;
            var interactivity = client.GetInteractivity();

            var roleEmbed = new DiscordEmbedBuilder
            {
                Title = "Ar esate įsitikinęs?",
                Description = "Sutikdami ištrinsite visą sąraša stebimų žmonių.",
                Color = DiscordColor.Red
            };

            var msg = await ctx.Channel.SendMessageAsync(embed: roleEmbed).ConfigureAwait(false);

            //Emoji naudojami kaip žinutės reakcija
            var confirm = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var cancel = DiscordEmoji.FromName(ctx.Client, ":x:");

            await msg.CreateReactionAsync(confirm).ConfigureAwait(false);
            await msg.CreateReactionAsync(cancel).ConfigureAwait(false);

            await Task.Delay(1000);

            var timeout = TimeSpan.FromSeconds(5);

            var result = await interactivity.WaitForReactionAsync(x => 
                x.Message == msg &&
                x.User == user &&
                x.Emoji == confirm || 
                x.Emoji == cancel, timeoutoverride: timeout).ConfigureAwait(false);

            await Task.Delay(1000);

            //Laikmatis, skaičiuojantis kiek laiko liko atsakyti
            if(result.TimedOut)
            {
                var deletionEmbed = new DiscordEmbedBuilder
                {
                    Title = "Timed Out",
                    Color = DiscordColor.Orange
                };
                await ctx.Channel.SendMessageAsync(embed: deletionEmbed).ConfigureAwait(false);
            }

            if (!result.TimedOut)
            {
                //Jeigu paspausta nykštys aukštyn, tada visi žmonės ištrinami iš watchlisto
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
                //Jeigu paspausta nykštys žemyn, tada viskas atšaukiama
                if (result.Result.Emoji == cancel)
                {
                    var deletionEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Atšaukta",
                        Color = DiscordColor.Blue
                    };
                    await ctx.Channel.SendMessageAsync(embed: deletionEmbed).ConfigureAwait(false);
                }
            }
        }
    }
}
