using System;
using Emocoin.Bitcoin.EventBus;

namespace Emocoin.Bitcoin.Features.SignalR
{
    public interface IClientEvent
    {
        Type NodeEventType { get; }

        void BuildFrom(EventBase @event);
    }
}