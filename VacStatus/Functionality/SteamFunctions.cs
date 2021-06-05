using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamWebAPI2;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using VacStatus.Local;

namespace VacStatus.Functionality
{
    class SteamFunctions
    {
        Logger log = new Logger();

        public async Task<(string, bool)> MainInfoAndPlayerAdd(string url)
        {
            log.Log($"Pridedama paskyra", Logger.LogType.Info);

            //Url padarom i steamId
            var steamIdUlong = await UrlIntoUlongAsync(url);

            //Is steamId gaunam apibendrinima
            var summary = await GetSummary(steamIdUlong);

            //Apibendrinima duodam funkcijai kuri idetu i duombaze
            var sql = new MySql();
            var boolResult = sql.AddSuspect(summary);

            //Responsus visus, dideli stringa viena ir boola atiduodam prasytojui
            return (summary.Summary, boolResult);
        }

        public async Task<String> GetRajonas(string url)
        {
            var steamIdUlong = await UrlIntoUlongAsync(url);

            var result = GetSummary(steamIdUlong).Result;

            StringBuilder response = new StringBuilder();

            //Įrašoma gauta paviršutinė žaidėjo informacija
            response.Append($"**numesk nicka:** `{result.Nickname}`\n");

            if (result.RealName != null)
                response.Append($"**Varda turi?:** `{result.RealName}`\n");
            else
                response.Append($"**Varda turi?:** `Unknown`\n");


            response.Append($"**Miegi a ne?:** `{result.UserStatus}`\n" +
            $"**Numesk numeri:** `{result.SteamId}`\n");


            response.Append($"**saikos dalis nuo:** `{result.MemberSince}`\n");



            if (result.PlayingGameName != null)
                response.Append($"**Ka losi?:** `{result.PlayingGameName}`\n");
            else
                response.Append($"**Ka losi?:** `Nothing`\n");

            /*
            foreach (var item in communityProfileData.MostPlayedGames)
            {
                if (item.Name == playerSummaryData.PlayingGameName)
                {
                    //response.Append($"  **Game:** `{item.Name}`\n");
                    response.Append($"    **•Last two weeks:** `{item.HoursPlayed} hours`\n");
                    response.Append($"    **•Hours on record:** `{item.HoursOnRecord} hours`\n");
                }
            }
            */


            response.Append("**Ban status:**\n");


            //Įrašoma gauta informacija susijusi su žaidėjo banais
            if (result.LastBan > 0 || result.VacBanned || result.NumberOfGameBans > 0)
            {
                response.Append($"    **•Turi vacu?** `{result.VacBanned}`\n");
                response.Append($"    **•Suagutas lopas:** `ant {result.LastBan} dienu`\n");
                response.Append($"    **•Zaist ismok:** `{result.NumberOfGameBans}`\n");
                response.Append($"    **•pabratski kiek vacu?:** `{result.NumberOfVACBans}`\n");
                response.Append($"    **•Skameris a ne?:** `{result.TradeBanState}`\n");
            }
            else
            {
                response.Append($"    **•Turi vacu:** `No`\n");
                response.Append($"    **•Skameris a ne?:** `{result.TradeBanState}`\n");
            }
            return response.ToString();
        }

        public async Task<AccountSummary> GetSummary(ulong steamId)
        {
            //Yes
            var accSummary = new AccountSummary();


            //Nauja uzklausa internetui
            var webInterfaceFactory = new SteamWebInterfaceFactory(Configuration.jsonConfig.DevKey);
            var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());


            //Most of the accounts information is in player summary so we ask the api to give it to us
            var playerSummaryResponse = await steamInterface.GetPlayerSummaryAsync(steamId);
            var playerSummaryData = playerSummaryResponse.Data;

            //Getting community profile data
            var communityProfileData = await steamInterface.GetCommunityProfileAsync(steamId);

