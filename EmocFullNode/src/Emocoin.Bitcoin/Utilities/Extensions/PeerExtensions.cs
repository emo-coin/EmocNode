using Emocoin.Bitcoin.Connection;
using Emocoin.Bitcoin.P2P.Peer;

namespace Emocoin.Bitcoin.Utilities.Extensions
{
    public static class PeerExtensions
    {
        public static bool IsWhitelisted(this INetworkPeer peer)
        {
            return peer.Behavior<IConnectionManagerBehavior>()?.Whitelisted == true;
        }
    }
}
