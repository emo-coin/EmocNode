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

namespace Emocoin.EmocD
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
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
