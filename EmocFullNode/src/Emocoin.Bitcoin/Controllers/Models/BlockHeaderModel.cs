using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using Emocoin.Bitcoin.Controllers.Converters;
using Emocoin.Bitcoin.Utilities;

namespace Emocoin.Bitcoin.Controllers.Models
{

    [JsonConverter(typeof(ToStringJsonConverter))]
    public class HexModel
    {
        public string Hex { get; set; }

        public HexModel(string hex)
        {
            this.Hex = hex;
        }

        public override string ToString()
        {
            return this.Hex;
        }
    }

    /// <summary>
    /// Data structure for block headers.
    /// Copied from RPC
    /// <see cref="https://bitcoin.org/en/developer-reference#getblockheader"/>
    /// </summary>
    public class BlockHeaderModel
    {
        /// <summary>
        /// Constructs a BlockHeaderModel from a block header object.
        /// </summary>
        /// <param name="blockHeader">The block header.</param>
        public BlockHeaderModel(BlockHeader blockHeader)
        {
            Guard.NotNull(blockHeader, nameof(blockHeader));

            this.Version = (uint)blockHeader.Version;
            this.PreviousBlockHash = blockHeader.HashPrevBlock.ToString();
            this.MerkleRoot = blockHeader.HashMerkleRoot.ToString();
            this.Time = blockHeader.Time;
            byte[] bytes = this.GetBytes(blockHeader.Bits.ToCompact());
            string encodedBytes = Encoders.Hex.EncodeData(bytes);
            this.Bits = encodedBytes;
            this.Nonce = (int)blockHeader.Nonce;
        }

        /// <summary>
        /// Constructs a BlockHeaderModel.
        /// Used when deserializing block header from Json.
        /// </summary>
        public BlockHeaderModel()
        {
        }

        /// <summary>
        /// The blocks version number.
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public uint Version { get; private set; }

        /// <summary>
        /// The merkle root for this block encoded as hex in RPC byte order.
        /// </summary>
        [JsonProperty(PropertyName = "merkleroot")]
        public string MerkleRoot { get; private set; }

        /// <summary>
        /// The nonce which was successful at turning this particular block
        /// into one that could be added to the best block chain.
        /// </summary>
        [JsonProperty(PropertyName = "nonce")]
        public int Nonce { get; private set; }

        /// <summary>
        /// The target threshold this block's header had to pass.
        /// </summary>
        [JsonProperty(PropertyName = "bits")]
        public string Bits { get; private set; }

        /// <summary>
        /// The hash of the header of the previous block,
        /// encoded as hex in RPC byte order.
        /// </summary>
        [JsonProperty(PropertyName = "previousblockhash")]
        public string PreviousBlockHash { get; private set; }

        /// <summary>
        /// The block time in seconds since epoch (Jan 1 1970 GMT).
        /// </summary>
        [JsonProperty(PropertyName = "time")]
        public uint Time { get; private set; }

        /// <summary>
        /// Convert compact of miner challenge to byte format,
        /// serialized for transmission via RPC.
        /// </summary>
        /// <param name="compact">Compact representation of challenge.</param>
        /// <returns>Byte representation of challenge.</returns>
        /// <seealso cref="Target"/>
        private byte[] GetBytes(uint compact)
        {
            return new byte[]
            {
                (byte)(compact >> 24),
                (byte)(compact >> 16),
                (byte)(compact >> 8),
                (byte)(compact)
            };
        }
    }

    public class BlockRewardEmojiModel
    {
        /// <summary>
        /// Constructs a BlockHeaderModel from a block header object.
        /// </summary>
        /// <param name="blockHeader">The block header.</param>
        public BlockRewardEmojiModel(int height, string emoji, string display)
        {
            this.height = height;
            this.emoji = emoji;
            this.display = display;
        }

        /// <summary>
        /// Constructs a BlockHeaderModel.
        /// Used when deserializing block header from Json.
        /// </summary>
        public BlockRewardEmojiModel()
        {
        }

        /// <summary>
        /// The blocks version number.
        /// </summary>
        [JsonProperty(PropertyName = "height")]
        public int height { get; private set; }

        /// <summary>
        /// The merkle root for this block encoded as hex in RPC byte order.
        /// </summary>
        [JsonProperty(PropertyName = "emoji")]
        public string emoji { get; private set; }

        [JsonProperty(PropertyName = "display")]
        public string display { get; private set; }        
    }

    public class EmojiCodeModel
    {
        /// <summary>
        /// Constructs a BlockHeaderModel from a block header object.
        /// </summary>
        /// <param name="blockHeader">The block header.</param>
        public EmojiCodeModel(string emoji, string display)
        {
            this.emoji = emoji;
            this.display = display;
        }

        /// <summary>
        /// Constructs a BlockHeaderModel.
        /// Used when deserializing block header from Json.
        /// </summary>
        public EmojiCodeModel()
        {
        }

        /// <summary>
        /// The merkle root for this block encoded as hex in RPC byte order.
        /// </summary>
        [JsonProperty(PropertyName = "emoji")]
        public string emoji { get; private set; }

        [JsonProperty(PropertyName = "display")]
        public string display { get; private set; }
    }
}
