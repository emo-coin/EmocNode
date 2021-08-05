using NBitcoin;

namespace Emocoin.Bitcoin.Networks
{
    public static class Networks
    {
        public static NetworksSelector Emoc
        {
            get
            {
                return new NetworksSelector(() => new EmocMain(), () => new EmocTest(), () => new EmocRegTest());
            }
        }
    }
}
