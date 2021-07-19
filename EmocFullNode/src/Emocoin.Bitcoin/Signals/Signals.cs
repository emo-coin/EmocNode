using Microsoft.Extensions.Logging;
using Emocoin.Bitcoin.EventBus;

namespace Emocoin.Bitcoin.Signals
{
    public interface ISignals : IEventBus
    {
    }

    public class Signals : InMemoryEventBus, ISignals
    {
        public Signals(ILoggerFactory loggerFactory, ISubscriptionErrorHandler subscriptionErrorHandler) : base(loggerFactory, subscriptionErrorHandler) { }
    }
}
