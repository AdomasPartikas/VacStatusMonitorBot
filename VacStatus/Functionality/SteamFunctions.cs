using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
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
        public async Task<(string,bool)> MainInfoAndPlayerAdd(string url)
        {
            var steamIdUlong = await UrlIntoUlongAsync(url);

            var summary = await GetSummary(steamIdUlong);

            var sql = new MySql();
            var boolResult = sql.AddSuspect(summary);

            return (summary.Summary,boolResult);
        }

        public async Task<AccountSummary> GetSummary(ulong steamId)
        {
            var accSummary = new AccountSummary();

            //await ConfigureJsonAsync();

            var webInterfaceFactory = new SteamWebInterfaceFactory(Configuration.jsonConfig.DevKey);
            var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());

            //Most of the accounts information is in player summary
            var playerSummaryResponse = await steamInterface.GetPlayerSummaryAsync(steamId);
            var playerSummaryData = playerSummaryResponse.Data;
            //Getting community profile data
            var communityProfileData = await steamInterface.GetCommunityProfileAsync(steamId);
            //What
            var playerBanResponse = await steamInterface.GetPlayerBansAsync(steamId);
            var playerBanData = playerBanResponse.Data;

            //Adding everything to variables who we will transfer
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
            StringBuilder response = new StringBuilder();

            response.Append($"**Nickname:** `{playerSummaryData.Nickname}`\n");

            if (playerSummaryData.RealName != null)
                response.Append($"**Real Name:** `{playerSummaryData.RealName}`\n");
            else
                response.Append($"**Real Name:** `Unknown`\n");


            response.Append($"**User Status:** `{playerSummaryData.UserStatus}`\n" +
            $"**SteamId:** `{steamId}`\n");

            response.Append($"**Member since:** `{communityProfileData.MemberSince}`\n");



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
                if(item.DaysSinceLastBan > 0 || item.VACBanned || item.NumberOfGameBans > 0)
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


            accSummary.Summary = response.ToString();

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
                //Konfiguruojamas json failas kad tureti tinkama steam DevKey
                //await ConfigureJsonAsync();

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
    }
}
