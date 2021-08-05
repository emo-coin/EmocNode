using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Emocoin.Bitcoin.Networks.Deployments;
using Emocoin.Bitcoin.Networks.Policies;

namespace Emocoin.Bitcoin.Networks
{
    public class EmocMain : Network
    {
        public EmocMain()
        {
            this.Name = "EmocMain";
            this.NetworkType = NetworkType.Mainnet;
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            this.Magic = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("EMOC"));
            this.DefaultPort = 27115;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 96;
            this.DefaultRPCPort = 27114;
            this.DefaultAPIPort = 27113;
            this.DefaultSignalRPort = 27112;
            this.MaxTipAge = 28800; //will reduce after Gen0
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = EmocNetwork.EmocRootFolderName;
            this.DefaultConfigFilename = EmocNetwork.EmocDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "EMOC";
            this.DefaultBanTimeSeconds = 11250; // 500 (MaxReorg) * 45 (TargetSpacing) / 2 = 3 hours, 7 minutes and 30 seconds

            //emoc dev wallet
            this.CirrusRewardDummyAddress = "f9shLUuKN18zXeSEdMYjHnyxSYTX3c1ju6";

            //low limit to make POW easy to start
            var powLimit = new Target(new uint256("00002fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1623961255; // Jun 17, 2021
            this.GenesisNonce = 1; // TODO: Update once the final block is mined 
            this.GenesisBits = powLimit.ToCompact();
            this.GenesisVersion = 7; //we should be here
            this.GenesisReward = Money.Zero;

            Block genesisBlock = EmocNetwork.CreateGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward, "emoc.io/emocgenesisblock");

            this.Genesis = genesisBlock;

            // Taken from Emocoin.
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 150_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 2,
                witnessScaleFactor: 4
            );

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new EmocBIP9Deployments()
            {
                // Always active.
                [EmocBIP9Deployments.CSV] = new BIP9DeploymentsParameters("CSV", 0, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [EmocBIP9Deployments.Segwit] = new BIP9DeploymentsParameters("Segwit", 1, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                //re-enable when ready
                //[EmocBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold)
            };

            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 105105, // Per https://github.com/satoshilabs/slips/blob/master/slip-0044.md - testnets share a cointype
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: null,
                minerConfirmationWindow: 516, // nPowTargetTimespan / nPowTargetSpacing (1/4 day)
                maxReorgLength: 500,
                defaultAssumeValid: null, // turn off assumevalid for regtest.
                maxMoney: long.MaxValue,
                coinbaseMaturity: 10,
                premineHeight: 4,
                premineReward: Money.Coins(1000000),
                proofOfWorkReward: Money.Coins(10),
                powTargetTimespan: TimeSpan.FromSeconds(1 * 24 * 60 * 60), // one day
                targetSpacing: TimeSpan.FromSeconds(45),
                powAllowMinDifficultyBlocks: true,
                posNoRetargeting: false,
                powNoRetargeting: true,
                powLimit: powLimit,
                minimumChainWork: uint256.Zero,
                isProofOfStake: true,
                lastPowBlock: 1000,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00002fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000fffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(10)
            );

            this.Consensus.PosEmptyCoinbase = false;

            this.Base58Prefixes = new byte[12][];
            //prefixes
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (94) }; //e or f
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (86) }; //b or c
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (94 + 128) };

            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x29 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x31 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1D) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE3) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0x1E, 0xFF, 0x39, 0x25 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9B };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2C };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 24 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x18 };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>()
            {
//new cirrusreward: 76a914580c0bd18511936e524b08d629e81cb1ef4644da88ac
                //init
                { 0, new CheckpointInfo(new uint256("0xd22dcc4e0c9b8f329a4e3607fa7d1947edf60981cbd665dc1f2801b1adc8a21c"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) },
                //premine
                { 4, new CheckpointInfo(new uint256("0xa54f5a628c004cec11111e46efe72ebc90bc8a4c695b34aecc90eb1e970ad845"), new uint256("0x227c92857c612c994fb5647efbeb78994122c69fbcc5c5dc23105f2d64a7fd81")) },
                //changed tip to 28800s, disabled POS retargeting, increased POS difficulty
                //last POW, POS enforced
                //{ 5000, new CheckpointInfo(new uint256("0x"), new uint256("0x350db25ca3ff01ec589681c94c325f619e5013bdc06efcbefa981776f4dcca4f")) },
                //Gen1 emoji start
                //{ 175000, new CheckpointInfo(new uint256("0xe3398765bc0da5b481a5dfe60f0acf14f4b1fc8582bab8f7a166317aea9aa026"), new uint256("0x350db25ca3ff01ec589681c94c325f619e5013bdc06efcbefa981776f4dcca4f")) },

            };

            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder("emoc");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.DNSSeeds = new List<DNSSeedData>();
            this.SeedNodes = new List<NetworkAddress>();

            this.StandardScriptsRegistry = new EmocStandardScriptsRegistry();

            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * this.Consensus.TargetSpacing.TotalSeconds / 2);

            //!!!must be updated for Emoji field in transaction!!!
            // TODO: Update this once the final block is mined
            //genesis hash: d22dcc4e0c9b8f329a4e3607fa7d1947edf60981cbd665dc1f2801b1adc8a21c
            //hashMerkleRoot: 49f01e69314c6845cb923fa3fb2c525aec76e1c507df5505730c5977b3c5729f            
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0xd22dcc4e0c9b8f329a4e3607fa7d1947edf60981cbd665dc1f2801b1adc8a21c"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x49f01e69314c6845cb923fa3fb2c525aec76e1c507df5505730c5977b3c5729f"));

            EmocNetwork.RegisterRules(this.Consensus);
            EmocNetwork.RegisterMempoolRules(this.Consensus);
        }
    }
}
