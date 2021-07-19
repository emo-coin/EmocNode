using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using Emocoin.Bitcoin.Base;
using Emocoin.Bitcoin.Builder;
using Emocoin.Bitcoin.Configuration.Logging;
using Emocoin.Bitcoin.Consensus;
using Emocoin.Bitcoin.Features.Consensus.CoinViews;
using Emocoin.Bitcoin.Features.Consensus.Interfaces;
using Emocoin.Bitcoin.Features.Consensus.ProvenBlockHeaders;
using Emocoin.Bitcoin.Features.Consensus.Rules;
using Emocoin.Bitcoin.Interfaces;

namespace Emocoin.Bitcoin.Features.Consensus
{
    /// <summary>
    /// A class providing extension methods for <see cref="IFullNodeBuilder"/>.
    /// </summary>
    public static class FullNodeBuilderConsensusExtension
    {
        public static IFullNodeBuilder UsePowConsensus(this IFullNodeBuilder fullNodeBuilder, DbType coindbType = DbType.Leveldb)
        {
            LoggingConfiguration.RegisterFeatureNamespace<PowConsensusFeature>("powconsensus");

            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                    .AddFeature<PowConsensusFeature>()
                    .FeatureServices(services =>
                    {
                        ConfigureCoinDatabaseImplementation(services, coindbType);

                        services.AddSingleton<ConsensusOptions, ConsensusOptions>();
                        services.AddSingleton<ICoinView, CachedCoinView>();
                        services.AddSingleton<IConsensusRuleEngine, PowConsensusRuleEngine>();
                        services.AddSingleton<IChainState, ChainState>();
                        services.AddSingleton<ConsensusQuery>()
                            .AddSingleton<INetworkDifficulty, ConsensusQuery>(provider => provider.GetService<ConsensusQuery>())
                            .AddSingleton<IGetUnspentTransaction, ConsensusQuery>(provider => provider.GetService<ConsensusQuery>());
                    });
            });

            return fullNodeBuilder;
        }

        public static IFullNodeBuilder UsePosConsensus(this IFullNodeBuilder fullNodeBuilder, DbType coindbType = DbType.Leveldb)
        {
            LoggingConfiguration.RegisterFeatureNamespace<PosConsensusFeature>("posconsensus");

            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                    .AddFeature<PosConsensusFeature>()
                    .FeatureServices(services =>
                    {
                        services.ConfigureCoinDatabaseImplementation(coindbType);

                        services.AddSingleton(provider => (IStakedb)provider.GetService<ICoindb>());
                        services.AddSingleton<ICoinView, CachedCoinView>();
                        services.AddSingleton<StakeChainStore>().AddSingleton<IStakeChain, StakeChainStore>(provider => provider.GetService<StakeChainStore>());
                        services.AddSingleton<IStakeValidator, StakeValidator>();
                        services.AddSingleton<IRewindDataIndexCache, RewindDataIndexCache>();
                        services.AddSingleton<IConsensusRuleEngine, PosConsensusRuleEngine>();
                        services.AddSingleton<IChainState, ChainState>();
                        services.AddSingleton<ConsensusQuery>()
                            .AddSingleton<INetworkDifficulty, ConsensusQuery>(provider => provider.GetService<ConsensusQuery>())
                            .AddSingleton<IGetUnspentTransaction, ConsensusQuery>(provider => provider.GetService<ConsensusQuery>());

                        services.AddSingleton<IProvenBlockHeaderStore, ProvenBlockHeaderStore>();

                        if (coindbType == DbType.Leveldb)
                            services.AddSingleton<IProvenBlockHeaderRepository, LevelDbProvenBlockHeaderRepository>();
                    });
            });

            return fullNodeBuilder;
        }

        public static void ConfigureCoinDatabaseImplementation(this IServiceCollection services, DbType coindbType)
        {
            if (coindbType == DbType.Dbreeze)
                services.AddSingleton<ICoindb, DBreezeCoindb>();

            if (coindbType == DbType.Leveldb)
                services.AddSingleton<ICoindb, LevelDbCoindb>();

            if (coindbType == DbType.Faster)
                services.AddSingleton<ICoindb, FasterCoindb>();
        }
    }
}
