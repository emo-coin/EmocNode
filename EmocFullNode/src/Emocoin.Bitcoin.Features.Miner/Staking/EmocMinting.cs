using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.AsyncWork;
using Emocoin.Bitcoin.Base;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Features.Consensus;
using Emocoin.Bitcoin.Features.Consensus.CoinViews;
using Emocoin.Bitcoin.Features.Consensus.Interfaces;
using Emocoin.Bitcoin.Features.Consensus.Rules.CommonRules;
using Emocoin.Bitcoin.Features.MemoryPool;
using Emocoin.Bitcoin.Features.MemoryPool.Interfaces;
using Emocoin.Bitcoin.Features.Wallet.Interfaces;
using Emocoin.Bitcoin.Interfaces;
using Emocoin.Bitcoin.Mining;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Features.Miner.Staking
{
    public class EmocMinting : PosMinting
    {
        public EmocMinting(
            IBlockProvider blockProvider,
            IConsensusManager consensusManager,
            ChainIndexer chainIndexer,
            Network network,
            IDateTimeProvider dateTimeProvider,
            IInitialBlockDownloadState initialBlockDownloadState,
            INodeLifetime nodeLifetime,
            ICoinView coinView,
            IStakeChain stakeChain,
            IStakeValidator stakeValidator,
            MempoolSchedulerLock mempoolLock,
            ITxMempool mempool,
            IWalletManager walletManager,
            IAsyncProvider asyncProvider,
            ITimeSyncBehaviorState timeSyncBehaviorState,
            ILoggerFactory loggerFactory,
            MinerSettings minerSettings) : base(blockProvider, consensusManager, chainIndexer, network, dateTimeProvider,
                initialBlockDownloadState, nodeLifetime, coinView, stakeChain, stakeValidator, mempoolLock, mempool,
                walletManager, asyncProvider, timeSyncBehaviorState, loggerFactory, minerSettings)
        {
        }

        public override Transaction PrepareCoinStakeTransactions(int currentChainHeight, CoinstakeContext coinstakeContext, long coinstakeOutputValue, int utxosCount, long amountStaked, long reward)
        {
            long cirrusReward = reward * EmocCoinviewRule.CirrusRewardPercentage / 100;
            //coinstakeOutputValue -= cirrusReward;

            // Populate the initial coinstake with the modified overall reward amount, the outputs will be split as necessary
            base.PrepareCoinStakeTransactions(currentChainHeight, coinstakeContext, coinstakeOutputValue, utxosCount, amountStaked, reward);

            // Now add the remaining reward into an additional output on the coinstake
            var emoji = Emoji.GetSlotsByBlockHeight(currentChainHeight);

            long stakeReward = reward - cirrusReward;
            var stakeOut = new TxOut(stakeReward, coinstakeContext.CoinstakeAddress.ScriptPubKey, emoji);
            coinstakeContext.CoinstakeTx.Outputs.Add(stakeOut);

            var cirrusRewardOutput = new TxOut(cirrusReward, EmocCoinstakeRule.CirrusRewardScript, emoji);
            coinstakeContext.CoinstakeTx.Outputs.Add(cirrusRewardOutput);

            return coinstakeContext.CoinstakeTx;
        }
    }
}
