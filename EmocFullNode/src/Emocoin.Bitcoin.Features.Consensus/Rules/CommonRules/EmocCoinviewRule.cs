﻿using Microsoft.Extensions.Logging;
using NBitcoin;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Consensus.Rules;

namespace Emocoin.Bitcoin.Features.Consensus.Rules.CommonRules
{
    /// <summary>
    /// Emoc PoS overrides for certain coinview rule checks.
    /// </summary>
    public sealed class EmocCoinviewRule : PosCoinviewRule
    {
        // 50% of the block reward should be assigned to the reward script.
        // This has to be within the coinview rule because we need access to the coinstake input value to determine the size of the block reward.
        public static readonly int CirrusRewardPercentage = 10;

        /// <inheritdoc />
        public override void CheckBlockReward(RuleContext context, Money fees, int height, Block block)
        {
            Emoji expectEmoji = Emoji.GetSlotsByBlockHeight(height-1);
            Emoji rewardedEmoji = null;
            // Currently this rule only applies to PoS blocks
            if (BlockStake.IsProofOfStake(block))
            {
                var posRuleContext = context as PosRuleContext;
                Transaction coinstake = block.Transactions[1];
                Money stakeReward = coinstake.TotalOut - posRuleContext.TotalCoinStakeValueIn;
                Money calcStakeReward = fees + this.GetProofOfStakeReward(height);

                this.Logger.LogDebug("Block stake reward is {0}, calculated reward is {1}.", stakeReward, calcStakeReward);
                if (stakeReward > calcStakeReward)
                {
                    this.Logger.LogTrace("(-)[BAD_COINSTAKE_AMOUNT]");
                    ConsensusErrors.BadCoinstakeAmount.Throw();
                }

                // Compute the total reward amount sent to the reward script.
                // We only mandate that at least x% of the reward is sent there, there are no other constraints on what gets done with the rest of the reward.
                Money rewardScriptTotal = Money.Coins(0.0m);

                foreach (TxOut output in coinstake.Outputs)
                {
                    // TODO: Double check which rule we have the negative output (and overflow) amount check inside; we assume that has been done before this check
                    if (output.ScriptPubKey == EmocCoinstakeRule.CirrusRewardScript)
                        rewardScriptTotal += output.Value;
                }

                // It must be CirrusRewardPercentage of the maximum possible reward precisely.
                // This additionally protects cold staking transactions from over-allocating to the Cirrus reward script at the expense of the non-Cirrus reward.
                // This means that the hot key can be used for staking by anybody and they will not be able to redirect the non-Cirrus reward to the Cirrus script.
                // It must additionally not be possible to short-change the Cirrus reward script by deliberately sacrificing part of the overall claimed reward.
                // TODO: Create a distinct consensus error for this?
                if ((calcStakeReward * CirrusRewardPercentage / 100) != rewardScriptTotal)
                {
                    this.Logger.LogTrace("(-)[BAD_COINSTAKE_REWARD_SCRIPT_AMOUNT]");
                    ConsensusErrors.BadCirrusRewardAmount.Throw();
                }

                // TODO: Perhaps we should limit it to a single output to prevent unnecessary UTXO set bloating

                //find rewarded emoji
                foreach (var txOut in coinstake.Outputs)
                {
                    if (txOut.ScriptPubKey != EmocCoinstakeRule.CirrusRewardScript)
                        continue;
                    //byte[] scriptPubKeyBytes = txOut.ScriptPubKey.ToBytes(true);
                    //if (scriptPubKeyBytes.Length == 0 || scriptPubKeyBytes[0] == (byte)OpcodeType.OP_RETURN)
                    //    continue;
                    rewardedEmoji = txOut.Emoji;
                    break;
                }
            }
            else
            {
                Money blockReward = fees + this.GetProofOfWorkReward(height);
                this.Logger.LogDebug("Block reward is {0}, calculated reward is {1}.", block.Transactions[0].TotalOut, blockReward);
                if (block.Transactions[0].TotalOut > blockReward)
                {
                    this.Logger.LogTrace("(-)[BAD_COINBASE_AMOUNT]");
                    ConsensusErrors.BadCoinbaseAmount.Throw();
                }
                // TODO: Should the reward split apply to blocks in the POW phase of the network too?

                //find rewarded emoji
                foreach(var txOut in block.Transactions[0].Outputs)
                {
                    byte[] scriptPubKeyBytes = txOut.ScriptPubKey.ToBytes(true);
                    if (scriptPubKeyBytes.Length == 0 || scriptPubKeyBytes[0] == (byte)OpcodeType.OP_RETURN)
                        continue;
                    rewardedEmoji = txOut.Emoji;
                    break;
                }
            }

            if(rewardedEmoji == null || expectEmoji != rewardedEmoji)
            {
                this.Logger.LogTrace("(-)[BAD_COINBASE_EMOJI]");
                ConsensusErrors.BadCoinbaseEmoji.Throw();
            }
        }

        /// <inheritdoc />
        protected override void AllowSpend(TxOut prevOut, Transaction tx)
        {
            // We further need to check that any transactions that spend outputs from the reward script only go to the cross-chain multisig.
            // This check is not isolated to PoS specifically.
            if (prevOut.ScriptPubKey == EmocCoinstakeRule.CirrusRewardScript)
            {
                foreach (TxOut output in tx.Outputs)
                {
                    // We allow OP_RETURNs for tagging purposes, but they must not be allowed to have any value attached
                    // (as that would then be burning Cirrus rewards)
                    if (output.ScriptPubKey.IsUnspendable)
                    {
                        if (output.Value != 0)
                        {
                            this.Logger.LogTrace("(-)[INVALID_REWARD_OP_RETURN_SPEND]");
                            ConsensusErrors.BadTransactionScriptError.Throw();
                        }

                        continue;
                    }

                    // Every other (spendable) output must go to the multisig
                    if (output.ScriptPubKey != this.Parent.Network.Federations.GetOnlyFederation().MultisigScript.PaymentScript)
                    {
                        this.Logger.LogTrace("(-)[INVALID_REWARD_SPEND_DESTINATION]");
                        ConsensusErrors.BadTransactionScriptError.Throw();
                    }
                }

                // TODO: This is the wrong destination message. Should be output.scriptpubkey?
                this.Logger.LogDebug("Reward distribution transaction validated in consensus, spending to '{0}'.", prevOut.ScriptPubKey);
            }

            // Otherwise allow the spend (do nothing).
        }
    }
}
