using System.Collections.Generic;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Features.Consensus
{
    public class UnspentOutputComparer : IComparer<UnspentOutput>
    {
        public static UnspentOutputComparer Instance { get; } = new UnspentOutputComparer();

        private readonly OutPointComparer Comparer = new OutPointComparer();

        public int Compare(UnspentOutput x, UnspentOutput y)
        {
            return this.Comparer.Compare(x.OutPoint, y.OutPoint);
        }
    }
}
