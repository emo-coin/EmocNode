using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.Base;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Interfaces;
using Emocoin.Bitcoin.P2P.Protocol.Payloads;
using Emocoin.Bitcoin.Utilities;
using TracerAttributes;

namespace Emocoin.Bitcoin.Features.BlockStore
{
    /// <inheritdoc />
    public sealed class ProvenHeadersBlockStoreBehavior : BlockStoreBehavior
    {
        private readonly Network network;
        private readonly ICheckpoints checkpoints;

        public ProvenHeadersBlockStoreBehavior(Network network, ChainIndexer chainIndexer, IChainState chainState, ILoggerFactory loggerFactory, IConsensusManager consensusManager, ICheckpoints checkpoints, IBlockStoreQueue blockStoreQueue)
            : base(chainIndexer, chainState, loggerFactory, consensusManager, blockStoreQueue)
        {
            this.network = Guard.NotNull(network, nameof(network));
            this.checkpoints = Guard.NotNull(checkpoints, nameof(checkpoints));
        }

        /// <inheritdoc />
        /// <returns>The <see cref="HeadersPayload"/> instance to announce to the peer, or <see cref="ProvenHeadersPayload"/> if the peers requires it.</returns>
        protected override Payload BuildHeadersAnnouncePayload(IEnumerable<ChainedHeader> headers)
        {
            // Sanity check. That should never happen.
            if (!headers.All(x => x.ProvenBlockHeader != null))
                throw new BlockStoreException("UnexpectedError: BlockHeader is expected to be a ProvenBlockHeader");

            var provenHeadersPayload = new ProvenHeadersPayload(headers.Select(s => s.ProvenBlockHeader).ToArray());

            return provenHeadersPayload;
        }

        [NoTrace]
        public override object Clone()
        {
            var clone = new ProvenHeadersBlockStoreBehavior(this.network, this.ChainIndexer, this.chainState, this.loggerFactory, this.consensusManager, this.checkpoints, this.blockStoreQueue)
            {
                CanRespondToGetDataPayload = this.CanRespondToGetDataPayload
            };

            return clone;
        }
    }
}
