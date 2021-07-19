using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.AsyncWork;
using Emocoin.Bitcoin.Base;
using Emocoin.Bitcoin.Base.Deployments;
using Emocoin.Bitcoin.Configuration.Settings;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Consensus.Rules;
using Emocoin.Bitcoin.Features.Consensus.CoinViews;
using Emocoin.Bitcoin.Features.Consensus.Interfaces;
using Emocoin.Bitcoin.Features.Consensus.ProvenBlockHeaders;
using Emocoin.Bitcoin.Utilities;
using TracerAttributes;

namespace Emocoin.Bitcoin.Features.Consensus.Rules
{
    /// <summary>
    /// Extension of consensus rules that provide access to a PoS store.
    /// </summary>
    /// <remarks>
    /// A Proof-Of-Stake blockchain as implemented in this code base represents a hybrid POS/POW consensus model.
    /// </remarks>
    public class PosConsensusRuleEngine : PowConsensusRuleEngine
    {
        /// <summary>Database of stake related data for the current blockchain.</summary>
        public IStakeChain StakeChain { get; }

        /// <summary>Provides functionality for checking validity of PoS blocks.</summary>
        public IStakeValidator StakeValidator { get; }

        public IRewindDataIndexCache RewindDataIndexCache { get; }

        public PosConsensusRuleEngine(Network network, ILoggerFactory loggerFactory, IDateTimeProvider dateTimeProvider, ChainIndexer chainIndexer, NodeDeployments nodeDeployments,
            ConsensusSettings consensusSettings, ICheckpoints checkpoints, ICoinView utxoSet, IStakeChain stakeChain, IStakeValidator stakeValidator, IChainState chainState,
            IInvalidBlockHashStore invalidBlockHashStore, INodeStats nodeStats, IRewindDataIndexCache rewindDataIndexCache, IAsyncProvider asyncProvider, ConsensusRulesContainer consensusRulesContainer)
            : base(network, loggerFactory, dateTimeProvider, chainIndexer, nodeDeployments, consensusSettings, checkpoints, utxoSet, chainState, invalidBlockHashStore, nodeStats, asyncProvider, consensusRulesContainer)
        {
            this.StakeChain = stakeChain;
            this.StakeValidator = stakeValidator;
            this.RewindDataIndexCache = rewindDataIndexCache;
        }

        /// <inheritdoc />
        [NoTrace]
        public override RuleContext CreateRuleContext(ValidationContext validationContext)
        {
            return new PosRuleContext(validationContext, this.DateTimeProvider.GetTimeOffset());
        }

        /// <inheritdoc />
        public override void Initialize(ChainedHeader chainTip)
        {
            base.Initialize(chainTip);

            this.StakeChain.Load();

            // A temporary hack until tip manage will be introduced.
            var coindb = ((CachedCoinView)this.UtxoSet).ICoindb;
            ChainedHeader coinDbTip = chainTip.FindAncestorOrSelf(coindb.GetTipHash().Hash);

            this.RewindDataIndexCache.Initialize(coinDbTip.Height, this.UtxoSet);
        }
    }
}
