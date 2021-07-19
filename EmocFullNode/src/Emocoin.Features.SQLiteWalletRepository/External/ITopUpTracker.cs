using Emocoin.Bitcoin.Features.Wallet.Interfaces;

namespace Emocoin.Features.SQLiteWalletRepository.External
{
    public interface ITopUpTracker
    {
        int WalletId { get; }
        int AccountIndex { get; }
        int AddressType { get; }
        int AddressCount { get; }
        int NextAddressIndex { get; }
        bool IsWatchOnlyAccount { get; }

        AddressIdentifier CreateAddress();
    }
}
