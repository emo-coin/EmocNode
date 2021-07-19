using System;
using Emocoin.Bitcoin.EventBus;
using Emocoin.Bitcoin.EventBus.CoreEvents;

namespace Emocoin.Bitcoin.Features.SignalR.Events
{
    public class BlockConnectedClientEvent : IClientEvent
    {
        public string Hash { get; set; }

        public int Height { get; set; }

        public Type NodeEventType { get; } = typeof(BlockConnected);

        public void BuildFrom(EventBase @event)
        {
            if (@event is BlockConnected blockConnected)
            {
                this.Hash = blockConnected.ConnectedBlock.ChainedHeader.HashBlock.ToString();
                this.Height = blockConnected.ConnectedBlock.ChainedHeader.Height;
                return;
            }

            throw new ArgumentException();
        }
    }
}