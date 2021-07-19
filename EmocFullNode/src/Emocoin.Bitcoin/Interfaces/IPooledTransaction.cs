using System.Threading.Tasks;
using NBitcoin;

namespace Emocoin.Bitcoin.Interfaces
{
    public interface IPooledTransaction
    {
        Task<Transaction> GetTransaction(uint256 trxid);
    }
}
