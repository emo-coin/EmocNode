using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Features.BlockStore;
using Emocoin.Bitcoin.Features.Wallet;
using Emocoin.Bitcoin.Features.Wallet.Interfaces;
using Emocoin.Bitcoin.Features.Wallet.Services;
using Emocoin.Bitcoin.Interfaces;

namespace Emocoin.Bitcoin.Features.ColdStaking.Controllers
{
    /// <summary> All functionality is in WalletRPCController, just inherit the functionality in this feature.</summary>
    [ApiVersion("1")]
    public class ColdStakingWalletRPCController : WalletRPCController
    {
        public ColdStakingWalletRPCController(
            IBlockStore blockStore,
            IBroadcasterManager broadcasterManager,
            ChainIndexer chainIndexer,
            IConsensusManager consensusManager,
            IFullNode fullNode,
            ILoggerFactory loggerFactory,
            Network network,
            IScriptAddressReader scriptAddressReader,
            StoreSettings storeSettings,
            IWalletManager walletManager,
            IWalletService walletService,
            WalletSettings walletSettings,
            IWalletTransactionHandler walletTransactionHandler,
            IWalletSyncManager walletSyncManager) :
            base(blockStore, broadcasterManager, chainIndexer, consensusManager, fullNode, loggerFactory, network, scriptAddressReader, storeSettings, walletManager, walletService, walletSettings, walletTransactionHandler, walletSyncManager)
        {
        }
    }
}
