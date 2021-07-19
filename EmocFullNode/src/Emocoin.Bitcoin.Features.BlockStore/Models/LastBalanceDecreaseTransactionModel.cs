using Emocoin.Bitcoin.Controllers.Models;

namespace Emocoin.Bitcoin.Features.BlockStore.Models
{
    public class LastBalanceDecreaseTransactionModel
    {
        public TransactionVerboseModel Transaction { get; set; }

        public int BlockHeight { get; set; }
    }
}
