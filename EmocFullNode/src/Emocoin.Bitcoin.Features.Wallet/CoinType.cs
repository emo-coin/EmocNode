namespace Emocoin.Bitcoin.Features.Wallet
{
    /// <summary>
    /// The type of coin, as specified in BIP44.
    /// </summary>
    /// <remarks>For more, see https://github.com/satoshilabs/slips/blob/master/slip-0044.md</remarks>
    public enum CoinType
    {
        /// <summary>
        /// Bitcoin (mainnet)
        /// </summary>
        Bitcoin = 0,

        /// <summary>
        /// Testnet and regtest (all coins)
        /// </summary>
        Testnet = 1,

        /// <summary>
        /// Emoc (mainnet)
        /// </summary>
        Emoc = 105105
    }
}
