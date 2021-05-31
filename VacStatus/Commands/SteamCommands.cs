using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
    }
}