            //What (ban data)
            var playerBanResponse = await steamInterface.GetPlayerBansAsync(steamId);
            var playerBanData = playerBanResponse.Data;

            //Sudedame viska i grupe (AccountSummary faila Mantai) viska grazinsim prasytojui sios informacijos
            accSummary.SteamId = steamId.ToString();
            accSummary.Nickname = playerSummaryData.Nickname;
            accSummary.RealName = playerSummaryData.RealName;
            accSummary.PlayingGameName = playerSummaryData.PlayingGameName;
            accSummary.VacBanned = false;
            accSummary.LastBan = 0;
            accSummary.NumberOfGameBans = 0;
            accSummary.NumberOfVACBans = 0;
            accSummary.TradeBanState = "None";
            //-------

            //Creating an indebt summary if the asking party just wants a summary they can write out
            //Vertimas: kartais funkcijom nereikia detaliu bet jos nori viso summary vienu metu, tsg didelio teksto (pvz: watch komanda)
            //tai mes sukursim si dideli teksta cia ir idesim i accSummary.Summary kaip viska apibendrinus
            //tai darydami bus patenkintos visos uzklausos tik su viena funkcija, ji tampa universali

            StringBuilder response = new StringBuilder();

            response.Append($"**Nickname:** `{playerSummaryData.Nickname}`\n");

            if (playerSummaryData.RealName != null)
                response.Append($"**Real Name:** `{playerSummaryData.RealName}`\n");
            else
                response.Append($"**Real Name:** `Unknown`\n");


            response.Append($"**User Status:** `{playerSummaryData.UserStatus}`\n" +
            $"**SteamId:** `{steamId}`\n");
            accSummary.UserStatus = playerSummaryData.UserStatus.ToString();

            response.Append($"**Member since:** `{communityProfileData.MemberSince}`\n");

            accSummary.MemberSince = communityProfileData.MemberSince.ToString();

            if (playerSummaryData.PlayingGameName != null)
                response.Append($"**Currently Playing:** `{playerSummaryData.PlayingGameName}`\n");
            else
                response.Append($"**Currently Playing:** `Nothing`\n");


            foreach (var item in communityProfileData.MostPlayedGames)
            {
                if (item.Name == playerSummaryData.PlayingGameName)
                {
                    //response.Append($"  **Game:** `{item.Name}`\n");
                    response.Append($"    **•Last two weeks:** `{item.HoursPlayed} hours`\n");
                    response.Append($"    **•Hours on record:** `{item.HoursOnRecord} hours`\n");
                }
            }


            response.Append("**Ban status:**\n");


            foreach (var item in playerBanData)
            {
                if (item.DaysSinceLastBan > 0 || item.VACBanned || item.NumberOfGameBans > 0)
                {
                    response.Append($"    **•Vac Banned:** `{item.VACBanned}`\n");
                    accSummary.VacBanned = item.VACBanned;
                    response.Append($"    **•Last ban:** `{item.DaysSinceLastBan} days ago`\n");
                    accSummary.LastBan = item.DaysSinceLastBan;
                    response.Append($"    **•Number of Game Bans:** `{item.NumberOfGameBans}`\n");
                    accSummary.NumberOfGameBans = item.NumberOfGameBans;
                    response.Append($"    **•Number of Vac Bans:** `{item.NumberOfVACBans}`\n");
                    accSummary.NumberOfVACBans = item.NumberOfVACBans;
                    response.Append($"    **•Trade ban state:** `{communityProfileData.TradeBanState}`\n");
                    accSummary.TradeBanState = communityProfileData.TradeBanState;
                }
                else
                {
                    response.Append($"    **•Vac Banned:** `No`\n");
                    response.Append($"    **•Trade ban state:** `{communityProfileData.TradeBanState}`\n");
                    accSummary.TradeBanState = communityProfileData.TradeBanState;
                }
            }


            //String builderi paverciam i stringa ir atiduodam accSummary kad ji isnestu
            accSummary.Summary = response.ToString();

