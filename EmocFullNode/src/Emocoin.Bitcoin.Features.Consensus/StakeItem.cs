using NBitcoin;

namespace Emocoin.Bitcoin.Features.Consensus
{
    public class StakeItem
    {
        public uint256 BlockId;

        public BlockStake BlockStake;

        public bool InStore;

        public long Height;
    }
}
