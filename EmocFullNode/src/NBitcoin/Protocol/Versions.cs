namespace NBitcoin.Protocol
{
    /// <summary>
    /// Network protocol versioning.
    /// </summary>
    public enum ProtocolVersion : uint
    {
        PROTOCOL_VERSION = 70029,

        ALT_PROTOCOL_VERSION = 70000,

        /// <summary>
        /// Initial protocol version, to be increased after version/verack negotiation.
        /// </summary>
        INIT_PROTO_VERSION = 209,

        /// <summary>
        /// Disconnect from peers older than this protocol version.
        /// </summary>
        MIN_PEER_PROTO_VERSION = 70015,

        /// <summary>
        /// nTime field added to CAddress, starting with this version;
        /// if possible, avoid requesting addresses nodes older than this.
        /// </summary>
        CADDR_TIME_VERSION = 31402,

        /// <summary>
        /// Only request blocks from nodes outside this range of versions (START).
        /// </summary>
        NOBLKS_VERSION_START = 32000,

        /// <summary>
        /// Only request blocks from nodes outside this range of versions (END).
        /// </summary>
        NOBLKS_VERSION_END = 32400,

        /// <summary>
        /// BIP 0031, pong message, is enabled for all versions AFTER this one.
        /// </summary>
        BIP0031_VERSION = 60000,

        /// <summary>
        /// "mempool" command, enhanced "getdata" behavior starts with this version.
        /// </summary>
        MEMPOOL_GD_VERSION = 60002,

        /// <summary>
        /// "reject" command.
        /// </summary>
        REJECT_VERSION = 70002,

        /// <summary>
        /// ! "filter*" commands are disabled without NODE_BLOOM after and including this version.
        /// </summary>
        NO_BLOOM_VERSION = 70011,

        /// <summary>
        /// ! "sendheaders" command and announcing blocks with headers starts with this version.
        /// </summary>
        SENDHEADERS_VERSION = 70029,

        /// <summary>
        /// ! Version after which witness support potentially exists.
        /// </summary>
        WITNESS_VERSION = 70015,

        /// <summary>
        /// Communication between nodes with proven headers is possible after this version.
        /// This is for emocoin only. Temporary solution, refers to issue #2144
        /// https://github.com/emocoinproject/EmocoinBitcoinFullNode/issues/2144
        /// </summary>
        PROVEN_HEADER_VERSION = 70015,

        /// <summary>
        /// shord-id-based block download starts with this version.
        /// </summary>
        SHORT_IDS_BLOCKS_VERSION = 70015,

        //emoc does not use cirrus - set these very high and remove in future
        /// <summary>
        /// Oldest supported version of the CirrusNode which this node can connect to.
        /// </summary>
        CIRRUS_MIN_SUPPORTED_VERSION = 800000,

        /// <summary>
        /// Current version of the CirrusNode.
        /// </summary>
        CIRRUS_VERSION = 800000,
    }
}
