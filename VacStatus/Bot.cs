using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VacStatus.Local;
using VacStatus.Commands;

namespace VacStatus
{
    class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            //Boto konfiguracija
            var config = new DiscordConfiguration
            {
                Token = Configuration.jsonConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            Client = new DiscordClient(config);

            Client.Ready += Client_Ready;

            //Boto pagrindiniu komandu konfiguracija
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { Configuration.jsonConfig.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = true
            };


            //Igalinam pagrindines komandas
            Commands = Client.UseCommandsNext(commandsConfig);
            //Igalinam Steam komandas
            Commands.RegisterCommands<SteamCommands>();


            //Suteikiame prieeiga prie interneto
            await Client.ConnectAsync();
            //Pasakom programai neissijungti kai niekas nevyksta
            await Task.Delay(-1);
        }

        private Task Client_Ready(object sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
