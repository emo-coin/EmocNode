using System;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Protocol;
using Emocoin.Bitcoin;
using Emocoin.Bitcoin.Builder;
using Emocoin.Bitcoin.Configuration;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Features.Api;
using Emocoin.Bitcoin.Features.BlockStore;
using Emocoin.Bitcoin.Features.ColdStaking;
using Emocoin.Bitcoin.Features.Consensus;
using Emocoin.Bitcoin.Features.MemoryPool;
using Emocoin.Bitcoin.Features.Miner;
using Emocoin.Bitcoin.Features.RPC;
using Emocoin.Bitcoin.Features.SignalR;
using Emocoin.Bitcoin.Features.SignalR.Broadcasters;
using Emocoin.Bitcoin.Features.SignalR.Events;
using Emocoin.Bitcoin.Networks;
using Emocoin.Bitcoin.Utilities;
using Emocoin.Features.Diagnostic;
using Emocoin.Features.SQLiteWalletRepository;
/*
TBD:
            !!!must be updated for Emoji field in transaction!!!
            // TODO: Update this once the final block is mined
            //Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x77283cca51b83fe3bda9ce8966248613036b0dc55a707ce76ca7b79aaa9962e4"));

- expose emoji through API;



*/
namespace Emocoin.NodeD
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //generate cirrusreward scripts
            var selector = Emocoin.Bitcoin.Networks.Networks.Emoc;
            Network mainNet = selector.Mainnet();

            string strAddr = "exoFT56PFv936JmRmqWgoYY4c9XjEQW4eD"; //dev wallet address
            Script script = BitcoinAddress.Create(strAddr, mainNet).ScriptPubKey;
            string hexScript = script.ToHex();
            Console.WriteLine("new cirrusreward: " + hexScript);

            //extract genesis block hash
            string genesisHash = mainNet.GenesisHash.ToString();
            Console.WriteLine("genesis hash: " + genesisHash);

            var genesis = mainNet.GetGenesis();
            string hashMerkleRoot = genesis.Header.HashMerkleRoot.ToString();
            Console.WriteLine("hashMerkleRoot: " + hashMerkleRoot);
        }
    }
}
