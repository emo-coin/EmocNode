using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Emocoin.Bitcoin.Builder;
using Emocoin.Bitcoin.Builder.Feature;
using Emocoin.Bitcoin.Connection;
using Emocoin.Bitcoin.Features.Notifications.Controllers;

namespace Emocoin.Bitcoin.Features.Notifications
{
    /// <summary>
    /// Feature enabling the broadcasting of transactions.
    /// </summary>
    public class TransactionNotificationFeature : FullNodeFeature
    {
        private readonly IConnectionManager connectionManager;

        private readonly TransactionReceiver transactionBehavior;

        public TransactionNotificationFeature(IConnectionManager connectionManager, TransactionReceiver transactionBehavior)
        {
            this.connectionManager = connectionManager;
            this.transactionBehavior = transactionBehavior;
        }

        public override Task InitializeAsync()
        {
            this.connectionManager.Parameters.TemplateBehaviors.Add(this.transactionBehavior);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// A class providing extension methods for <see cref="IFullNodeBuilder"/>.
    /// </summary>
    public static class FullNodeBuilderTransactionNotificationExtension
    {
        public static IFullNodeBuilder UseTransactionNotification(this IFullNodeBuilder fullNodeBuilder)
        {
            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<TransactionNotificationFeature>()
                .FeatureServices(services =>
                    {
                        services.AddSingleton<TransactionNotificationProgress>();
                        services.AddSingleton<TransactionReceiver>();
                    });
            });

            return fullNodeBuilder;
        }
    }
}
