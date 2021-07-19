using Microsoft.Extensions.Logging;
using Emocoin.Bitcoin.AsyncWork;
using Emocoin.Bitcoin.Features.Wallet.Services;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Features.SignalR.Broadcasters
{
    /// <summary>
    /// Broadcasts current staking information to SignalR clients
    /// </summary>
    public class CirrusWalletInfoBroadcaster : WalletInfoBroadcaster
    {
        public CirrusWalletInfoBroadcaster(
            ILoggerFactory loggerFactory,
            IWalletService walletService,
            IAsyncProvider asyncProvider,
            INodeLifetime nodeLifetime,
            EventsHub eventsHub)
            : base(loggerFactory, walletService, asyncProvider, nodeLifetime, eventsHub, true)
        {
        }
    }
}