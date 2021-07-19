using Emocoin.Bitcoin.Primitives;

namespace Emocoin.Bitcoin.EventBus.CoreEvents
{
    /// <summary>
    /// Event that is executed when a block is disconnected from a consensus chain.
    /// </summary>
    /// <seealso cref="Emocoin.Bitcoin.EventBus.EventBase" />
    public class BlockDisconnected : EventBase
    {
        public ChainedHeaderBlock DisconnectedBlock { get; }

        public BlockDisconnected(ChainedHeaderBlock disconnectedBlock)
        {
            this.DisconnectedBlock = disconnectedBlock;
        }
    }
}