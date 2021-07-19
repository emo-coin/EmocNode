using System;
using System.Threading.Tasks;
using NBitcoin;
using Emocoin.Bitcoin.Features.MemoryPool;
using Emocoin.Bitcoin.Features.Wallet.Broadcasting;

namespace Emocoin.Bitcoin.Features.Wallet.Interfaces
{
    public interface IBroadcasterManager
    {
        Task BroadcastTransactionAsync(Transaction transaction);

        event EventHandler<TransactionBroadcastEntry> TransactionStateChanged;

        TransactionBroadcastEntry GetTransaction(uint256 transactionHash);

        void AddOrUpdate(Transaction transaction, TransactionBroadcastState transactionBroadcastState, MempoolError mempoolError = null);
    }
}
