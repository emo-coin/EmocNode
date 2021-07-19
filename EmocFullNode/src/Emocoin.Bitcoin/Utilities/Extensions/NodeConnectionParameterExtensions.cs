using System.Linq;
using Emocoin.Bitcoin.P2P;
using Emocoin.Bitcoin.P2P.Peer;

namespace Emocoin.Bitcoin.Utilities.Extensions
{
    public static class NodeConnectionParameterExtensions
    {
        public static PeerAddressManagerBehaviour PeerAddressManagerBehaviour(this NetworkPeerConnectionParameters parameters)
        {
            return parameters.TemplateBehaviors.OfType<PeerAddressManagerBehaviour>().FirstOrDefault();
        }
    }
}