            GC.Collect();

            return accSummary;
        }

        public async Task<ulong> UrlIntoUlongAsync(string url)
        {
            ulong result = new ulong();


            if (url.Contains("https://steamcommunity.com/profiles/"))//Jeigu turi accounta be vanity url, tuo atveju ju steam id yra linke tai tenka ji tik apkarpyt
            {
                //Apkarpomas linkas kad tureti tik SteamId
                var holder = string.Empty;
                holder = url.Remove(0, 36);
                holder = holder.TrimEnd('/');

                //Atsakymas paverciamas is string i ulong ir priskiriama reiksme
                result = Convert.ToUInt64(holder);
            }
            else if (url.Contains("https://steamcommunity.com/id/"))//Jei vanity url yra, tenka prasyti steamapi mums ji isversti ir atiduoti steamid
            {
                //Apkarpomas linkas kad tureti tik vanity nick
                string vanity = "";
                vanity = url.Remove(0, 30);
                vanity = vanity.TrimEnd('/');

                //Internetines uzklausos sukurimas
                var webInterfaceFactory = new SteamWebInterfaceFactory(Configuration.jsonConfig.DevKey);
                var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());

                //Steam api prasymas duoti SteamId sio vanity nick savininko
                var vanityResolverResponse = await steamInterface.ResolveVanityUrlAsync(vanity).ConfigureAwait(false);
                var vanityResolverData = vanityResolverResponse.Data;

                //Priskiriama atsakymo reiksme
                result = vanityResolverData;
            }


