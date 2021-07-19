using Emocoin.Bitcoin.Consensus.Rules;
using Emocoin.Bitcoin.Features.Consensus.CoinViews;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Features.Consensus.Rules
{
    /// <summary>
    /// Rules that provide easy access to the <see cref="CoinView"/> which is the store for a PoW system.
    /// </summary>
    public abstract class UtxoStoreConsensusRule : FullValidationConsensusRule
    {
        /// <summary>Allow access to the POS parent.</summary>
        protected PowConsensusRuleEngine PowParent;

        protected CoinviewHelper coinviewHelper;

        /// <inheritdoc />
        public override void Initialize()
        {
            this.PowParent = this.Parent as PowConsensusRuleEngine;
            Guard.NotNull(this.PowParent, nameof(this.PowParent));

            this.coinviewHelper = new CoinviewHelper();
        }
    }
}