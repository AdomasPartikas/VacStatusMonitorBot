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
        [Description("Istrina visa watchlist sarasa.")]
        public async Task ClearList(CommandContext ctx)
        {
            //Zmogus is pradziu parses zinute
            var user = ctx.User;
            var client = ctx.Client;
            var interactivity = client.GetInteractivity();

            var roleEmbed = new DiscordEmbedBuilder
            {
                Title = "Ar esate isitikines?",
                Description = "Sutikdami istrinsite visa sarasa stebimu zmoniu.",
                Color = DiscordColor.Red
            };

            var msg = await ctx.Channel.SendMessageAsync(embed: roleEmbed).ConfigureAwait(false);

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
                if (result.Result.Emoji == confirm)
                {

                    var sql = new VacStatus.Functionality.MySql();
                    var currUsers = sql.CurrentPlayerCountInDatabase(true);
                    sql.ClearList();

                    var deletionEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Istrinti {currUsers} zmones",
                        Color = DiscordColor.Red
                    };

                    await ctx.Channel.SendMessageAsync(embed: deletionEmbed).ConfigureAwait(false);
                }

                if (result.Result.Emoji == cancel)
                {
                    var deletionEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Aborted",
                        Color = DiscordColor.Blue
                    };
                    await ctx.Channel.SendMessageAsync(embed: deletionEmbed).ConfigureAwait(false);
                }
            }
        }
    }
}
