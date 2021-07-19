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
namespace Emocoin.EmocD
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Emoji emo = Emoji.GetRandomPureBreed();
            //string strEmo = emo.ToString();
            //Emoji emo1 = new Emoji(strEmo);
            //string strEmo1 = emo1.ToString();

            try
            {
                var nodeSettings = new NodeSettings(networksSelector: Networks.Emoc, protocolVersion: ProtocolVersion.PROVEN_HEADER_VERSION, args: args)
                {
                    MinProtocolVersion = ProtocolVersion.PROVEN_HEADER_VERSION
                };

                // Set the console window title to identify this as a Emoc full node
                Console.Title = $"Emoc Full Node {nodeSettings.Network.NetworkType}";

                DbType dbType = nodeSettings.GetDbType();

                IFullNodeBuilder nodeBuilder = new FullNodeBuilder()
                    .UseNodeSettings(nodeSettings, dbType)
                    .UseBlockStore(dbType)
                    .UsePosConsensus(dbType)
                    .UseMempool()
                    .UseColdStakingWallet()
                    .AddSQLiteWalletRepository()
                    .AddPowPosMining(true)
                    .UseApi()
                    .AddRPC()
                    .UseDiagnosticFeature();

                if (nodeSettings.EnableSignalR)
                {
                    nodeBuilder.AddSignalR(options =>
                    {
                        options.EventsToHandle = new[]
                        {
                            (IClientEvent) new BlockConnectedClientEvent(),
                            new TransactionReceivedClientEvent()
                        };

                        options.ClientEventBroadcasters = new[]
                        {
                            (Broadcaster: typeof(StakingBroadcaster), ClientEventBroadcasterSettings: new ClientEventBroadcasterSettings
                                {
                                    BroadcastFrequencySeconds = 5
                                }),
                            (Broadcaster: typeof(WalletInfoBroadcaster), ClientEventBroadcasterSettings: new ClientEventBroadcasterSettings
                                {
                                    BroadcastFrequencySeconds = 5
                                })
                        };
                    });
                }

                IFullNode node = nodeBuilder.Build();

                if (node != null)
                    await node.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex);
            }
        }
    }
}
