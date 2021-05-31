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
        ConfigJson configJson = new ConfigJson();

        public async Task<string> MainInfoAndPlayerAdd(string url)
        {
            var steamIdUlong = await UrlIntoUlongAsync(url);
            var result = string.Empty;

            result = await GetSummary(steamIdUlong);
            return result;
        }

        public async Task<String> GetSummary(ulong steamId)
        {
            await ConfigureJsonAsync();

            var webInterfaceFactory = new SteamWebInterfaceFactory(configJson.DevKey);
            var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());

            var playerSummaryResponse = await steamInterface.GetPlayerSummaryAsync(steamId);
            var playerSummaryData = playerSummaryResponse.Data;


            var communityProfileData = await steamInterface.GetCommunityProfileAsync(steamId);

            var playerBanResponse = await steamInterface.GetPlayerBansAsync(steamId);
            var playerBanData = playerBanResponse.Data;




            StringBuilder response = new StringBuilder();


            response.Append($"**Nickname:** `{playerSummaryData.Nickname}`\n");


            if (playerSummaryData.RealName != null)
                response.Append($"**Real Name:** `{playerSummaryData.RealName}`\n");
            else
                response.Append($"**Real Name:** `Unknown`\n");


            response.Append($"**User Status:** `{playerSummaryData.UserStatus}`\n" +
            $"**SteamId:** `{steamId}`\n");


            if (playerSummaryData.PlayingGameName != null)
                response.Append($"**Currently Playing:** `{playerSummaryData.PlayingGameName}`\n");
            else
                response.Append($"**Currently Playing:** `Nothing`\n");


            if (communityProfileData.IsVacBanned)
                response.Append("**Vac Banned:** `Yes`\n");
            else
                response.Append("**Vac Banned:** `No`\n");


            response.Append($"**Member since:** `{communityProfileData.MemberSince}`\n");



            /*
            response.Append($"**Last log off:** `{playerSummaryData.LastLoggedOffDate}`\n" +
                $"**Vac Banned:** `{playerJson.VACBanned}`\n" +
                $"**Community Banned:** `{playerJson.CommunityBanned}`\n" +
                $"**Days Since Last Ban:** `{playerJson.DaysSinceLastBan}`\n");
            */

            return response.ToString();
        }

        public async Task<ulong> UrlIntoUlongAsync(string url)
        {
            ulong result = new ulong();


            if (url.Contains("https://steamcommunity.com/profiles/"))
            {
                //Apkarpomas linkas kad tureti tik SteamId
                var holder = string.Empty;
                holder = url.Remove(0, 36);
                holder = holder.TrimEnd('/');

                //Atsakymas paverciamas is string i ulong ir priskiriama reiksme
                result = Convert.ToUInt64(holder);
            }
            else if (url.Contains("https://steamcommunity.com/id/"))
            {
                //Konfiguruojamas json failas kad isgauti DevKey
                await ConfigureJsonAsync();

                //Apkarpomas linkas kad tureti tik vanity nick
                string vanity = "";
                vanity = url.Remove(0, 30);
                vanity = vanity.TrimEnd('/');

                //Internetines uzklausos sukurimas
                var webInterfaceFactory = new SteamWebInterfaceFactory(configJson.DevKey);
                var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());

                //Steam api prasymas duoti SteamId sio vanity nick savininko
                var vanityResolverResponse = await steamInterface.ResolveVanityUrlAsync(vanity).ConfigureAwait(false);
                var vanityResolverData = vanityResolverResponse.Data;

                //Priskiriama atsakymo reiksme
                result = vanityResolverData;
            }


            return result;
        }

        public async Task ConfigureJsonAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead(@$"..\..\..\config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
        }
    }
}
