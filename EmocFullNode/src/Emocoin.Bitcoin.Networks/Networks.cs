using NBitcoin;

namespace Emocoin.Bitcoin.Networks
{
    public static class Networks
    {
        public static NetworksSelector Bitcoin
        {
            get
            {
                return new NetworksSelector(() => new BitcoinMain(), () => new BitcoinTest(), () => new BitcoinRegTest());
            }
        }

        public static NetworksSelector Emocoin
        {
            get
            {
                return new NetworksSelector(() => new EmocoinMain(), () => new EmocoinTest(), () => new EmocoinRegTest());
            }
        }

        public static NetworksSelector Emoc
        {
            get
            {
                return new NetworksSelector(() => new EmocMain(), () => new EmocTest(), () => new EmocRegTest());
            }
        }
    }
}
