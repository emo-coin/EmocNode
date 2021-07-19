using System;

namespace Emocoin.Bitcoin.Features.Wallet.Interfaces
{
    public interface ITransactionContext : IDisposable
    {
        void Rollback();
        void Commit();
    }
}
