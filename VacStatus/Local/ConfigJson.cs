using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace VacStatus.Local
{

    public class Configuration
    {
        public static ConfigJson jsonConfig;

        public async Task ConfigureJsonAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead(@$"..\..\..\config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            jsonConfig = JsonConvert.DeserializeObject<ConfigJson>(json);
        }
    }

    public class ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
        [JsonProperty("devKey")]
        public string DevKey { get; set; }
        [JsonProperty("mysqlConnection")]
        public string MySqlConnection { get; set; }
    }
}
