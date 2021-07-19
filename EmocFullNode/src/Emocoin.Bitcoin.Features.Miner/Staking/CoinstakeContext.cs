using Emocoin.Bitcoin.Features.Wallet;
using NBitcoin;

namespace Emocoin.Bitcoin.Features.Miner.Staking
{
    /// <summary>
    /// Information about coinstake transaction and its private key.
    /// </summary>
    public class CoinstakeContext
    {
        /// <summary>Coinstake transaction being constructed.</summary>
        public Transaction CoinstakeTx { get; set; }

        public HdAddress CoinstakeAddress { get; set; }

        /// <summary>If the function succeeds, this is filled with private key for signing the coinstake kernel.</summary>
        public Key Key { get; set; }

        /// <summary>The time slot that stake was found.</summary>
        public uint StakeTimeSlot { get; set; }
    }
}
