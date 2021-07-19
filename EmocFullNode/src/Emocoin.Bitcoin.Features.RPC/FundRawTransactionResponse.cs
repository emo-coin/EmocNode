using NBitcoin;
using Newtonsoft.Json;

namespace Emocoin.Bitcoin.Features.RPC
{
    public class FundRawTransactionResponse
    {
        [JsonProperty(PropertyName = "hex")]
        public Transaction Transaction
        {
            get; set;
        }

        [JsonProperty(PropertyName = "fee")]
        public Money Fee
        {
            get; set;
        }

        [JsonProperty(PropertyName = "changepos")]
        public int ChangePos
        {
            get; set;
        }
    }
}
