using System;

namespace Emocoin.Bitcoin.Features.Consensus
{
    public class BlockNotFoundException : Exception
    {
        public BlockNotFoundException(string message) : base(message)
        {
        }
    }
}
