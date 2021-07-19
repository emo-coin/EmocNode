using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.Base.Deployments;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Features.MemoryPool;
using Emocoin.Bitcoin.Features.MemoryPool.Interfaces;
using Emocoin.Bitcoin.Mining;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Features.Miner
{
    public class PowBlockDefinition : BlockDefinition
    {
        private readonly IConsensusRuleEngine consensusRules;
        private readonly ILogger logger;

        public PowBlockDefinition(
            IConsensusManager consensusManager,
            IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory,
            ITxMempool mempool,
            MempoolSchedulerLock mempoolLock,
            MinerSettings minerSettings,
            Network network,
            IConsensusRuleEngine consensusRules,
            NodeDeployments nodeDeployments,
            BlockDefinitionOptions options = null)
            : base(consensusManager, dateTimeProvider, loggerFactory, mempool, mempoolLock, minerSettings, network, nodeDeployments)
        {
            this.consensusRules = consensusRules;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
        }

        public override void AddToBlock(TxMempoolEntry mempoolEntry)
        {
            this.AddTransactionToBlock(mempoolEntry.Transaction);
            this.UpdateBlockStatistics(mempoolEntry);
            this.UpdateTotalFees(mempoolEntry.Fee);
        }

        /// <inheritdoc/>
        public override BlockTemplate Build(ChainedHeader chainTip, Script scriptPubKey)
        {
            base.OnBuild(chainTip, scriptPubKey);

            return this.BlockTemplate;
        }

        /// <inheritdoc/>
        public override void UpdateHeaders()
        {
            base.UpdateBaseHeaders();

            this.block.Header.Bits = this.block.Header.GetWorkRequired(this.Network, this.ChainTip);
        }
    }
}