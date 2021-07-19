using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.Base.Deployments;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Features.Consensus;
using Emocoin.Bitcoin.Features.Consensus.Interfaces;
using Emocoin.Bitcoin.Features.MemoryPool;
using Emocoin.Bitcoin.Features.MemoryPool.Interfaces;
using Emocoin.Bitcoin.Mining;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Features.Miner
{
    /// <summary>
    /// Defines how a proof of work block will be built on a proof of stake network.
    /// </summary>
    public sealed class PosPowBlockDefinition : BlockDefinition
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        /// <summary>Database of stake related data for the current blockchain.</summary>
        private readonly IStakeChain stakeChain;

        /// <summary>Provides functionality for checking validity of PoS blocks.</summary>
        private readonly IStakeValidator stakeValidator;
        
        public PosPowBlockDefinition(
            IConsensusManager consensusManager,
            IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory,
            ITxMempool mempool,
            MempoolSchedulerLock mempoolLock,
            Network network,
            MinerSettings minerSettings,
            IStakeChain stakeChain,
            IStakeValidator stakeValidator,
            NodeDeployments nodeDeployments)
            : base(consensusManager, dateTimeProvider, loggerFactory, mempool, mempoolLock, minerSettings, network, nodeDeployments)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.stakeChain = stakeChain;
            this.stakeValidator = stakeValidator;
        }

        /// <inheritdoc/>
        public override void AddToBlock(TxMempoolEntry mempoolEntry)
        {
            this.AddTransactionToBlock(mempoolEntry.Transaction);
            this.UpdateBlockStatistics(mempoolEntry);
            this.UpdateTotalFees(mempoolEntry.Fee);
        }

        /// <inheritdoc/>
        public override BlockTemplate Build(ChainedHeader chainTip, Script scriptPubKey)
        {
            this.OnBuild(chainTip, scriptPubKey);

            return this.BlockTemplate;
        }

        /// <inheritdoc/>
        public override void UpdateHeaders()
        {
            base.UpdateBaseHeaders();

            this.block.Header.Bits = this.stakeValidator.GetNextTargetRequired(this.stakeChain, this.ChainTip, this.Network.Consensus, false);
        }
    }
}
