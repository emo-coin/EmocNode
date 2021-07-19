using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Emocoin.Bitcoin.Controllers.Converters;

namespace Emocoin.Bitcoin.Controllers.Models
{
    /// <summary>
    /// Class representing a banned peer.
    /// </summary>
    public class BannedPeerModel
    {
        public string EndPoint { get; set; }

        public DateTime? BanUntil { get; set; }

        public string BanReason { get; set; }
    }
}