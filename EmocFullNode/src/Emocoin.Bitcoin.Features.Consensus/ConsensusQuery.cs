using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.Base;
using Emocoin.Bitcoin.Features.Consensus.CoinViews;
using Emocoin.Bitcoin.Interfaces;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Features.Consensus
{
    /// <summary>
    /// A class that provides the ability to query consensus elements.
    /// </summary>
    public class ConsensusQuery : IGetUnspentTransaction, INetworkDifficulty
    {
        private readonly IChainState chainState;
        private readonly ICoinView coinView;
        private readonly ILogger logger;
        private readonly Network network;

        public ConsensusQuery(
            ICoinView coinView,
            IChainState chainState,
            Network network,
            ILoggerFactory loggerFactory)
        {
            this.coinView = coinView;
            this.chainState = chainState;
            this.network = network;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
        }

        /// <inheritdoc />
        public Task<UnspentOutput> GetUnspentTransactionAsync(OutPoint outPoint)
        {
            FetchCoinsResponse response = this.coinView.FetchCoins(new[] { outPoint });

            return Task.FromResult(response.UnspentOutputs.Values.SingleOrDefault());
        }

        /// <inheritdoc/>
        public Target GetNetworkDifficulty()
        {
            return this.chainState.ConsensusTip?.GetWorkRequired(this.network.Consensus);
        }
    }
}
