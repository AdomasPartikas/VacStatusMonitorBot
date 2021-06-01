using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VacStatus.Functionality;

namespace VacStatus.Commands
{
    class SteamCommands : BaseCommandModule
    {
        //Baselainas kaip veikia zinuciu siuntimas ir atsakymas
        /* 
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            //Atsiranda taskiukai rasymo discorde
            await ctx.TriggerTypingAsync();
            
            //Atsakymas i boto iskvietima
            await ctx.RespondAsync($"Pong! Ping: {ctx.Client.Ping}ms");

            //Sends some sort of message
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }
        */

        [Command("watch")]
        [Description("Isduoda rasta informacija ir ideda profili i duombaze.")]
        public async Task Watch(CommandContext ctx, [Description("Pilnas url (https://....) naudotojo kuri norit ideti i duombaze")] string url)
        {
            await ctx.TriggerTypingAsync();

            var steamFunc = new SteamFunctions();
            var result = steamFunc.MainInfoAndPlayerAdd(url);

            await ctx.Channel.SendMessageAsync(result.Result.Item1).ConfigureAwait(false);
            if(result.Result.Item2)
                await ctx.Channel.SendMessageAsync("I'll continue to **monitor** them :yum:").ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync("This user is **already being monitored**, no need in adding them twice :smile:").ConfigureAwait(false);
        }
    }
}
