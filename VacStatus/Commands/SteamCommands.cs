using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
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
        private static bool monitorActive = false;

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

            GC.Collect();
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
            int currCount = steamFunc.CurrentSuspectCount(false);


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

            GC.Collect();
        }

        [Command("watchlist")]
        [Description("Gives a list of current watched people")]
        public async Task WatchList(CommandContext ctx)
        {
            log.Log($"[{ctx.Member}] Panaudota 'Watchlist' komanda.", Logger.LogType.Info);

            await ctx.TriggerTypingAsync();

            var steamFunc = new SteamFunctions();

            await ctx.Channel.SendMessageAsync(steamFunc.Watchlist());

            GC.Collect();
        }

        [Command("monitor")]
        [Description("Skanuoja specifiniu intervalu visus zmones duombazeje " +
            "Kas 30min, is duombazes yra istraukiami nebaninti zmones ir patikrinami ar gavo banus. " +
            "Parasai viena karta isijungia, kita karta issijungia.")]
        public async Task Monitor(CommandContext ctx)
        {
            log.Log($"[{ctx.Member}] Panaudota 'Monitor' komanda.", Logger.LogType.Info);

            //Sukuriam trys kintamuosius, vienas laikys starto laika, kitas laikys kito recheck laika ir trecias intervalo ilgi
            var aTimer = new TimeSpan();
            var bTimer = new TimeSpan();
            TimeSpan interval = TimeSpan.FromMinutes(30);
            aTimer = DateTime.Now.TimeOfDay;
            bTimer = aTimer.Add(interval);

            //Jeigu monitor mode isjungtas ir pakvieciama si komanda tai monitor mode turetu isijungti, tam sitas if
            if (!monitorActive)
            {
                //Padarom ji aktyvu
                monitorActive = true;

                //Issiunciam pirma zinute
                var message = await ctx.Channel.SendMessageAsync($"**Monitor mode:** `Active`\n" +
                    $"**Next Scan:** `{string.Format("{0:00}:{1:00}", bTimer.Hours, bTimer.Minutes)}`").ConfigureAwait(false);

                //Uzkuriam cikla
                while (monitorActive)
                {
                    //Sukuriam steam funkcijas ir paprasom saraso zmoniu
                    var steamFunc = new SteamFunctions();
                    var list = steamFunc.Recheck(false);

                    //Issiunciam default zinute tikrinimo startui parodyt
                    await message.ModifyAsync(msg => msg.Content = $"**Monitor mode:** `Active`\n" +
                    $"**Next Scan:** `Now`").ConfigureAwait(false);

                    //Paprasom visu esanciu zmoniu databeise skaiciu, taip pat susikuriam kintamaji kuris skaiciuos kelintas eileje sis zmogus yra
                    var currCount = steamFunc.CurrentSuspectCount(false);
                    int count = 0;

                    //Kiekvienam zmogui esanciam databeise ciklas
                    foreach (var item in list)
                    {
                        count++;

                        //Zinute su zmogaus vardu, kelintas jis sarase
                        await message.ModifyAsync(msg => msg.Content = $"**Monitor mode:** `Active`\n" +
                        $"**Next Scan:** `Now`\n" +
                        $"**Rechecking..**  `[{item.Nickname}]`  **[{count}/{currCount}]**").ConfigureAwait(false);


                        //Patikriname ar tikrinamo accounto nickname nepasikeite, jei pasikeite pakeiskime duombazeje i dabar esanti
                        await steamFunc.VerifyNicknameChange(item);
                        //Jeigu gavo bana israsykime sita zinute, kad sis zmogus dabar yra uzbanintas
                        if (await steamFunc.DidVacStatusChangeAsync(item.SteamId))
                        {
                            //Ban zinute
                            var currTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            await ctx.Channel.SendMessageAsync($"> **[{currTime}]**\n" +
                                                                $"> **SteamId:**  `{item.SteamId}`\n" +
                                                                 $"> **Nickname:**  `{item.Nickname}`\n" +
                                                                  "> Has been  **Banned**  from official matchmaking.").ConfigureAwait(false);
                        }
                    }

                    //Po patikrinimo turim suzinoti kito patikrinimo laika, tai dabartini laika pasirasome i aTimer pridedam intervala ir gaunam bTimer
                    aTimer = DateTime.Now.TimeOfDay;
                    bTimer = aTimer.Add(interval);

                    //Zinute su kuri pasako kada kitas recheckas
                    await message.ModifyAsync(msg => msg.Content = $"**Monitor mode:** `Active`\n" +
                    $"**Next Scan:** `{string.Format("{0:00}:{1:00}", bTimer.Hours, bTimer.Minutes)}`").ConfigureAwait(false);

                    //Uzsaldome taska intervalo laikui
                    await Task.Delay(Convert.ToInt32(interval.TotalMilliseconds));
                }
            }
            else if (monitorActive)
            {
                //Jei monitor mode buvo aktivuotas ir kazkas parase komanda, tada isjunkime monitor mode
                monitorActive = false;
                await ctx.Channel.SendMessageAsync($"``Monitor mode:`` **Disabled**").ConfigureAwait(false);
            }


            GC.Collect();
        }

        //Easter egg-as, taip pat stebėjimo funkcija
        [Command("vasya")]
        [Description("gaudo čyterius mano cs mače.")]
        public async Task Vasya(CommandContext ctx, [Description("Pilnas url (https://....) naudotojo kuri norit ideti i duombaze")] string url)
        {
            //Loginama informacija
            log.Log($"[{ctx.Member}] Rastas easter egg.", Logger.LogType.Info);

            await ctx.TriggerTypingAsync();

            var steamFunc = new SteamFunctions();
            var result = steamFunc.GetRajonas(url);

            //Išsiunčia turima info apie žaidėja
            await ctx.Channel.SendMessageAsync(result.Result).ConfigureAwait(false);

            //bool rezultatas, kuris tikrina ar jau patikrintas žaidėjas
            
            await ctx.Channel.SendMessageAsync("Vasya toliau stebės").ConfigureAwait(false);
        }

        [Command("check")]
        public async Task Check(CommandContext ctx, string indexToCheck)
        {
            log.Log($"[{ctx.Member}] Panaudota 'Check' komanda.", Logger.LogType.Info);

            await ctx.TriggerTypingAsync();
            //Patikrina žmogų nepridedant jo į watchlistą
            var steamFunc = new SteamFunctions();
            var result = await steamFunc.Check(indexToCheck);

            await ctx.Channel.SendMessageAsync(result).ConfigureAwait(false);

        }

        [Command("remove")]
        [Description("Removes a player from watchlist")]
        public async Task Remove(CommandContext ctx, int indexToRemove)
        {
            log.Log($"[{ctx.Member}] Panaudota 'Remove' komanda", Logger.LogType.Info);

            await ctx.TriggerTypingAsync();
            //Panaikina žmogų iš watchlisto
            var steamFunc = new SteamFunctions();
            var result = await steamFunc.Remove(indexToRemove);

            await ctx.Channel.SendMessageAsync(result).ConfigureAwait(false);
        }
        
        [Command("respond")]
        [Description("Botas atrašo tą patį, ką parašė useris")]
        public async Task Response(CommandContext ctx)
        {
            log.Log($"[{ctx.Member}] Panaudota 'response' komanda", Logger.LogType.Info);
            var interaktyvumas = ctx.Client.GetInteractivity();

            //Sulaukus komandos botas atrašo tą patį kas buvo parašytą į chatą
            var žinutė = await interaktyvumas.WaitForMessageAsync(m => m.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(žinutė.Result.Content);
        }

        [Command("respondReaction")]
        [Description("Botas atrašo tą patį, ką parašė useris")]
        public async Task Respondreaction(CommandContext ctx)
        {
            log.Log($"[{ctx.Member}] Panaudota 'respondReaction' komanda", Logger.LogType.Info);
            var interaktyvumas = ctx.Client.GetInteractivity();
            //Nuskaito kokia emoji naudota kaip rekcija žinutės ir ją parašo į chatą
            var žinutė = await interaktyvumas.WaitForReactionAsync(m => m.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(žinutė.Result.Emoji);
        }

        [Command("role")]
        public async Task Role(CommandContext ctx)
        {
            log.Log($"[{ctx.Member}] Panaudota 'role' komanda", Logger.LogType.Info);
            //Sukuria discord embedą
            var roleEmbed = new DiscordEmbedBuilder
            {
                Title = "Paspauskite :thumbsup: , kad gauti rolę",
                Color = DiscordColor.Blue
            };

            var roleŽinutė = await ctx.Channel.SendMessageAsync(embed: roleEmbed).ConfigureAwait(false);
            //Parenkamos naudojamos emoji
            var thumbsUp = DiscordEmoji.FromName(ctx.Client, ":+1:");
                var thumbsDown = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await roleŽinutė.CreateReactionAsync(thumbsUp).ConfigureAwait(false);
            await roleŽinutė.CreateReactionAsync(thumbsDown).ConfigureAwait(false);

            var interaktyvumas = ctx.Client.GetInteractivity();

            var reakcijaResult = await interaktyvumas.WaitForReactionAsync(
                m => m.Message == roleŽinutė &&
                m.User== ctx.User &&
                m.Emoji == thumbsUp || m.Emoji == thumbsDown).ConfigureAwait(false);
            //Prideda rolę jeigu sureaguoja su thumbs up emoji
            if(reakcijaResult.Result.Emoji == thumbsUp)
            {
                var role = ctx.Guild.GetRole(852234253525712937);
                await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
            }
            
           
        }
    }
}
