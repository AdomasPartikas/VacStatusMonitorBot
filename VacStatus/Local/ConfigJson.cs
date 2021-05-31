using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VacStatus.Local
{
    class ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("devKey")]
        public string DevKey { get; private set; }
    }
}