            return result;
        }

        public List<AccountSummary> Recheck(bool vacBannedAlso)
        {
            //Labai mazai ka daryt cia siai funkcijai bet as nenoriu kad steamCommands liestu mysql
            var sql = new MySql();
            var result = sql.Recheck(vacBannedAlso);

            return result;
        }

        public string Watchlist()
        {
            var result = Recheck(true);

            var count = 1;
            var sb = new StringBuilder();

            sb.Append("```Players on watch list:\n");

            foreach (var item in result)
            {
                if (item.VacBanned)
                    sb.Append($"{count}. [Banned] {item.Nickname}\n");
                else
                    sb.Append($"{count}. {item.Nickname}\n");

                count++;
            }

            sb.Append("```");

            return sb.ToString();
        }

        public async Task<bool> DidVacStatusChangeAsync(string steamId)
        {
            //Paklausiam steamApi ar sis steamId yra banintas
            var webInterfaceFactory = new SteamWebInterfaceFactory(Configuration.jsonConfig.DevKey);
            var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());

            var communityProfileData = await steamInterface.GetCommunityProfileAsync(Convert.ToUInt64(steamId));

            if (communityProfileData.IsVacBanned)
            {
                var sql = new MySql();
                sql.DeemSteamIdBanned(steamId);

                return true;
            }
            else
                return false;
        }

        public int CurrentSuspectCount(bool vacBannedAlso)
        {
            //Labai mazai ka daryt cia siai funkcijai bet as nenoriu kad steamCommands liestu mysql
            var sql = new MySql();
            var result = sql.CurrentPlayerCountInDatabase(vacBannedAlso);

            return result;
        }

        public async Task VerifyNicknameChange(AccountSummary summary)
        {
            //Paklausiam koks nickname'as yra sio steamId
            var webInterfaceFactory = new SteamWebInterfaceFactory(Configuration.jsonConfig.DevKey);
            var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());

            var playerSummaryResponse = await steamInterface.GetPlayerSummaryAsync(Convert.ToUInt64(summary.SteamId));
            var playerSummaryData = playerSummaryResponse.Data;

            //Jei jis pasikeites tai pakeiciam duombazeje
            if (playerSummaryData.Nickname != summary.Nickname)
            {
                var sql = new MySql();
                sql.ChangeNickname(summary.Nickname, playerSummaryData.Nickname);
                Console.WriteLine($"{summary.Nickname} has changed his nickname to {playerSummaryData.Nickname}");
            }

            GC.Collect();
        }

        public async Task<String> Check(string indexToCheck)
        {
            //Isgauname sarasa esanciu useriu duombazeje
            var list = Recheck(true);
            //Rezultato kintamasis (String)
            var response = string.Empty;
            //Paprasome funkcijos visu esanciu zmoniu skaicio
            var currCount = CurrentSuspectCount(true);

            //Dvi regex patikros, pirma patikrina ar esanti string atrodo kaip steam id, antra patikrina ar esantis string yra tik skaiciai
            var rx = new Regex(@"^\d{17}$");
            var skPatikra = new Regex(@"^\d*$");

            //Patikrinam ar strind indexToCheck yra sudarytas tik is skaiciu ar ne
            if (skPatikra.IsMatch(indexToCheck))
            {
                //Jei taip pasidarom int kopija
                var skaicius = Convert.ToInt32(indexToCheck);

                //Patikrinam ar sis skaicius yra lygiai 17charakteriu ilgumo
                if (rx.IsMatch(indexToCheck))
                {
                    //Jei taip tai reiskia mums duotas steamId, issiunciam ji i getsummary, gaunam rezultata ir returninam
                    var summary = GetSummary(Convert.ToUInt64(indexToCheck));
                    response = summary.Result.Summary;

                    return response;
                }
                else if (skaicius > currCount)
                {
                    //Jei skaicius yra didesnis uz visu zmoniu kieki serveryje taciau ne 17 skaiciu reiskias irasyta klaida, returninam kaip klaida
                    response = ($"Have you **imputed** a wrong Steam Id?\n" +
                        $"Or have you **tried** to break me, by giving a non valid list index?\n" +
                        $"Either way, an **error** has occured.");

                    return response;
                }
                else if (skaicius <= currCount && skaicius > 0)
                {
                    //Jei skaicius mazesnis uz esamu zmoniu kieki taciau didesnis uz nuli, reiskias sarase turetu buti
                    int index = 0;

                    //Sukuriam cikla visiems zmoniems ir issitraukiam tik ta kurio praso uzklausos asmuo
                    foreach (var item in list)
                    {
                        index++;
                        if (index == skaicius)
                        {
                            var summary = GetSummary(Convert.ToUInt64(item.SteamId));
                            response = summary.Result.Summary;

                            return response;
                        }
                    }
                }
            }
            else if (indexToCheck.Contains("https://steamcommunity.com/profiles/") || indexToCheck.Contains("https://steamcommunity.com/id/"))
            {
                //Taciau jei indexToCheck nera sudarytas tik is skaiciu reiskias bandomas irasyti steam url, patikrinam ar tai tiesa
                //jeigu taip tada isgaunam ulong is steamurl su esama funkcija ir returninam response

                var steamId = await UrlIntoUlongAsync(indexToCheck);
                var summary = GetSummary(steamId);

                response = summary.Result.Summary;
                return response;
            }

            //Jeigu nei vienas is ifu nebuvo patenkintas israsom kaip nezinoma klaida
            response = $"Something went **wrong**.";
            return response;
        }

        public async Task<String> Remove(int indexToRemove)
        {
            var list = Recheck(true);

            var currCount = CurrentSuspectCount(true);

            var response = string.Empty;

            if (indexToRemove > currCount)
            {
                response = $"Error, **number:** {indexToRemove} does `not` exist.";
                return response;
            }
            else if (indexToRemove <= currCount && indexToRemove > 0)
            {
                int index = 0;

                foreach (var item in list)
                {
                    index++;
                    if (index == indexToRemove)
                    {
                        var sql = new MySql();
                        sql.Remove(item.SteamId);
                        response = $"Success! Player: `{item.Nickname}` has been **removed**!";

                        return response;
                    }
                }
            }

            response = $"Something went **wrong**.";
            return response;

        }

    }
}


