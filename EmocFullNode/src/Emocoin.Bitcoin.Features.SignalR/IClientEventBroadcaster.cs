namespace Emocoin.Bitcoin.Features.SignalR
{
    public interface IClientEventBroadcaster
    {
        void Init(ClientEventBroadcasterSettings broadcasterSettings);
    }
}