using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VacStatus.Functionality;
using VacStatus.Local;

namespace VacStatus.Commands
{
    class SteamCommands : BaseCommandModule
    {
        Logger log = new Logger();
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


        //Stebejimo funkcija
        [Command("watch")]
        [Description("Isduoda rasta informacija ir ideda profili i duombaze.")]
        public async Task Watch(CommandContext ctx, [Description("Pilnas url (https://....) naudotojo kuri norit ideti i duombaze")] string url)
        {
            log.Log($"[{ctx.Member}] Panaudota 'Watch' komanda.", Logger.LogType.Info);

            await ctx.TriggerTypingAsync();

            var steamFunc = new SteamFunctions();
            var result = steamFunc.MainInfoAndPlayerAdd(url);

            //Issiunciame pirma rezultata kuris yra esanti informacija apie zaideja
            await ctx.Channel.SendMessageAsync(result.Result.Item1).ConfigureAwait(false);

            //Antras rezultatas yra bool tipo, jame yra pasakyta ar zmogus yra duombazeje ar ne
            if (result.Result.Item2)
                await ctx.Channel.SendMessageAsync("I'll continue to **monitor** them :yum:").ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync("This user is **already being monitored**, no need in adding them twice :smile:").ConfigureAwait(false);
        }

        //Perpatikrinimo funkcija
        [Command("recheck")]
        [Description("Patikrina esancius akountus duombazeje.")]
        public async Task Recheck(CommandContext ctx)
        {
            log.Log($"[{ctx.Member}] Panaudota 'Recheck' komanda.", Logger.LogType.Info);

            await ctx.TriggerTypingAsync();

            var steamFunc = new SteamFunctions();
            var recheckResult = steamFunc.Recheck(false);

            //Issiunciame default zinute kuria modifikuosime
            var message = await ctx.Channel.SendMessageAsync("`[0/0]` **Loading**").ConfigureAwait(false);

            //Sukuriame laiko string, count int kuris skaiciuos kelintas cia zmogus kuri nuskaitome
            int count = 0;
            string currTime = string.Empty;

            //CurrentSuspectCount funkcija kuri suskaiciuoja kiek yra nebanintu zmoniu duombazeje
            int currCount = steamFunc.CurrentSuspectCount();


            foreach (var item in recheckResult)
            {
                //Esamas laikas
                currTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                count++;

                //Modifikuojame default zinute, taip sukuriam effecta kad zinute juda, sioje modifikuotoje zinute idedam zmoniu vardus, skaiciu eileje ir visu zmoniu duombazeje skaiciu
                await message.ModifyAsync(msg => msg.Content = $"**Rechecking..**  `[{item.Nickname}]`  **[{count}/{currCount}]**").ConfigureAwait(false);

                //Patikriname ar tikrinamo accounto nickname nepasikeite, jei pasikeite pakeiskime duombazeje i dabar esanti
                await steamFunc.VerifyNicknameChange(item);

                //Jeigu gavo bana israsykime sita zinute, kad sis zmogus dabar yra uzbanintas
                if (await steamFunc.DidVacStatusChangeAsync(item.SteamId))
                {
                    await ctx.Channel.SendMessageAsync($"> **[{currTime}]**\n" +
                                                        $"> **SteamId:**  `{item.SteamId}`\n" +
                                                         $"> **Nickname:**  `{item.Nickname}`\n" +
                                                          "> Has been  **Banned**  from official matchmaking.").ConfigureAwait(false);
                }
            }

            //Pabaigos zinute
            await ctx.Channel.SendMessageAsync($"`Current user count`  **[{currCount}]**\nI am **finished!** :smiley:").ConfigureAwait(false);
        }

        [Command("Rajoninis")]
        [Description("gaudo čyterius mano cs mače.")]
        public async Task Rajoninis(CommandContext ctx, [Description("Pilnas url (https://....) naudotojo kuri norit ideti i duombaze")] string url)
        {
            log.Log($"[{ctx.Member}] Rastas easter egg.", Logger.LogType.Info);

            await ctx.TriggerTypingAsync();

            var steamFunc = new SteamFunctions();
            var result = steamFunc.MainInfoAndPlayerAdd(url);

            //Issiunciame pirma rezultata kuris yra esanti informacija apie zaideja
            await ctx.Channel.SendMessageAsync(result.Result.Item1).ConfigureAwait(false);

            //Antras rezultatas yra bool tipo, jame yra pasakyta ar zmogus yra duombazeje ar ne
            if (result.Result.Item2)
                await ctx.Channel.SendMessageAsync("Vasya toliau stebės").ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync("A durns? jau tikrinai sita ciuveli").ConfigureAwait(false);
        }
    }
}
