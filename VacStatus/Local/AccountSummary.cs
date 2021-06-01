using System;
using System.Collections.Generic;
using System.Text;

namespace VacStatus.Local
{
    public class AccountSummary
    {
        public string SteamId { get; set; }
        public string Nickname { get; set; }
        public string RealName { get; set; }
        public string MemberSince { get; set; }
        public string PlayingGameName { get; set; }
        public bool VacBanned { get; set; }
        public uint LastBan { get; set; }
        public uint NumberOfGameBans { get; set; }
        public uint NumberOfVACBans { get; set; }
        public string TradeBanState { get; set; }
        public string Summary { get; set; }
    }
}
