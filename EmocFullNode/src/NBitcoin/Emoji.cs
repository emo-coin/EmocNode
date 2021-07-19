//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Dynamic;
using Newtonsoft.Json;
using System.IO;
using System.Numerics;

namespace NBitcoin
{
    class EmojiCombineAlgo
    {
        private int _n = 0;
        private BigInteger _oneEmojiSlotCount = 0;
        private BigInteger _twoEmojiSlotCount = 0;
        private BigInteger _threeEmojiSlotCount = 0;
        private BigInteger _fourEmojiSlotCount = 0;

        public EmojiCombineAlgo(int n)
        {
            _n = n;
            _oneEmojiSlotCount = CalcCount(n, 1);
            _twoEmojiSlotCount = CalcCount(n, 2);
            _threeEmojiSlotCount = CalcCount(n, 3);
            _fourEmojiSlotCount = CalcCount(n, 4);
        }

        private BigInteger CalcCount(BigInteger n, BigInteger r)
        {
            BigInteger result = Factorial(n) / Factorial(n - r);
            return result;
        }
        private BigInteger Factorial(BigInteger integer)
        {
            if (integer < 1) return new BigInteger(1);

            BigInteger result = integer;
            for (BigInteger i = 1; i < integer; i++)
            {
                result = result * i;
            }

            return result;
        }

        private int[] nthPerm(BigInteger myIndex, int n, int r, BigInteger total)
        {
            int j = 0, n1 = n;
            BigInteger temp, index1 = myIndex;

            temp = total;
            List<int> indexList = new List<int>();

            for (int k = 0; k < n; k++)
            {
                indexList.Add(k);
            }

            int[] res = new int[r];

            for (int k = 0; k < r; k++, n1--)
            {
                temp /= n1;
                j = (int)(index1 / temp);
                res[k] = indexList[j];
                index1 -= (temp * j);
                indexList.RemoveAt(j);
            }
            return res;
        }

        public int[] nthSlots(int blockHeight)
        {
            int blockPeriod = 6;
            int nRound = blockHeight / _n;
            int iBlock = nRound % blockPeriod;
            //AAAA, AABB, AAAA, AABC, AAAA, ABCD

            int idx0;
            var idxess0 = new int[4];
            int[] idxes;
            BigInteger idx;
            switch (iBlock)
            {
                case 0:
                case 2:
                case 4:
                    idx0 = blockHeight % _n;
                    idxess0[0] = idxess0[1] = idxess0[2] = idxess0[3] = idx0;
                    break;
                case 1:
                    idx = nRound / blockPeriod;
                    idx = idx * _n + (blockHeight % _n);
                    idx = idx % _twoEmojiSlotCount;
                    idxes = nthPerm(idx, _n, 2, _twoEmojiSlotCount);
                    idxess0[0] = idxess0[1] = idxes[0];
                    idxess0[2] = idxess0[3] = idxes[1];
                    break;
                case 3:
                    idx = nRound / blockPeriod;
                    idx = idx * _n + (blockHeight % _n);
                    idx = idx % _threeEmojiSlotCount;
                    idxes = nthPerm(idx, _n, 3, _threeEmojiSlotCount);
                    idxess0[0] = idxess0[1] = idxes[0];
                    idxess0[2] = idxes[1];
                    idxess0[3] = idxes[2];
                    break;
                case 5:
                    idx = nRound / blockPeriod;
                    idx = idx * _n + (blockHeight % _n);
                    idx = idx % _fourEmojiSlotCount;
                    idxes = nthPerm(idx, _n, 4, _fourEmojiSlotCount);
                    idxess0[0] = idxes[0];
                    idxess0[1] = idxes[1];
                    idxess0[2] = idxes[2];
                    idxess0[3] = idxes[3];
                    break;
            }
            //if (nRound % 2 == 0) //purebeeds
            //{
            //}
            ////mix 
            //int idx = (nRound / 2) * _n + (blockHeight % _n);
            //idx = idx % (int)_twoEmojiSlotCount;
            //var idxes = nthPerm(idx, _n, 2, _twoEmojiSlotCount);
            //var idxess = new int[4] { idxes[0], idxes[0], idxes[1], idxes[1] };
            return idxess0;
        }
    }

    /// <summary>
    /// Emoji pertaining to each transaction.
    /// </summary>
    //[DebuggerDisplay("{this[0].Code, this[1].Code, this[2].Code, this[3].Code}")]
    public class Emoji : IBitcoinSerializable
    {
        #region IBitcoinSerializable Members
        public void ReadWrite(BitcoinStream stream)
        {
            //using (stream.BigEndianScope())//???
            //{
            if (stream.Serializing)
            {
                for (int i = 0; i < this.genes.Length; i++)
                    stream.ReadWrite(this[i].Code);
            }
            else
            {
                for (int i = 0; i < this.genes.Length; i++)
                {
                    uint v = this[i].Code;
                    stream.ReadWrite(ref v);
                    this.genes[i] = Gene.GetByCode(v);
                }
            }
            //}
        }
        #endregion

        /// <summary>
        /// Emoji consists of 4 genes.
        /// </summary>
        public Gene this[int i]
        {
            get
            {
                return this.genes[i];
            }
        }
        Gene[] genes;
        public Gene[] Genes()
        {
            return this.genes;
        }


        public bool Equals(Emoji p)
        {
            if (p is null)
            {
                return false;
            }
            var srcGenes = p.Genes();
            if (this.genes == null && srcGenes == null)
                return true;
            if (srcGenes.Length != this.genes.Length)
                return false;

            for(int i=0; i< this.genes.Length; i++)
            {
                if (srcGenes[i].Code != this.genes[i].Code)
                    return false;
            }
            return true;
        }

        public static bool operator !=(Emoji lhs, Emoji rhs) => !(lhs == rhs);
        public static bool operator ==(Emoji lhs, Emoji rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static readonly Emoji Empty = new Emoji(Gene.Empty, Gene.Empty, Gene.Empty, Gene.Empty);

        public bool isEmpty()
        {
            bool bEmpty = false;
            foreach(var gen in this.genes)
            {
                if(gen == Gene.Empty)
                {
                    bEmpty = true;
                }
            }
            return bEmpty;
        }
        public Emoji()//used by deserializer
        {
            this.genes = new Gene[4] { Gene.Empty, Gene.Empty, Gene.Empty, Gene.Empty };
        }

        /// <summary>
        /// To hex string ordered as bigendian.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            byte[] buffer = new byte[GENE_ARRAY_LENGTH * GENE_CODE_BYTE_ARRAY_LENGTH];
            for (int i = 0; i < this.genes.Length; i++)
            {
                byte[] bs = Utils.ToBytes(this.genes[i].Code, false);//to big endian
                Array.Copy(bs, 4 - GENE_CODE_BYTE_ARRAY_LENGTH, buffer, i * GENE_CODE_BYTE_ARRAY_LENGTH, GENE_CODE_BYTE_ARRAY_LENGTH);
            }
            return DataEncoders.Encoders.Hex.EncodeData(buffer);
        }

        public string ToStringUnicode()
        {
            string res = "";
            foreach(var gene in this.genes)
            {
                res += Char.ConvertFromUtf32((int)gene.Code);
            }
            return res;
        }

        const int GENE_CODE_BYTE_ARRAY_LENGTH = 3;//to have a shorter string. Standard: 4
        const int GENE_ARRAY_LENGTH = 4;

        /// <summary>
        /// Restore emoji from hex string ordered as bigendian.  
        /// </summary>
        /// <param name="hexString"></param>
        public Emoji(string hexString)
        {
            if (hexString == null)
            {
                this.genes = new Gene[GENE_ARRAY_LENGTH] { Gene.Empty, Gene.Empty, Gene.Empty, Gene.Empty };
                return;
            }
            hexString = hexString.Trim();
            if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hexString = hexString.Substring(2);
            byte[] buffer = DataEncoders.Encoders.Hex.DecodeData(hexString);//expect it bigendian
            if (buffer.Length == GENE_ARRAY_LENGTH * 4) { }
            else if (buffer.Length == GENE_ARRAY_LENGTH * 3)//if hex string contains 3 bytes per uint
            {
                byte[] bs = buffer;
                buffer = new byte[GENE_ARRAY_LENGTH * 4];
                for (int i = 0; i < GENE_ARRAY_LENGTH; i++)
                    Array.Copy(bs, i*3, buffer, i * 4 + 1, 3);
            }
            else
                throw new FormatException("Invalid hex length: " + buffer.Length);
            this.genes = new Gene[GENE_ARRAY_LENGTH];
            for (int i = 0; i < this.genes.Length; i++)
            {
                this.genes[i] = Gene.GetByCode(Utils.ToUInt32(buffer, 4 * i, false));
                if(this.genes[i] == Gene.Empty)
                    throw new FormatException("Invalid hex length: " + buffer.Length);
            }                
        }

        public Emoji(uint code0, uint code1, uint code2, uint code3)
        {
            this.genes = new Gene[GENE_ARRAY_LENGTH] { 
                Gene.GetByCode(code0),
                Gene.GetByCode(code1),
                Gene.GetByCode(code2),
                Gene.GetByCode(code3),
            };
        }

        public Emoji(Gene gene0, Gene gene1, Gene gene2, Gene gene3)
        {
            this.genes = new Gene[GENE_ARRAY_LENGTH] { gene0, gene1, gene2, gene3 };
        }

        /// <summary>
        /// Get a pure breed emoji.
        /// </summary>
        public static Emoji GetRandomPureBreed()
        {
            Gene g = Gene.GetRandom();
            return new Emoji(g, g, g, g);
        }

        public static Emoji GetSlotsByBlockHeight(int height)
        {
            var indexes = _algo.nthSlots(height);
            return new Emoji(Gene.genes[indexes[0]+1] ,
                Gene.genes[indexes[1]+1],
                Gene.genes[indexes[2]+1],
                Gene.genes[indexes[3]+1]);
        }
        

        /// <summary>
        /// Emoji gene constituting an emoji
        /// </summary>
        public class Gene
        {
            public readonly uint Code;
            public readonly string Name;

            internal Gene(uint code, string name)
            {
                this.Code = code;
                this.Name = name;
            }

            static public readonly Gene Empty = new Gene(0, "EMPTY");

            public override string ToString()
            {
                return this.Code.ToString();
            }

            static public Gene GetByCode(uint code)
            {
                if (geneCodes2Gene.TryGetValue(code, out Gene g))
                    return g;
                return Empty;
            }


            public static Gene GetRandom()
            {
                int i = random.Next(1, geneCodes2Gene.Count);//!!!exclude EMPTY
                return genes[i];
            }
            static Random random = new Random((int)(DateTime.Now.Ticks & 0xFFFFFFFF));

            static Gene()
            {
                foreach (Gene g in genes)
                    geneCodes2Gene[g.Code] = g;                
                //string str = JsonConvert.SerializeObject(genes);
                //File.WriteAllText("d:\\emojis.json", str);
            }

            static readonly Dictionary<uint, Gene> geneCodes2Gene = new Dictionary<uint, Gene>();
            static public readonly List<Gene> genes = new List<Gene>
            {
            Gene.Empty,
new Gene(0x1f600, "face-smiling - grinning face"),
new Gene(0x1f603, "face-smiling - grinning face with big eyes"),
new Gene(0x1f604, "face-smiling - grinning face with smiling eyes"),
new Gene(0x1f601, "face-smiling - beaming face with smiling eyes"),
new Gene(0x1f606, "face-smiling - grinning squinting face"),
new Gene(0x1f605, "face-smiling - grinning face with sweat"),
new Gene(0x1f923, "face-smiling - rolling on the floor laughing"),
new Gene(0x1f602, "face-smiling - face with tears of joy"),
new Gene(0x1f642, "face-smiling - slightly smiling face"),
new Gene(0x1f643, "face-smiling - upside-down face"),
new Gene(0x1f609, "face-smiling - winking face"),
new Gene(0x1f60a, "face-smiling - smiling face with smiling eyes"),
new Gene(0x1f607, "face-smiling - smiling face with halo"),
new Gene(0x1f970, "face-affection - smiling face with 3 hearts"),
new Gene(0x1f60d, "face-affection - smiling face with heart-eyes"),
new Gene(0x1f929, "face-affection - star-struck"),
new Gene(0x1f618, "face-affection - face blowing a kiss"),
new Gene(0x1f617, "face-affection - kissing face"),
new Gene(0x263a, "face-affection - smiling face"),
new Gene(0x1f61a, "face-affection - kissing face with closed eyes"),
new Gene(0x1f619, "face-affection - kissing face with smiling eyes"),
new Gene(0x1f60b, "face-tongue - face savoring food"),
new Gene(0x1f61b, "face-tongue - face with tongue"),
new Gene(0x1f61c, "face-tongue - winking face with tongue"),
new Gene(0x1f92a, "face-tongue - zany face"),
new Gene(0x1f61d, "face-tongue - squinting face with tongue"),
new Gene(0x1f911, "face-tongue - money-mouth face"),
new Gene(0x1f917, "face-hand - hugging face"),
new Gene(0x1f92d, "face-hand - face with hand over mouth"),
new Gene(0x1f92b, "face-hand - shushing face"),
new Gene(0x1f914, "face-hand - thinking face"),
new Gene(0x1f910, "face-neutral-skeptical - zipper-mouth face"),
new Gene(0x1f928, "face-neutral-skeptical - face with raised eyebrow"),
new Gene(0x1f610, "face-neutral-skeptical - neutral face"),
new Gene(0x1f611, "face-neutral-skeptical - expressionless face"),
new Gene(0x1f636, "face-neutral-skeptical - face without mouth"),
new Gene(0x1f60f, "face-neutral-skeptical - smirking face"),
new Gene(0x1f612, "face-neutral-skeptical - unamused face"),
new Gene(0x1f644, "face-neutral-skeptical - face with rolling eyes"),
new Gene(0x1f62c, "face-neutral-skeptical - grimacing face"),
new Gene(0x1f925, "face-neutral-skeptical - lying face"),
new Gene(0x1f60c, "face-sleepy - relieved face"),
new Gene(0x1f614, "face-sleepy - pensive face"),
new Gene(0x1f62a, "face-sleepy - sleepy face"),
new Gene(0x1f924, "face-sleepy - drooling face"),
new Gene(0x1f634, "face-sleepy - sleeping face"),
new Gene(0x1f637, "face-unwell - face with medical mask"),
new Gene(0x1f912, "face-unwell - face with thermometer"),
new Gene(0x1f915, "face-unwell - face with head-bandage"),
new Gene(0x1f922, "face-unwell - nauseated face"),
new Gene(0x1f92e, "face-unwell - face vomiting"),
new Gene(0x1f927, "face-unwell - sneezing face"),
new Gene(0x1f975, "face-unwell - hot face"),
new Gene(0x1f976, "face-unwell - cold face"),
new Gene(0x1f974, "face-unwell - woozy face"),
new Gene(0x1f635, "face-unwell - dizzy face"),
new Gene(0x1f92f, "face-unwell - exploding head"),
new Gene(0x1f920, "face-hat - cowboy hat face"),
new Gene(0x1f973, "face-hat - partying face"),
new Gene(0x1f60e, "face-glasses - smiling face with sunglasses"),
new Gene(0x1f913, "face-glasses - nerd face"),
new Gene(0x1f9d0, "face-glasses - face with monocle"),
new Gene(0x1f615, "face-concerned - confused face"),
new Gene(0x1f61f, "face-concerned - worried face"),
new Gene(0x1f641, "face-concerned - slightly frowning face"),
new Gene(0x2639, "face-concerned - frowning face"),
new Gene(0x1f62e, "face-concerned - face with open mouth"),
new Gene(0x1f62f, "face-concerned - hushed face"),
new Gene(0x1f632, "face-concerned - astonished face"),
new Gene(0x1f633, "face-concerned - flushed face"),
new Gene(0x1f97a, "face-concerned - pleading face"),
new Gene(0x1f626, "face-concerned - frowning face with open mouth"),
new Gene(0x1f627, "face-concerned - anguished face"),
new Gene(0x1f628, "face-concerned - fearful face"),
new Gene(0x1f630, "face-concerned - anxious face with sweat"),
new Gene(0x1f625, "face-concerned - sad but relieved face"),
new Gene(0x1f622, "face-concerned - crying face"),
new Gene(0x1f62d, "face-concerned - loudly crying face"),
new Gene(0x1f631, "face-concerned - face screaming in fear"),
new Gene(0x1f616, "face-concerned - confounded face"),
new Gene(0x1f623, "face-concerned - persevering face"),
new Gene(0x1f61e, "face-concerned - disappointed face"),
new Gene(0x1f613, "face-concerned - downcast face with sweat"),
new Gene(0x1f629, "face-concerned - weary face"),
new Gene(0x1f62b, "face-concerned - tired face"),
new Gene(0x1f624, "face-negative - face with steam from nose"),
new Gene(0x1f621, "face-negative - pouting face"),
new Gene(0x1f620, "face-negative - angry face"),
new Gene(0x1f92c, "face-negative - face with symbols on mouth"),
new Gene(0x1f608, "face-negative - smiling face with horns"),
new Gene(0x1f47f, "face-negative - angry face with horns"),
new Gene(0x1f480, "face-negative - skull"),
new Gene(0x2620, "face-negative - skull and crossbones"),
new Gene(0x1f4a9, "face-costume - pile of poo"),
new Gene(0x1f921, "face-costume - clown face"),
new Gene(0x1f479, "face-costume - ogre"),
new Gene(0x1f47a, "face-costume - goblin"),
new Gene(0x1f47b, "face-costume - ghost"),
new Gene(0x1f47d, "face-costume - alien"),
new Gene(0x1f47e, "face-costume - alien monster"),
new Gene(0x1f916, "face-costume - robot face"),
new Gene(0x1f63a, "cat-face - grinning cat face"),
new Gene(0x1f638, "cat-face - grinning cat face with smiling eyes"),
new Gene(0x1f639, "cat-face - cat face with tears of joy"),
new Gene(0x1f63b, "cat-face - smiling cat face with heart-eyes"),
new Gene(0x1f63c, "cat-face - cat face with wry smile"),
new Gene(0x1f63d, "cat-face - kissing cat face"),
new Gene(0x1f640, "cat-face - weary cat face"),
new Gene(0x1f63f, "cat-face - crying cat face"),
new Gene(0x1f63e, "cat-face - pouting cat face"),
new Gene(0x1f648, "monkey-face - see-no-evil monkey"),
new Gene(0x1f649, "monkey-face - hear-no-evil monkey"),
new Gene(0x1f64a, "monkey-face - speak-no-evil monkey"),
new Gene(0x1f48b, "emotion - kiss mark"),
new Gene(0x1f48c, "emotion - love letter"),
new Gene(0x1f498, "emotion - heart with arrow"),
new Gene(0x1f49d, "emotion - heart with ribbon"),
new Gene(0x1f496, "emotion - sparkling heart"),
new Gene(0x1f497, "emotion - growing heart"),
new Gene(0x1f493, "emotion - beating heart"),
new Gene(0x1f49e, "emotion - revolving hearts"),
new Gene(0x1f495, "emotion - two hearts"),
new Gene(0x1f49f, "emotion - heart decoration"),
new Gene(0x2763, "emotion - heavy heart exclamation"),
new Gene(0x1f494, "emotion - broken heart"),
new Gene(0x2764, "emotion - red heart"),
new Gene(0x1f9e1, "emotion - orange heart"),
new Gene(0x1f49b, "emotion - yellow heart"),
new Gene(0x1f49a, "emotion - green heart"),
new Gene(0x1f499, "emotion - blue heart"),
new Gene(0x1f49c, "emotion - purple heart"),
new Gene(0x1f5a4, "emotion - black heart"),
new Gene(0x1f4af, "emotion - hundred points"),
new Gene(0x1f4a2, "emotion - anger symbol"),
new Gene(0x1f4a5, "emotion - collision"),
new Gene(0x1f4ab, "emotion - dizzy"),
new Gene(0x1f4a6, "emotion - sweat droplets"),
new Gene(0x1f4a8, "emotion - dashing away"),
new Gene(0x1f573, "emotion - hole"),
new Gene(0x1f4a3, "emotion - bomb"),
new Gene(0x1f4ac, "emotion - speech balloon"),
                      // vBaseEmojis.Add(0x1f441 u+fe0f u+200d u+1f5e8 u+fe0f);	// emotion - eye in speech bubble
new Gene(0x1f5e8, "emotion - left speech bubble"),
new Gene(0x1f5ef, "emotion - right anger bubble"),
new Gene(0x1f4ad, "emotion - thought balloon"),
new Gene(0x1f4a4, "emotion - zzz"),
new Gene(0x1f44b, "hand-fingers-open - waving hand"),
new Gene(0x1f91a, "hand-fingers-open - raised back of hand"),
new Gene(0x1f590, "hand-fingers-open - hand with fingers splayed"),
new Gene(0x270b, "hand-fingers-open - raised hand"),
new Gene(0x1f596, "hand-fingers-open - vulcan salute"),
new Gene(0x1f44c, "hand-fingers-partial - OK hand"),
new Gene(0x270c, "hand-fingers-partial - victory hand"),
new Gene(0x1f91e, "hand-fingers-partial - crossed fingers"),
new Gene(0x1f91f, "hand-fingers-partial - love-you gesture"),
new Gene(0x1f918, "hand-fingers-partial - sign of the horns"),
new Gene(0x1f919, "hand-fingers-partial - call me hand"),
new Gene(0x1f448, "hand-single-finger - backhand index pointing left"),
new Gene(0x1f449, "hand-single-finger - backhand index pointing right"),
new Gene(0x1f446, "hand-single-finger - backhand index pointing up"),
new Gene(0x1f595, "hand-single-finger - middle finger"),
new Gene(0x1f447, "hand-single-finger - backhand index pointing down"),
new Gene(0x261d, "hand-single-finger - index pointing up"),
new Gene(0x1f44d, "hand-fingers-closed - thumbs up"),
new Gene(0x1f44e, "hand-fingers-closed - thumbs down"),
new Gene(0x270a, "hand-fingers-closed - raised fist"),
new Gene(0x1f44a, "hand-fingers-closed - oncoming fist"),
new Gene(0x1f91b, "hand-fingers-closed - left-facing fist"),
new Gene(0x1f91c, "hand-fingers-closed - right-facing fist"),
new Gene(0x1f44f, "hands - clapping hands"),
new Gene(0x1f64c, "hands - raising hands"),
new Gene(0x1f450, "hands - open hands"),
new Gene(0x1f932, "hands - palms up together"),
new Gene(0x1f91d, "hands - handshake"),
new Gene(0x1f64f, "hands - folded hands"),
new Gene(0x270d, "hand-prop - writing hand"),
new Gene(0x1f485, "hand-prop - nail polish"),
new Gene(0x1f933, "hand-prop - selfie"),
new Gene(0x1f4aa, "body-parts - flexed biceps"),
new Gene(0x1f9b5, "body-parts - leg"),
new Gene(0x1f9b6, "body-parts - foot"),
new Gene(0x1f442, "body-parts - ear"),
new Gene(0x1f443, "body-parts - nose"),
new Gene(0x1f9e0, "body-parts - brain"),
new Gene(0x1f9b7, "body-parts - tooth"),
new Gene(0x1f9b4, "body-parts - bone"),
new Gene(0x1f440, "body-parts - eyes"),
new Gene(0x1f441, "body-parts - eye"),
new Gene(0x1f445, "body-parts - tongue"),
new Gene(0x1f444, "body-parts - mouth"),
new Gene(0x1f476, "person - baby"),
new Gene(0x1f9d2, "person - child"),
new Gene(0x1f466, "person - boy"),
new Gene(0x1f467, "person - girl"),
new Gene(0x1f9d1, "person - person"),
new Gene(0x1f471, "person - person: blond hair"),
new Gene(0x1f468, "person - man"),
// new Gene(0x1f471, "person - man: blond hair"),
// new Gene(0x1f468, "person - man: red hair"),
// new Gene(0x1f468, "person - man: curly hair"),
// new Gene(0x1f468, "person - man: white hair"),
// new Gene(0x1f468, "person - man: bald"),
new Gene(0x1f9d4, "person - man: beard"),
new Gene(0x1f469, "person - woman"),
// new Gene(0x1f471, "person - woman: blond hair"),
// new Gene(0x1f469, "person - woman: red hair"),
// new Gene(0x1f469, "person - woman: curly hair"),
// new Gene(0x1f469, "person - woman: white hair"),
// new Gene(0x1f469, "person - woman: bald"),
new Gene(0x1f9d3, "person - older person"),
new Gene(0x1f474, "person - old man"),
new Gene(0x1f475, "person - old woman"),
new Gene(0x1f64d, "person-gesture - person frowning"),
                      // vBaseEmojis.Add(0x1f64d u+200d u+2642 u+fe0f);	// person-gesture - man frowning
                      // vBaseEmojis.Add(0x1f64d u+200d u+2640 u+fe0f);	// person-gesture - woman frowning
new Gene(0x1f64e, "person-gesture - person pouting"),
                      // vBaseEmojis.Add(0x1f64e u+200d u+2642 u+fe0f);	// person-gesture - man pouting
                      // vBaseEmojis.Add(0x1f64e u+200d u+2640 u+fe0f);	// person-gesture - woman pouting
new Gene(0x1f645, "person-gesture - person gesturing NO"),
                      // vBaseEmojis.Add(0x1f645 u+200d u+2642 u+fe0f);	// person-gesture - man gesturing NO
                      // vBaseEmojis.Add(0x1f645 u+200d u+2640 u+fe0f);	// person-gesture - woman gesturing NO
new Gene(0x1f646, "person-gesture - person gesturing OK"),
                      // vBaseEmojis.Add(0x1f646 u+200d u+2642 u+fe0f);	// person-gesture - man gesturing OK
                      // vBaseEmojis.Add(0x1f646 u+200d u+2640 u+fe0f);	// person-gesture - woman gesturing OK
new Gene(0x1f481, "person-gesture - person tipping hand"),
                      // vBaseEmojis.Add(0x1f481 u+200d u+2642 u+fe0f);	// person-gesture - man tipping hand
                      // vBaseEmojis.Add(0x1f481 u+200d u+2640 u+fe0f);	// person-gesture - woman tipping hand
new Gene(0x1f64b, "person-gesture - person raising hand"),
                      // vBaseEmojis.Add(0x1f64b u+200d u+2642 u+fe0f);	// person-gesture - man raising hand
                      // vBaseEmojis.Add(0x1f64b u+200d u+2640 u+fe0f);	// person-gesture - woman raising hand
new Gene(0x1f647, "person-gesture - person bowing"),
                      // vBaseEmojis.Add(0x1f647 u+200d u+2642 u+fe0f);	// person-gesture - man bowing
                      // vBaseEmojis.Add(0x1f647 u+200d u+2640 u+fe0f);	// person-gesture - woman bowing
new Gene(0x1f926, "person-gesture - person facepalming"),
                      // vBaseEmojis.Add(0x1f926 u+200d u+2642 u+fe0f);	// person-gesture - man facepalming
                      // vBaseEmojis.Add(0x1f926 u+200d u+2640 u+fe0f);	// person-gesture - woman facepalming
new Gene(0x1f937, "person-gesture - person shrugging"),
                      // vBaseEmojis.Add(0x1f937 u+200d u+2642 u+fe0f);	// person-gesture - man shrugging
                      // vBaseEmojis.Add(0x1f937 u+200d u+2640 u+fe0f);	// person-gesture - woman shrugging
                      // vBaseEmojis.Add(0x1f468 u+200d u+2695 u+fe0f);	// person-role - man health worker
                      // vBaseEmojis.Add(0x1f469 u+200d u+2695 u+fe0f);	// person-role - woman health worker
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f393);	// person-role - man student
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f393);	// person-role - woman student
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f3eb);	// person-role - man teacher
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f3eb);	// person-role - woman teacher
                      // vBaseEmojis.Add(0x1f468 u+200d u+2696 u+fe0f);	// person-role - man judge
                      // vBaseEmojis.Add(0x1f469 u+200d u+2696 u+fe0f);	// person-role - woman judge
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f33e);	// person-role - man farmer
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f33e);	// person-role - woman farmer
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f373);	// person-role - man cook
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f373);	// person-role - woman cook
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f527);	// person-role - man mechanic
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f527);	// person-role - woman mechanic
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f3ed);	// person-role - man factory worker
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f3ed);	// person-role - woman factory worker
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f4bc);	// person-role - man office worker
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f4bc);	// person-role - woman office worker
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f52c);	// person-role - man scientist
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f52c);	// person-role - woman scientist
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f4bb);	// person-role - man technologist
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f4bb);	// person-role - woman technologist
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f3a4);	// person-role - man singer
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f3a4);	// person-role - woman singer
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f3a8);	// person-role - man artist
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f3a8);	// person-role - woman artist
                      // vBaseEmojis.Add(0x1f468 u+200d u+2708 u+fe0f);	// person-role - man pilot
                      // vBaseEmojis.Add(0x1f469 u+200d u+2708 u+fe0f);	// person-role - woman pilot
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f680);	// person-role - man astronaut
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f680);	// person-role - woman astronaut
                      // vBaseEmojis.Add(0x1f468 u+200d u+1f692);	// person-role - man firefighter
                      // vBaseEmojis.Add(0x1f469 u+200d u+1f692);	// person-role - woman firefighter
new Gene(0x1f46e, "person-role - police officer"),
                      // vBaseEmojis.Add(0x1f46e u+200d u+2642 u+fe0f);	// person-role - man police officer
                      // vBaseEmojis.Add(0x1f46e u+200d u+2640 u+fe0f);	// person-role - woman police officer
new Gene(0x1f575, "person-role - detective"),
                      // vBaseEmojis.Add(0x1f575 u+fe0f u+200d u+2642 u+fe0f);	// person-role - man detective
                      // vBaseEmojis.Add(0x1f575 u+fe0f u+200d u+2640 u+fe0f);	// person-role - woman detective
new Gene(0x1f482, "person-role - guard"),
                      // vBaseEmojis.Add(0x1f482 u+200d u+2642 u+fe0f);	// person-role - man guard
                      // vBaseEmojis.Add(0x1f482 u+200d u+2640 u+fe0f);	// person-role - woman guard
new Gene(0x1f477, "person-role - construction worker"),
                      // vBaseEmojis.Add(0x1f477 u+200d u+2642 u+fe0f);	// person-role - man construction worker
                      // vBaseEmojis.Add(0x1f477 u+200d u+2640 u+fe0f);	// person-role - woman construction worker
new Gene(0x1f934, "person-role - prince"),
new Gene(0x1f478, "person-role - princess"),
new Gene(0x1f473, "person-role - person wearing turban"),
                      // vBaseEmojis.Add(0x1f473 u+200d u+2642 u+fe0f);	// person-role - man wearing turban
                      // vBaseEmojis.Add(0x1f473 u+200d u+2640 u+fe0f);	// person-role - woman wearing turban
new Gene(0x1f472, "person-role - man with Chinese cap"),
new Gene(0x1f9d5, "person-role - woman with headscarf"),
new Gene(0x1f935, "person-role - man in tuxedo"),
new Gene(0x1f470, "person-role - bride with veil"),
new Gene(0x1f930, "person-role - pregnant woman"),
new Gene(0x1f931, "person-role - breast-feeding"),
new Gene(0x1f47c, "person-fantasy - baby angel"),
new Gene(0x1f385, "person-fantasy - Santa Claus"),
new Gene(0x1f936, "person-fantasy - Mrs. Claus"),
new Gene(0x1f9b8, "person-fantasy - superhero"),
                      // vBaseEmojis.Add(0x1f9b8 u+200d u+2642 u+fe0f);	// person-fantasy - man superhero
                      // vBaseEmojis.Add(0x1f9b8 u+200d u+2640 u+fe0f);	// person-fantasy - woman superhero
new Gene(0x1f9b9, "person-fantasy - supervillain"),
                      // vBaseEmojis.Add(0x1f9b9 u+200d u+2642 u+fe0f);	// person-fantasy - man supervillain
                      // vBaseEmojis.Add(0x1f9b9 u+200d u+2640 u+fe0f);	// person-fantasy - woman supervillain
new Gene(0x1f9d9, "person-fantasy - mage"),
                      // vBaseEmojis.Add(0x1f9d9 u+200d u+2642 u+fe0f);	// person-fantasy - man mage
                      // vBaseEmojis.Add(0x1f9d9 u+200d u+2640 u+fe0f);	// person-fantasy - woman mage
new Gene(0x1f9da, "person-fantasy - fairy"),
                      // vBaseEmojis.Add(0x1f9da u+200d u+2642 u+fe0f);	// person-fantasy - man fairy
                      // vBaseEmojis.Add(0x1f9da u+200d u+2640 u+fe0f);	// person-fantasy - woman fairy
new Gene(0x1f9db, "person-fantasy - vampire"),
                      // vBaseEmojis.Add(0x1f9db u+200d u+2642 u+fe0f);	// person-fantasy - man vampire
                      // vBaseEmojis.Add(0x1f9db u+200d u+2640 u+fe0f);	// person-fantasy - woman vampire
new Gene(0x1f9dc, "person-fantasy - merperson"),
                      // vBaseEmojis.Add(0x1f9dc u+200d u+2642 u+fe0f);	// person-fantasy - merman
                      // vBaseEmojis.Add(0x1f9dc u+200d u+2640 u+fe0f);	// person-fantasy - mermaid
new Gene(0x1f9dd, "person-fantasy - elf"),
                      // vBaseEmojis.Add(0x1f9dd u+200d u+2642 u+fe0f);	// person-fantasy - man elf
                      // vBaseEmojis.Add(0x1f9dd u+200d u+2640 u+fe0f);	// person-fantasy - woman elf
new Gene(0x1f9de, "person-fantasy - genie"),
                      // vBaseEmojis.Add(0x1f9de u+200d u+2642 u+fe0f);	// person-fantasy - man genie
                      // vBaseEmojis.Add(0x1f9de u+200d u+2640 u+fe0f);	// person-fantasy - woman genie
new Gene(0x1f9df, "person-fantasy - zombie"),
                      // vBaseEmojis.Add(0x1f9df u+200d u+2642 u+fe0f);	// person-fantasy - man zombie
                      // vBaseEmojis.Add(0x1f9df u+200d u+2640 u+fe0f);	// person-fantasy - woman zombie
new Gene(0x1f486, "person-activity - person getting massage"),
                      // vBaseEmojis.Add(0x1f486 u+200d u+2642 u+fe0f);	// person-activity - man getting massage
                      // vBaseEmojis.Add(0x1f486 u+200d u+2640 u+fe0f);	// person-activity - woman getting massage
new Gene(0x1f487, "person-activity - person getting haircut"),
                      // vBaseEmojis.Add(0x1f487 u+200d u+2642 u+fe0f);	// person-activity - man getting haircut
                      // vBaseEmojis.Add(0x1f487 u+200d u+2640 u+fe0f);	// person-activity - woman getting haircut
new Gene(0x1f6b6, "person-activity - person walking"),
                      // vBaseEmojis.Add(0x1f6b6 u+200d u+2642 u+fe0f);	// person-activity - man walking
                      // vBaseEmojis.Add(0x1f6b6 u+200d u+2640 u+fe0f);	// person-activity - woman walking
new Gene(0x1f3c3, "person-activity - person running"),
                      // vBaseEmojis.Add(0x1f3c3 u+200d u+2642 u+fe0f);	// person-activity - man running
                      // vBaseEmojis.Add(0x1f3c3 u+200d u+2640 u+fe0f);	// person-activity - woman running
new Gene(0x1f483, "person-activity - woman dancing"),
new Gene(0x1f57a, "person-activity - man dancing"),
new Gene(0x1f574, "person-activity - man in suit levitating"),
new Gene(0x1f46f, "person-activity - people with bunny ears"),
                      // vBaseEmojis.Add(0x1f46f u+200d u+2642 u+fe0f);	// person-activity - men with bunny ears
                      // vBaseEmojis.Add(0x1f46f u+200d u+2640 u+fe0f);	// person-activity - women with bunny ears
new Gene(0x1f9d6, "person-activity - person in steamy room"),
                      // vBaseEmojis.Add(0x1f9d6 u+200d u+2642 u+fe0f);	// person-activity - man in steamy room
                      // vBaseEmojis.Add(0x1f9d6 u+200d u+2640 u+fe0f);	// person-activity - woman in steamy room
new Gene(0x1f9d7, "person-activity - person climbing"),
                      // vBaseEmojis.Add(0x1f9d7 u+200d u+2642 u+fe0f);	// person-activity - man climbing
                      // vBaseEmojis.Add(0x1f9d7 u+200d u+2640 u+fe0f);	// person-activity - woman climbing
new Gene(0x1f93a, "person-sport - person fencing"),
new Gene(0x1f3c7, "person-sport - horse racing"),
new Gene(0x26f7, "person-sport - skier"),
new Gene(0x1f3c2, "person-sport - snowboarder"),
new Gene(0x1f3cc, "person-sport - person golfing"),
                      // vBaseEmojis.Add(0x1f3cc u+fe0f u+200d u+2642 u+fe0f);	// person-sport - man golfing
                      // vBaseEmojis.Add(0x1f3cc u+fe0f u+200d u+2640 u+fe0f);	// person-sport - woman golfing
new Gene(0x1f3c4, "person-sport - person surfing"),
                      // vBaseEmojis.Add(0x1f3c4 u+200d u+2642 u+fe0f);	// person-sport - man surfing
                      // vBaseEmojis.Add(0x1f3c4 u+200d u+2640 u+fe0f);	// person-sport - woman surfing
new Gene(0x1f6a3, "person-sport - person rowing boat"),
                      // vBaseEmojis.Add(0x1f6a3 u+200d u+2642 u+fe0f);	// person-sport - man rowing boat
                      // vBaseEmojis.Add(0x1f6a3 u+200d u+2640 u+fe0f);	// person-sport - woman rowing boat
new Gene(0x1f3ca, "person-sport - person swimming"),
                      // vBaseEmojis.Add(0x1f3ca u+200d u+2642 u+fe0f);	// person-sport - man swimming
                      // vBaseEmojis.Add(0x1f3ca u+200d u+2640 u+fe0f);	// person-sport - woman swimming
new Gene(0x26f9, "person-sport - person bouncing ball"),
                      // vBaseEmojis.Add(0x26f9 u+fe0f u+200d u+2642 u+fe0f);	// person-sport - man bouncing ball
                      // vBaseEmojis.Add(0x26f9 u+fe0f u+200d u+2640 u+fe0f);	// person-sport - woman bouncing ball
new Gene(0x1f3cb, "person-sport - person lifting weights"),
                      // vBaseEmojis.Add(0x1f3cb u+fe0f u+200d u+2642 u+fe0f);	// person-sport - man lifting weights
                      // vBaseEmojis.Add(0x1f3cb u+fe0f u+200d u+2640 u+fe0f);	// person-sport - woman lifting weights
new Gene(0x1f6b4, "person-sport - person biking"),
                      // vBaseEmojis.Add(0x1f6b4 u+200d u+2642 u+fe0f);	// person-sport - man biking
                      // vBaseEmojis.Add(0x1f6b4 u+200d u+2640 u+fe0f);	// person-sport - woman biking
new Gene(0x1f6b5, "person-sport - person mountain biking"),
                      // vBaseEmojis.Add(0x1f6b5 u+200d u+2642 u+fe0f);	// person-sport - man mountain biking
                      // vBaseEmojis.Add(0x1f6b5 u+200d u+2640 u+fe0f);	// person-sport - woman mountain biking
new Gene(0x1f938, "person-sport - person cartwheeling"),
                      // vBaseEmojis.Add(0x1f938 u+200d u+2642 u+fe0f);	// person-sport - man cartwheeling
                      // vBaseEmojis.Add(0x1f938 u+200d u+2640 u+fe0f);	// person-sport - woman cartwheeling
new Gene(0x1f93c, "person-sport - people wrestling"),
                      // vBaseEmojis.Add(0x1f93c u+200d u+2642 u+fe0f);	// person-sport - men wrestling
                      // vBaseEmojis.Add(0x1f93c u+200d u+2640 u+fe0f);	// person-sport - women wrestling
new Gene(0x1f93d, "person-sport - person playing water polo"),
                      // vBaseEmojis.Add(0x1f93d u+200d u+2642 u+fe0f);	// person-sport - man playing water polo
                      // vBaseEmojis.Add(0x1f93d u+200d u+2640 u+fe0f);	// person-sport - woman playing water polo
new Gene(0x1f93e, "person-sport - person playing handball"),
                      // vBaseEmojis.Add(0x1f93e u+200d u+2642 u+fe0f);	// person-sport - man playing handball
                      // vBaseEmojis.Add(0x1f93e u+200d u+2640 u+fe0f);	// person-sport - woman playing handball
new Gene(0x1f939, "person-sport - person juggling"),
                      // vBaseEmojis.Add(0x1f939 u+200d u+2642 u+fe0f);	// person-sport - man juggling
                      // vBaseEmojis.Add(0x1f939 u+200d u+2640 u+fe0f);	// person-sport - woman juggling
new Gene(0x1f9d8, "person-resting - person in lotus position"),
                      // vBaseEmojis.Add(0x1f9d8 u+200d u+2642 u+fe0f);	// person-resting - man in lotus position
                      // vBaseEmojis.Add(0x1f9d8 u+200d u+2640 u+fe0f);	// person-resting - woman in lotus position
new Gene(0x1f6c0, "person-resting - person taking bath"),
new Gene(0x1f6cc, "person-resting - person in bed"),
new Gene(0x1f46d, "family - two women holding hands"),
new Gene(0x1f46b, "family - man and woman holding hands"),
new Gene(0x1f46c, "family - two men holding hands"),
new Gene(0x1f48f, "family - kiss"),
                      // vBaseEmojis.Add(0x1f469 u+200d u+2764 u+fe0f u+200d u+1f48b u+200d u+1f468);	// family - kiss: woman, man
                      // vBaseEmojis.Add(0x1f468 u+200d u+2764 u+fe0f u+200d u+1f48b u+200d u+1f468);	// family - kiss: man, man
                      // vBaseEmojis.Add(0x1f469 u+200d u+2764 u+fe0f u+200d u+1f48b u+200d u+1f469);	// family - kiss: woman, woman
new Gene(0x1f491, "family - couple with heart"),
                      // vBaseEmojis.Add(0x1f469 u+200d u+2764 u+fe0f u+200d u+1f468);	// family - couple with heart: woman, man
                      // vBaseEmojis.Add(0x1f468 u+200d u+2764 u+fe0f u+200d u+1f468);	// family - couple with heart: man, man
                      // vBaseEmojis.Add(0x1f469 u+200d u+2764 u+fe0f u+200d u+1f469);	// family - couple with heart: woman, woman
new Gene(0x1f46a, "family - family"),
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f469 u+200d u+1f466);	// family - family: man, woman, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f469 u+200d u+1f467);	// family - family: man, woman, girl
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f469 u+200d u+1f467 u+200d u+1f466);	// family - family: man, woman, girl, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f469 u+200d u+1f466 u+200d u+1f466);	// family - family: man, woman, boy, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f469 u+200d u+1f467 u+200d u+1f467);	// family - family: man, woman, girl, girl
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f468 u+200d u+1f466);	// family - family: man, man, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f468 u+200d u+1f467);	// family - family: man, man, girl
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f468 u+200d u+1f467 u+200d u+1f466);	// family - family: man, man, girl, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f468 u+200d u+1f466 u+200d u+1f466);	// family - family: man, man, boy, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f468 u+200d u+1f467 u+200d u+1f467);	// family - family: man, man, girl, girl
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f469 u+200d u+1f466);	// family - family: woman, woman, boy
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f469 u+200d u+1f467);	// family - family: woman, woman, girl
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f469 u+200d u+1f467 u+200d u+1f466);	// family - family: woman, woman, girl, boy
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f469 u+200d u+1f466 u+200d u+1f466);	// family - family: woman, woman, boy, boy
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f469 u+200d u+1f467 u+200d u+1f467);	// family - family: woman, woman, girl, girl
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f466);	// family - family: man, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f466 u+200d u+1f466);	// family - family: man, boy, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f467);	// family - family: man, girl
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f467 u+200d u+1f466);	// family - family: man, girl, boy
                     // vBaseEmojis.Add(0x1f468 u+200d u+1f467 u+200d u+1f467);	// family - family: man, girl, girl
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f466);	// family - family: woman, boy
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f466 u+200d u+1f466);	// family - family: woman, boy, boy
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f467);	// family - family: woman, girl
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f467 u+200d u+1f466);	// family - family: woman, girl, boy
                     // vBaseEmojis.Add(0x1f469 u+200d u+1f467 u+200d u+1f467);	// family - family: woman, girl, girl
new Gene(0x1f5e3, "person-symbol - speaking head"),
new Gene(0x1f464, "person-symbol - bust in silhouette"),
new Gene(0x1f465, "person-symbol - busts in silhouette"),
new Gene(0x1f463, "person-symbol - footprints"),
new Gene(0x1f9b0, "hair-style - red hair"),
new Gene(0x1f9b1, "hair-style - curly hair"),
new Gene(0x1f9b3, "hair-style - white hair"),
new Gene(0x1f9b2, "hair-style - bald"),
new Gene(0x1f435, "animal-mammal - monkey face"),
new Gene(0x1f412, "animal-mammal - monkey"),
new Gene(0x1f98d, "animal-mammal - gorilla"),
new Gene(0x1f436, "animal-mammal - dog face"),
new Gene(0x1f415, "animal-mammal - dog"),
new Gene(0x1f429, "animal-mammal - poodle"),
new Gene(0x1f43a, "animal-mammal - wolf face"),
new Gene(0x1f98a, "animal-mammal - fox face"),
new Gene(0x1f99d, "animal-mammal - raccoon"),
new Gene(0x1f431, "animal-mammal - cat face"),
new Gene(0x1f408, "animal-mammal - cat"),
new Gene(0x1f981, "animal-mammal - lion face"),
new Gene(0x1f42f, "animal-mammal - tiger face"),
new Gene(0x1f405, "animal-mammal - tiger"),
new Gene(0x1f406, "animal-mammal - leopard"),
new Gene(0x1f434, "animal-mammal - horse face"),
new Gene(0x1f40e, "animal-mammal - horse"),
new Gene(0x1f984, "animal-mammal - unicorn face"),
new Gene(0x1f993, "animal-mammal - zebra"),
new Gene(0x1f98c, "animal-mammal - deer"),
new Gene(0x1f42e, "animal-mammal - cow face"),
new Gene(0x1f402, "animal-mammal - ox"),
new Gene(0x1f403, "animal-mammal - water buffalo"),
new Gene(0x1f404, "animal-mammal - cow"),
new Gene(0x1f437, "animal-mammal - pig face"),
new Gene(0x1f416, "animal-mammal - pig"),
new Gene(0x1f417, "animal-mammal - boar"),
new Gene(0x1f43d, "animal-mammal - pig nose"),
new Gene(0x1f40f, "animal-mammal - ram"),
new Gene(0x1f411, "animal-mammal - ewe"),
new Gene(0x1f410, "animal-mammal - goat"),
new Gene(0x1f42a, "animal-mammal - camel"),
new Gene(0x1f42b, "animal-mammal - two-hump camel"),
new Gene(0x1f999, "animal-mammal - llama"),
new Gene(0x1f992, "animal-mammal - giraffe"),
new Gene(0x1f418, "animal-mammal - elephant"),
new Gene(0x1f98f, "animal-mammal - rhinoceros"),
new Gene(0x1f99b, "animal-mammal - hippopotamus"),
new Gene(0x1f42d, "animal-mammal - mouse face"),
new Gene(0x1f401, "animal-mammal - mouse"),
new Gene(0x1f400, "animal-mammal - rat"),
new Gene(0x1f439, "animal-mammal - hamster face"),
new Gene(0x1f430, "animal-mammal - rabbit face"),
new Gene(0x1f407, "animal-mammal - rabbit"),
new Gene(0x1f43f, "animal-mammal - chipmunk"),
new Gene(0x1f994, "animal-mammal - hedgehog"),
new Gene(0x1f987, "animal-mammal - bat"),
new Gene(0x1f43b, "animal-mammal - bear face"),
new Gene(0x1f428, "animal-mammal - koala"),
new Gene(0x1f43c, "animal-mammal - panda face"),
new Gene(0x1f998, "animal-mammal - kangaroo"),
new Gene(0x1f9a1, "animal-mammal - badger"),
new Gene(0x1f43e, "animal-mammal - paw prints"),
new Gene(0x1f983, "animal-bird - turkey"),
new Gene(0x1f414, "animal-bird - chicken"),
new Gene(0x1f413, "animal-bird - rooster"),
new Gene(0x1f423, "animal-bird - hatching chick"),
new Gene(0x1f424, "animal-bird - baby chick"),
new Gene(0x1f425, "animal-bird - front-facing baby chick"),
new Gene(0x1f426, "animal-bird - bird"),
new Gene(0x1f427, "animal-bird - penguin"),
new Gene(0x1f54a, "animal-bird - dove"),
new Gene(0x1f985, "animal-bird - eagle"),
new Gene(0x1f986, "animal-bird - duck"),
new Gene(0x1f9a2, "animal-bird - swan"),
new Gene(0x1f989, "animal-bird - owl"),
new Gene(0x1f99a, "animal-bird - peacock"),
new Gene(0x1f99c, "animal-bird - parrot"),
new Gene(0x1f438, "animal-amphibian - frog face"),
new Gene(0x1f40a, "animal-reptile - crocodile"),
new Gene(0x1f422, "animal-reptile - turtle"),
new Gene(0x1f98e, "animal-reptile - lizard"),
new Gene(0x1f40d, "animal-reptile - snake"),
new Gene(0x1f432, "animal-reptile - dragon face"),
new Gene(0x1f409, "animal-reptile - dragon"),
new Gene(0x1f995, "animal-reptile - sauropod"),
new Gene(0x1f996, "animal-reptile - T-Rex"),
new Gene(0x1f433, "animal-marine - spouting whale"),
new Gene(0x1f40b, "animal-marine - whale"),
new Gene(0x1f42c, "animal-marine - dolphin"),
new Gene(0x1f41f, "animal-marine - fish"),
new Gene(0x1f420, "animal-marine - tropical fish"),
new Gene(0x1f421, "animal-marine - blowfish"),
new Gene(0x1f988, "animal-marine - shark"),
new Gene(0x1f419, "animal-marine - octopus"),
new Gene(0x1f41a, "animal-marine - spiral shell"),
new Gene(0x1f40c, "animal-bug - snail"),
new Gene(0x1f98b, "animal-bug - butterfly"),
new Gene(0x1f41b, "animal-bug - bug"),
new Gene(0x1f41c, "animal-bug - ant"),
new Gene(0x1f41d, "animal-bug - honeybee"),
new Gene(0x1f41e, "animal-bug - lady beetle"),
new Gene(0x1f997, "animal-bug - cricket"),
new Gene(0x1f577, "animal-bug - spider"),
new Gene(0x1f578, "animal-bug - spider web"),
new Gene(0x1f982, "animal-bug - scorpion"),
new Gene(0x1f99f, "animal-bug - mosquito"),
new Gene(0x1f9a0, "animal-bug - microbe"),
new Gene(0x1f490, "plant-flower - bouquet"),
new Gene(0x1f338, "plant-flower - cherry blossom"),
new Gene(0x1f4ae, "plant-flower - white flower"),
new Gene(0x1f3f5, "plant-flower - rosette"),
new Gene(0x1f339, "plant-flower - rose"),
new Gene(0x1f940, "plant-flower - wilted flower"),
new Gene(0x1f33a, "plant-flower - hibiscus"),
new Gene(0x1f33b, "plant-flower - sunflower"),
new Gene(0x1f33c, "plant-flower - blossom"),
new Gene(0x1f337, "plant-flower - tulip"),
new Gene(0x1f331, "plant-other - seedling"),
new Gene(0x1f332, "plant-other - evergreen tree"),
new Gene(0x1f333, "plant-other - deciduous tree"),
new Gene(0x1f334, "plant-other - palm tree"),
new Gene(0x1f335, "plant-other - cactus"),
new Gene(0x1f33e, "plant-other - sheaf of rice"),
new Gene(0x1f33f, "plant-other - herb"),
new Gene(0x2618, "plant-other - shamrock"),
new Gene(0x1f340, "plant-other - four leaf clover"),
new Gene(0x1f341, "plant-other - maple leaf"),
new Gene(0x1f342, "plant-other - fallen leaf"),
new Gene(0x1f343, "plant-other - leaf fluttering in wind"),
new Gene(0x1f347, "food-fruit - grapes"),
new Gene(0x1f348, "food-fruit - melon"),
new Gene(0x1f349, "food-fruit - watermelon"),
new Gene(0x1f34a, "food-fruit - tangerine"),
new Gene(0x1f34b, "food-fruit - lemon"),
new Gene(0x1f34c, "food-fruit - banana"),
new Gene(0x1f34d, "food-fruit - pineapple"),
new Gene(0x1f96d, "food-fruit - mango"),
new Gene(0x1f34e, "food-fruit - red apple"),
new Gene(0x1f34f, "food-fruit - green apple"),
new Gene(0x1f350, "food-fruit - pear"),
new Gene(0x1f351, "food-fruit - peach"),
new Gene(0x1f352, "food-fruit - cherries"),
new Gene(0x1f353, "food-fruit - strawberry"),
new Gene(0x1f95d, "food-fruit - kiwi fruit"),
new Gene(0x1f345, "food-fruit - tomato"),
new Gene(0x1f965, "food-fruit - coconut"),
new Gene(0x1f951, "food-vegetable - avocado"),
new Gene(0x1f346, "food-vegetable - eggplant"),
new Gene(0x1f954, "food-vegetable - potato"),
new Gene(0x1f955, "food-vegetable - carrot"),
new Gene(0x1f33d, "food-vegetable - ear of corn"),
new Gene(0x1f336, "food-vegetable - hot pepper"),
new Gene(0x1f952, "food-vegetable - cucumber"),
new Gene(0x1f96c, "food-vegetable - leafy green"),
new Gene(0x1f966, "food-vegetable - broccoli"),
new Gene(0x1f344, "food-vegetable - mushroom"),
new Gene(0x1f95c, "food-vegetable - peanuts"),
new Gene(0x1f330, "food-vegetable - chestnut"),
new Gene(0x1f35e, "food-prepared - bread"),
new Gene(0x1f950, "food-prepared - croissant"),
new Gene(0x1f956, "food-prepared - baguette bread"),
new Gene(0x1f968, "food-prepared - pretzel"),
new Gene(0x1f96f, "food-prepared - bagel"),
new Gene(0x1f95e, "food-prepared - pancakes"),
new Gene(0x1f9c0, "food-prepared - cheese wedge"),
new Gene(0x1f356, "food-prepared - meat on bone"),
new Gene(0x1f357, "food-prepared - poultry leg"),
new Gene(0x1f969, "food-prepared - cut of meat"),
new Gene(0x1f953, "food-prepared - bacon"),
new Gene(0x1f354, "food-prepared - hamburger"),
new Gene(0x1f35f, "food-prepared - french fries"),
new Gene(0x1f355, "food-prepared - pizza"),
new Gene(0x1f32d, "food-prepared - hot dog"),
new Gene(0x1f96a, "food-prepared - sandwich"),
new Gene(0x1f32e, "food-prepared - taco"),
new Gene(0x1f32f, "food-prepared - burrito"),
new Gene(0x1f959, "food-prepared - stuffed flatbread"),
new Gene(0x1f95a, "food-prepared - egg"),
new Gene(0x1f373, "food-prepared - cooking"),
new Gene(0x1f958, "food-prepared - shallow pan of food"),
new Gene(0x1f372, "food-prepared - pot of food"),
new Gene(0x1f963, "food-prepared - bowl with spoon"),
new Gene(0x1f957, "food-prepared - green salad"),
new Gene(0x1f37f, "food-prepared - popcorn"),
new Gene(0x1f9c2, "food-prepared - salt"),
new Gene(0x1f96b, "food-prepared - canned food"),
new Gene(0x1f371, "food-asian - bento box"),
new Gene(0x1f358, "food-asian - rice cracker"),
new Gene(0x1f359, "food-asian - rice ball"),
new Gene(0x1f35a, "food-asian - cooked rice"),
new Gene(0x1f35b, "food-asian - curry rice"),
new Gene(0x1f35c, "food-asian - steaming bowl"),
new Gene(0x1f35d, "food-asian - spaghetti"),
new Gene(0x1f360, "food-asian - roasted sweet potato"),
new Gene(0x1f362, "food-asian - oden"),
new Gene(0x1f363, "food-asian - sushi"),
new Gene(0x1f364, "food-asian - fried shrimp"),
new Gene(0x1f365, "food-asian - fish cake with swirl"),
new Gene(0x1f96e, "food-asian - moon cake"),
new Gene(0x1f361, "food-asian - dango"),
new Gene(0x1f95f, "food-asian - dumpling"),
new Gene(0x1f960, "food-asian - fortune cookie"),
new Gene(0x1f961, "food-asian - takeout box"),
new Gene(0x1f980, "food-marine - crab"),
new Gene(0x1f99e, "food-marine - lobster"),
new Gene(0x1f990, "food-marine - shrimp"),
new Gene(0x1f991, "food-marine - squid"),
new Gene(0x1f366, "food-sweet - soft ice cream"),
new Gene(0x1f367, "food-sweet - shaved ice"),
new Gene(0x1f368, "food-sweet - ice cream"),
new Gene(0x1f369, "food-sweet - doughnut"),
new Gene(0x1f36a, "food-sweet - cookie"),
new Gene(0x1f382, "food-sweet - birthday cake"),
new Gene(0x1f370, "food-sweet - shortcake"),
new Gene(0x1f9c1, "food-sweet - cupcake"),
new Gene(0x1f967, "food-sweet - pie"),
new Gene(0x1f36b, "food-sweet - chocolate bar"),
new Gene(0x1f36c, "food-sweet - candy"),
new Gene(0x1f36d, "food-sweet - lollipop"),
new Gene(0x1f36e, "food-sweet - custard"),
new Gene(0x1f36f, "food-sweet - honey pot"),
new Gene(0x1f37c, "drink - baby bottle"),
new Gene(0x1f95b, "drink - glass of milk"),
new Gene(0x2615, "drink - hot beverage"),
new Gene(0x1f375, "drink - teacup without handle"),
new Gene(0x1f376, "drink - sake"),
new Gene(0x1f37e, "drink - bottle with popping cork"),
new Gene(0x1f377, "drink - wine glass"),
new Gene(0x1f378, "drink - cocktail glass"),
new Gene(0x1f379, "drink - tropical drink"),
new Gene(0x1f37a, "drink - beer mug"),
new Gene(0x1f37b, "drink - clinking beer mugs"),
new Gene(0x1f942, "drink - clinking glasses"),
new Gene(0x1f943, "drink - tumbler glass"),
new Gene(0x1f964, "drink - cup with straw"),
new Gene(0x1f962, "dishware - chopsticks"),
new Gene(0x1f37d, "dishware - fork and knife with plate"),
new Gene(0x1f374, "dishware - fork and knife"),
new Gene(0x1f944, "dishware - spoon"),
new Gene(0x1f52a, "dishware - kitchen knife"),
new Gene(0x1f3fa, "dishware - amphora"),
new Gene(0x1f30d, "place-map - globe showing Europe-Africa"),
new Gene(0x1f30e, "place-map - globe showing Americas"),
new Gene(0x1f30f, "place-map - globe showing Asia-Australia"),
new Gene(0x1f310, "place-map - globe with meridians"),
new Gene(0x1f5fa, "place-map - world map"),
new Gene(0x1f5fe, "place-map - map of Japan"),
new Gene(0x1f9ed, "place-map - compass"),
new Gene(0x1f3d4, "place-geographic - snow-capped mountain"),
new Gene(0x26f0, "place-geographic - mountain"),
new Gene(0x1f30b, "place-geographic - volcano"),
new Gene(0x1f5fb, "place-geographic - mount fuji"),
new Gene(0x1f3d5, "place-geographic - camping"),
new Gene(0x1f3d6, "place-geographic - beach with umbrella"),
new Gene(0x1f3dc, "place-geographic - desert"),
new Gene(0x1f3dd, "place-geographic - desert island"),
new Gene(0x1f3de, "place-geographic - national park"),
new Gene(0x1f3df, "place-building - stadium"),
new Gene(0x1f3db, "place-building - classical building"),
new Gene(0x1f3d7, "place-building - building construction"),
new Gene(0x1f9f1, "place-building - brick"),
new Gene(0x1f3d8, "place-building - houses"),
new Gene(0x1f3da, "place-building - derelict house"),
new Gene(0x1f3e0, "place-building - house"),
new Gene(0x1f3e1, "place-building - house with garden"),
new Gene(0x1f3e2, "place-building - office building"),
new Gene(0x1f3e3, "place-building - Japanese post office"),
new Gene(0x1f3e4, "place-building - post office"),
new Gene(0x1f3e5, "place-building - hospital"),
new Gene(0x1f3e6, "place-building - bank"),
new Gene(0x1f3e8, "place-building - hotel"),
new Gene(0x1f3e9, "place-building - love hotel"),
new Gene(0x1f3ea, "place-building - convenience store"),
new Gene(0x1f3eb, "place-building - school"),
new Gene(0x1f3ec, "place-building - department store"),
new Gene(0x1f3ed, "place-building - factory"),
new Gene(0x1f3ef, "place-building - Japanese castle"),
new Gene(0x1f3f0, "place-building - castle"),
new Gene(0x1f492, "place-building - wedding"),
new Gene(0x1f5fc, "place-building - Tokyo tower"),
new Gene(0x1f5fd, "place-building - Statue of Liberty"),
new Gene(0x26ea, "place-religious - church"),
new Gene(0x1f54c, "place-religious - mosque"),
new Gene(0x1f54d, "place-religious - synagogue"),
new Gene(0x26e9, "place-religious - shinto shrine"),
new Gene(0x1f54b, "place-religious - kaaba"),
new Gene(0x26f2, "place-other - fountain"),
new Gene(0x26fa, "place-other - tent"),
new Gene(0x1f301, "place-other - foggy"),
new Gene(0x1f303, "place-other - night with stars"),
new Gene(0x1f3d9, "place-other - cityscape"),
new Gene(0x1f304, "place-other - sunrise over mountains"),
new Gene(0x1f305, "place-other - sunrise"),
new Gene(0x1f306, "place-other - cityscape at dusk"),
new Gene(0x1f307, "place-other - sunset"),
new Gene(0x1f309, "place-other - bridge at night"),
new Gene(0x2668, "place-other - hot springs"),
new Gene(0x1f30c, "place-other - milky way"),
new Gene(0x1f3a0, "place-other - carousel horse"),
new Gene(0x1f3a1, "place-other - ferris wheel"),
new Gene(0x1f3a2, "place-other - roller coaster"),
new Gene(0x1f488, "place-other - barber pole"),
new Gene(0x1f3aa, "place-other - circus tent"),
new Gene(0x1f682, "transport-ground - locomotive"),
new Gene(0x1f683, "transport-ground - railway car"),
new Gene(0x1f684, "transport-ground - high-speed train"),
new Gene(0x1f685, "transport-ground - bullet train"),
new Gene(0x1f686, "transport-ground - train"),
new Gene(0x1f687, "transport-ground - metro"),
new Gene(0x1f688, "transport-ground - light rail"),
new Gene(0x1f689, "transport-ground - station"),
new Gene(0x1f68a, "transport-ground - tram"),
new Gene(0x1f69d, "transport-ground - monorail"),
new Gene(0x1f69e, "transport-ground - mountain railway"),
new Gene(0x1f68b, "transport-ground - tram car"),
new Gene(0x1f68c, "transport-ground - bus"),
new Gene(0x1f68d, "transport-ground - oncoming bus"),
new Gene(0x1f68e, "transport-ground - trolleybus"),
new Gene(0x1f690, "transport-ground - minibus"),
new Gene(0x1f691, "transport-ground - ambulance"),
new Gene(0x1f692, "transport-ground - fire engine"),
new Gene(0x1f693, "transport-ground - police car"),
new Gene(0x1f694, "transport-ground - oncoming police car"),
new Gene(0x1f695, "transport-ground - taxi"),
new Gene(0x1f696, "transport-ground - oncoming taxi"),
new Gene(0x1f697, "transport-ground - automobile"),
new Gene(0x1f698, "transport-ground - oncoming automobile"),
new Gene(0x1f699, "transport-ground - sport utility vehicle"),
new Gene(0x1f69a, "transport-ground - delivery truck"),
new Gene(0x1f69b, "transport-ground - articulated lorry"),
new Gene(0x1f69c, "transport-ground - tractor"),
new Gene(0x1f3ce, "transport-ground - racing car"),
new Gene(0x1f3cd, "transport-ground - motorcycle"),
new Gene(0x1f6f5, "transport-ground - motor scooter"),
new Gene(0x1f6b2, "transport-ground - bicycle"),
new Gene(0x1f6f4, "transport-ground - kick scooter"),
new Gene(0x1f6f9, "transport-ground - skateboard"),
new Gene(0x1f68f, "transport-ground - bus stop"),
new Gene(0x1f6e3, "transport-ground - motorway"),
new Gene(0x1f6e4, "transport-ground - railway track"),
new Gene(0x1f6e2, "transport-ground - oil drum"),
new Gene(0x26fd, "transport-ground - fuel pump"),
new Gene(0x1f6a8, "transport-ground - police car light"),
new Gene(0x1f6a5, "transport-ground - horizontal traffic light"),
new Gene(0x1f6a6, "transport-ground - vertical traffic light"),
new Gene(0x1f6d1, "transport-ground - stop sign"),
new Gene(0x1f6a7, "transport-ground - construction"),
new Gene(0x2693, "transport-water - anchor"),
new Gene(0x26f5, "transport-water - sailboat"),
new Gene(0x1f6f6, "transport-water - canoe"),
new Gene(0x1f6a4, "transport-water - speedboat"),
new Gene(0x1f6f3, "transport-water - passenger ship"),
new Gene(0x26f4, "transport-water - ferry"),
new Gene(0x1f6e5, "transport-water - motor boat"),
new Gene(0x1f6a2, "transport-water - ship"),
new Gene(0x2708, "transport-air - airplane"),
new Gene(0x1f6e9, "transport-air - small airplane"),
new Gene(0x1f6eb, "transport-air - airplane departure"),
new Gene(0x1f6ec, "transport-air - airplane arrival"),
new Gene(0x1f4ba, "transport-air - seat"),
new Gene(0x1f681, "transport-air - helicopter"),
new Gene(0x1f69f, "transport-air - suspension railway"),
new Gene(0x1f6a0, "transport-air - mountain cableway"),
new Gene(0x1f6a1, "transport-air - aerial tramway"),
new Gene(0x1f6f0, "transport-air - satellite"),
new Gene(0x1f680, "transport-air - rocket"),
new Gene(0x1f6f8, "transport-air - flying saucer"),
new Gene(0x1f6ce, "hotel - bellhop bell"),
new Gene(0x1f9f3, "hotel - luggage"),
new Gene(0x231b, "time - hourglass done"),
new Gene(0x23f3, "time - hourglass not done"),
new Gene(0x231a, "time - watch"),
new Gene(0x23f0, "time - alarm clock"),
new Gene(0x23f1, "time - stopwatch"),
new Gene(0x23f2, "time - timer clock"),
new Gene(0x1f570, "time - mantelpiece clock"),
new Gene(0x1f55b, "time - twelve o‚Äôclock"),
new Gene(0x1f567, "time - twelve-thirty"),
new Gene(0x1f550, "time - one o‚Äôclock"),
new Gene(0x1f55c, "time - one-thirty"),
new Gene(0x1f551, "time - two o‚Äôclock"),
new Gene(0x1f55d, "time - two-thirty"),
new Gene(0x1f552, "time - three o‚Äôclock"),
new Gene(0x1f55e, "time - three-thirty"),
new Gene(0x1f553, "time - four o‚Äôclock"),
new Gene(0x1f55f, "time - four-thirty"),
new Gene(0x1f554, "time - five o‚Äôclock"),
new Gene(0x1f560, "time - five-thirty"),
new Gene(0x1f555, "time - six o‚Äôclock"),
new Gene(0x1f561, "time - six-thirty"),
new Gene(0x1f556, "time - seven o‚Äôclock"),
new Gene(0x1f562, "time - seven-thirty"),
new Gene(0x1f557, "time - eight o‚Äôclock"),
new Gene(0x1f563, "time - eight-thirty"),
new Gene(0x1f558, "time - nine o‚Äôclock"),
new Gene(0x1f564, "time - nine-thirty"),
new Gene(0x1f559, "time - ten o‚Äôclock"),
new Gene(0x1f565, "time - ten-thirty"),
new Gene(0x1f55a, "time - eleven o‚Äôclock"),
new Gene(0x1f566, "time - eleven-thirty"),
new Gene(0x1f311, "sky & weather - new moon"),
new Gene(0x1f312, "sky & weather - waxing crescent moon"),
new Gene(0x1f313, "sky & weather - first quarter moon"),
new Gene(0x1f314, "sky & weather - waxing gibbous moon"),
new Gene(0x1f315, "sky & weather - full moon"),
new Gene(0x1f316, "sky & weather - waning gibbous moon"),
new Gene(0x1f317, "sky & weather - last quarter moon"),
new Gene(0x1f318, "sky & weather - waning crescent moon"),
new Gene(0x1f319, "sky & weather - crescent moon"),
new Gene(0x1f31a, "sky & weather - new moon face"),
new Gene(0x1f31b, "sky & weather - first quarter moon face"),
new Gene(0x1f31c, "sky & weather - last quarter moon face"),
new Gene(0x1f321, "sky & weather - thermometer"),
new Gene(0x2600, "sky & weather - sun"),
new Gene(0x1f31d, "sky & weather - full moon face"),
new Gene(0x1f31e, "sky & weather - sun with face"),
new Gene(0x2b50, "sky & weather - star"),
new Gene(0x1f31f, "sky & weather - glowing star"),
new Gene(0x1f320, "sky & weather - shooting star"),
new Gene(0x2601, "sky & weather - cloud"),
new Gene(0x26c5, "sky & weather - sun behind cloud"),
new Gene(0x26c8, "sky & weather - cloud with lightning and rain"),
new Gene(0x1f324, "sky & weather - sun behind small cloud"),
new Gene(0x1f325, "sky & weather - sun behind large cloud"),
new Gene(0x1f326, "sky & weather - sun behind rain cloud"),
new Gene(0x1f327, "sky & weather - cloud with rain"),
new Gene(0x1f328, "sky & weather - cloud with snow"),
new Gene(0x1f329, "sky & weather - cloud with lightning"),
new Gene(0x1f32a, "sky & weather - tornado"),
new Gene(0x1f32b, "sky & weather - fog"),
new Gene(0x1f32c, "sky & weather - wind face"),
new Gene(0x1f300, "sky & weather - cyclone"),
new Gene(0x1f308, "sky & weather - rainbow"),
new Gene(0x1f302, "sky & weather - closed umbrella"),
new Gene(0x2602, "sky & weather - umbrella"),
new Gene(0x2614, "sky & weather - umbrella with rain drops"),
new Gene(0x26f1, "sky & weather - umbrella on ground"),
new Gene(0x26a1, "sky & weather - high voltage"),
new Gene(0x2744, "sky & weather - snowflake"),
new Gene(0x2603, "sky & weather - snowman"),
new Gene(0x26c4, "sky & weather - snowman without snow"),
new Gene(0x2604, "sky & weather - comet"),
new Gene(0x1f525, "sky & weather - fire"),
new Gene(0x1f4a7, "sky & weather - droplet"),
new Gene(0x1f30a, "sky & weather - water wave"),
new Gene(0x1f383, "event - jack-o-lantern"),
new Gene(0x1f384, "event - Christmas tree"),
new Gene(0x1f386, "event - fireworks"),
new Gene(0x1f387, "event - sparkler"),
new Gene(0x1f9e8, "event - firecracker"),
new Gene(0x2728, "event - sparkles"),
new Gene(0x1f388, "event - balloon"),
new Gene(0x1f389, "event - party popper"),
new Gene(0x1f38a, "event - confetti ball"),
new Gene(0x1f38b, "event - tanabata tree"),
new Gene(0x1f38d, "event - pine decoration"),
new Gene(0x1f38e, "event - Japanese dolls"),
new Gene(0x1f38f, "event - carp streamer"),
new Gene(0x1f390, "event - wind chime"),
new Gene(0x1f391, "event - moon viewing ceremony"),
new Gene(0x1f9e7, "event - red envelope"),
new Gene(0x1f380, "event - ribbon"),
new Gene(0x1f381, "event - wrapped gift"),
new Gene(0x1f397, "event - reminder ribbon"),
new Gene(0x1f39f, "event - admission tickets"),
new Gene(0x1f3ab, "event - ticket"),
new Gene(0x1f396, "award-medal - military medal"),
new Gene(0x1f3c6, "award-medal - trophy"),
new Gene(0x1f3c5, "award-medal - sports medal"),
new Gene(0x1f947, "award-medal - 1st place medal"),
new Gene(0x1f948, "award-medal - 2nd place medal"),
new Gene(0x1f949, "award-medal - 3rd place medal"),
new Gene(0x26bd, "sport - soccer ball"),
new Gene(0x26be, "sport - baseball"),
new Gene(0x1f94e, "sport - softball"),
new Gene(0x1f3c0, "sport - basketball"),
new Gene(0x1f3d0, "sport - volleyball"),
new Gene(0x1f3c8, "sport - american football"),
new Gene(0x1f3c9, "sport - rugby football"),
new Gene(0x1f3be, "sport - tennis"),
new Gene(0x1f94f, "sport - flying disc"),
new Gene(0x1f3b3, "sport - bowling"),
new Gene(0x1f3cf, "sport - cricket game"),
new Gene(0x1f3d1, "sport - field hockey"),
new Gene(0x1f3d2, "sport - ice hockey"),
new Gene(0x1f94d, "sport - lacrosse"),
new Gene(0x1f3d3, "sport - ping pong"),
new Gene(0x1f3f8, "sport - badminton"),
new Gene(0x1f94a, "sport - boxing glove"),
new Gene(0x1f94b, "sport - martial arts uniform"),
new Gene(0x1f945, "sport - goal net"),
new Gene(0x26f3, "sport - flag in hole"),
new Gene(0x26f8, "sport - ice skate"),
new Gene(0x1f3a3, "sport - fishing pole"),
new Gene(0x1f3bd, "sport - running shirt"),
new Gene(0x1f3bf, "sport - skis"),
new Gene(0x1f6f7, "sport - sled"),
new Gene(0x1f94c, "sport - curling stone"),
new Gene(0x1f3af, "game - direct hit"),
new Gene(0x1f3b1, "game - pool 8 ball"),
new Gene(0x1f52e, "game - crystal ball"),
new Gene(0x1f9ff, "game - nazar amulet"),
new Gene(0x1f3ae, "game - video game"),
new Gene(0x1f579, "game - joystick"),
new Gene(0x1f3b0, "game - slot machine"),
new Gene(0x1f3b2, "game - game die"),
new Gene(0x1f9e9, "game - jigsaw"),
new Gene(0x1f9f8, "game - teddy bear"),
new Gene(0x2660, "game - spade suit"),
new Gene(0x2665, "game - heart suit"),
new Gene(0x2666, "game - diamond suit"),
new Gene(0x2663, "game - club suit"),
new Gene(0x265f, "game - chess pawn"),
new Gene(0x1f0cf, "game - joker"),
new Gene(0x1f004, "game - mahjong red dragon"),
new Gene(0x1f3b4, "game - flower playing cards"),
new Gene(0x1f3ad, "arts & crafts - performing arts"),
new Gene(0x1f5bc, "arts & crafts - framed picture"),
new Gene(0x1f3a8, "arts & crafts - artist palette"),
new Gene(0x1f9f5, "arts & crafts - thread"),
new Gene(0x1f9f6, "arts & crafts - yarn"),
new Gene(0x1f453, "clothing - glasses"),
new Gene(0x1f576, "clothing - sunglasses"),
new Gene(0x1f97d, "clothing - goggles"),
new Gene(0x1f97c, "clothing - lab coat"),
new Gene(0x1f454, "clothing - necktie"),
new Gene(0x1f455, "clothing - t-shirt"),
new Gene(0x1f456, "clothing - jeans"),
new Gene(0x1f9e3, "clothing - scarf"),
new Gene(0x1f9e4, "clothing - gloves"),
new Gene(0x1f9e5, "clothing - coat"),
new Gene(0x1f9e6, "clothing - socks"),
new Gene(0x1f457, "clothing - dress"),
new Gene(0x1f458, "clothing - kimono"),
new Gene(0x1f459, "clothing - bikini"),
new Gene(0x1f45a, "clothing - woman‚Äôs clothes"),
new Gene(0x1f45b, "clothing - purse"),
new Gene(0x1f45c, "clothing - handbag"),
new Gene(0x1f45d, "clothing - clutch bag"),
new Gene(0x1f6cd, "clothing - shopping bags"),
new Gene(0x1f392, "clothing - backpack"),
new Gene(0x1f45e, "clothing - man‚Äôs shoe"),
new Gene(0x1f45f, "clothing - running shoe"),
new Gene(0x1f97e, "clothing - hiking boot"),
new Gene(0x1f97f, "clothing - flat shoe"),
new Gene(0x1f460, "clothing - high-heeled shoe"),
new Gene(0x1f461, "clothing - woman‚Äôs sandal"),
new Gene(0x1f462, "clothing - woman‚Äôs boot"),
new Gene(0x1f451, "clothing - crown"),
new Gene(0x1f452, "clothing - woman‚Äôs hat"),
new Gene(0x1f3a9, "clothing - top hat"),
new Gene(0x1f393, "clothing - graduation cap"),
new Gene(0x1f9e2, "clothing - billed cap"),
new Gene(0x26d1, "clothing - rescue worker‚Äôs helmet"),
new Gene(0x1f4ff, "clothing - prayer beads"),
new Gene(0x1f484, "clothing - lipstick"),
new Gene(0x1f48d, "clothing - ring"),
new Gene(0x1f48e, "clothing - gem stone"),
new Gene(0x1f507, "sound - muted speaker"),
new Gene(0x1f508, "sound - speaker low volume"),
new Gene(0x1f509, "sound - speaker medium volume"),
new Gene(0x1f50a, "sound - speaker high volume"),
new Gene(0x1f4e2, "sound - loudspeaker"),
new Gene(0x1f4e3, "sound - megaphone"),
new Gene(0x1f4ef, "sound - postal horn"),
new Gene(0x1f514, "sound - bell"),
new Gene(0x1f515, "sound - bell with slash"),
new Gene(0x1f3bc, "music - musical score"),
new Gene(0x1f3b5, "music - musical note"),
new Gene(0x1f3b6, "music - musical notes"),
new Gene(0x1f399, "music - studio microphone"),
new Gene(0x1f39a, "music - level slider"),
new Gene(0x1f39b, "music - control knobs"),
new Gene(0x1f3a4, "music - microphone"),
new Gene(0x1f3a7, "music - headphone"),
new Gene(0x1f4fb, "music - radio"),
new Gene(0x1f3b7, "musical-instrument - saxophone"),
new Gene(0x1f3b8, "musical-instrument - guitar"),
new Gene(0x1f3b9, "musical-instrument - musical keyboard"),
new Gene(0x1f3ba, "musical-instrument - trumpet"),
new Gene(0x1f3bb, "musical-instrument - violin"),
new Gene(0x1f941, "musical-instrument - drum"),
new Gene(0x1f4f1, "phone - mobile phone"),
new Gene(0x1f4f2, "phone - mobile phone with arrow"),
new Gene(0x260e, "phone - telephone"),
new Gene(0x1f4de, "phone - telephone receiver"),
new Gene(0x1f4df, "phone - pager"),
new Gene(0x1f4e0, "phone - fax machine"),
new Gene(0x1f50b, "computer - battery"),
new Gene(0x1f50c, "computer - electric plug"),
new Gene(0x1f4bb, "computer - laptop computer"),
new Gene(0x1f5a5, "computer - desktop computer"),
new Gene(0x1f5a8, "computer - printer"),
new Gene(0x2328, "computer - keyboard"),
new Gene(0x1f5b1, "computer - computer mouse"),
new Gene(0x1f5b2, "computer - trackball"),
new Gene(0x1f4bd, "computer - computer disk"),
new Gene(0x1f4be, "computer - floppy disk"),
new Gene(0x1f4bf, "computer - optical disk"),
new Gene(0x1f4c0, "computer - dvd"),
new Gene(0x1f9ee, "computer - abacus"),
new Gene(0x1f3a5, "light & video - movie camera"),
new Gene(0x1f39e, "light & video - film frames"),
new Gene(0x1f4fd, "light & video - film projector"),
new Gene(0x1f3ac, "light & video - clapper board"),
new Gene(0x1f4fa, "light & video - television"),
new Gene(0x1f4f7, "light & video - camera"),
new Gene(0x1f4f8, "light & video - camera with flash"),
new Gene(0x1f4f9, "light & video - video camera"),
new Gene(0x1f4fc, "light & video - videocassette"),
new Gene(0x1f50d, "light & video - magnifying glass tilted left"),
new Gene(0x1f50e, "light & video - magnifying glass tilted right"),
new Gene(0x1f56f, "light & video - candle"),
new Gene(0x1f4a1, "light & video - light bulb"),
new Gene(0x1f526, "light & video - flashlight"),
new Gene(0x1f3ee, "light & video - red paper lantern"),
new Gene(0x1f4d4, "book-paper - notebook with decorative cover"),
new Gene(0x1f4d5, "book-paper - closed book"),
new Gene(0x1f4d6, "book-paper - open book"),
new Gene(0x1f4d7, "book-paper - green book"),
new Gene(0x1f4d8, "book-paper - blue book"),
new Gene(0x1f4d9, "book-paper - orange book"),
new Gene(0x1f4da, "book-paper - books"),
new Gene(0x1f4d3, "book-paper - notebook"),
new Gene(0x1f4d2, "book-paper - ledger"),
new Gene(0x1f4c3, "book-paper - page with curl"),
new Gene(0x1f4dc, "book-paper - scroll"),
new Gene(0x1f4c4, "book-paper - page facing up"),
new Gene(0x1f4f0, "book-paper - newspaper"),
new Gene(0x1f5de, "book-paper - rolled-up newspaper"),
new Gene(0x1f4d1, "book-paper - bookmark tabs"),
new Gene(0x1f516, "book-paper - bookmark"),
new Gene(0x1f3f7, "book-paper - label"),
new Gene(0x1f4b0, "money - money bag"),
new Gene(0x1f4b4, "money - yen banknote"),
new Gene(0x1f4b5, "money - dollar banknote"),
new Gene(0x1f4b6, "money - euro banknote"),
new Gene(0x1f4b7, "money - pound banknote"),
new Gene(0x1f4b8, "money - money with wings"),
new Gene(0x1f4b3, "money - credit card"),
new Gene(0x1f9fe, "money - receipt"),
new Gene(0x1f4b9, "money - chart increasing with yen"),
new Gene(0x1f4b1, "money - currency exchange"),
new Gene(0x1f4b2, "money - heavy dollar sign"),
new Gene(0x2709, "mail - envelope"),
new Gene(0x1f4e7, "mail - e-mail"),
new Gene(0x1f4e8, "mail - incoming envelope"),
new Gene(0x1f4e9, "mail - envelope with arrow"),
new Gene(0x1f4e4, "mail - outbox tray"),
new Gene(0x1f4e5, "mail - inbox tray"),
new Gene(0x1f4e6, "mail - package"),
new Gene(0x1f4eb, "mail - closed mailbox with raised flag"),
new Gene(0x1f4ea, "mail - closed mailbox with lowered flag"),
new Gene(0x1f4ec, "mail - open mailbox with raised flag"),
new Gene(0x1f4ed, "mail - open mailbox with lowered flag"),
new Gene(0x1f4ee, "mail - postbox"),
new Gene(0x1f5f3, "mail - ballot box with ballot"),
new Gene(0x270f, "writing - pencil"),
new Gene(0x2712, "writing - black nib"),
new Gene(0x1f58b, "writing - fountain pen"),
new Gene(0x1f58a, "writing - pen"),
new Gene(0x1f58c, "writing - paintbrush"),
new Gene(0x1f58d, "writing - crayon"),
new Gene(0x1f4dd, "writing - memo"),
new Gene(0x1f4bc, "office - briefcase"),
new Gene(0x1f4c1, "office - file folder"),
new Gene(0x1f4c2, "office - open file folder"),
new Gene(0x1f5c2, "office - card index dividers"),
new Gene(0x1f4c5, "office - calendar"),
new Gene(0x1f4c6, "office - tear-off calendar"),
new Gene(0x1f5d2, "office - spiral notepad"),
new Gene(0x1f5d3, "office - spiral calendar"),
new Gene(0x1f4c7, "office - card index"),
new Gene(0x1f4c8, "office - chart increasing"),
new Gene(0x1f4c9, "office - chart decreasing"),
new Gene(0x1f4ca, "office - bar chart"),
new Gene(0x1f4cb, "office - clipboard"),
new Gene(0x1f4cc, "office - pushpin"),
new Gene(0x1f4cd, "office - round pushpin"),
new Gene(0x1f4ce, "office - paperclip"),
new Gene(0x1f587, "office - linked paperclips"),
new Gene(0x1f4cf, "office - straight ruler"),
new Gene(0x1f4d0, "office - triangular ruler"),
new Gene(0x2702, "office - scissors"),
new Gene(0x1f5c3, "office - card file box"),
new Gene(0x1f5c4, "office - file cabinet"),
new Gene(0x1f5d1, "office - wastebasket"),
new Gene(0x1f512, "lock - locked"),
new Gene(0x1f513, "lock - unlocked"),
new Gene(0x1f50f, "lock - locked with pen"),
new Gene(0x1f510, "lock - locked with key"),
new Gene(0x1f511, "lock - key"),
new Gene(0x1f5dd, "lock - old key"),
new Gene(0x1f528, "tool - hammer"),
new Gene(0x26cf, "tool - pick"),
new Gene(0x2692, "tool - hammer and pick"),
new Gene(0x1f6e0, "tool - hammer and wrench"),
new Gene(0x1f5e1, "tool - dagger"),
new Gene(0x2694, "tool - crossed swords"),
new Gene(0x1f52b, "tool - pistol"),
new Gene(0x1f3f9, "tool - bow and arrow"),
new Gene(0x1f6e1, "tool - shield"),
new Gene(0x1f527, "tool - wrench"),
new Gene(0x1f529, "tool - nut and bolt"),
new Gene(0x2699, "tool - gear"),
new Gene(0x1f5dc, "tool - clamp"),
new Gene(0x2696, "tool - balance scale"),
new Gene(0x1f517, "tool - link"),
new Gene(0x26d3, "tool - chains"),
new Gene(0x1f9f0, "tool - toolbox"),
new Gene(0x1f9f2, "tool - magnet"),
new Gene(0x2697, "science - alembic"),
new Gene(0x1f9ea, "science - test tube"),
new Gene(0x1f9eb, "science - petri dish"),
new Gene(0x1f9ec, "science - dna"),
new Gene(0x1f52c, "science - microscope"),
new Gene(0x1f52d, "science - telescope"),
new Gene(0x1f4e1, "science - satellite antenna"),
new Gene(0x1f489, "medical - syringe"),
new Gene(0x1f48a, "medical - pill"),
new Gene(0x1f6aa, "household - door"),
new Gene(0x1f6cf, "household - bed"),
new Gene(0x1f6cb, "household - couch and lamp"),
new Gene(0x1f6bd, "household - toilet"),
new Gene(0x1f6bf, "household - shower"),
new Gene(0x1f6c1, "household - bathtub"),
new Gene(0x1f9f4, "household - lotion bottle"),
new Gene(0x1f9f7, "household - safety pin"),
new Gene(0x1f9f9, "household - broom"),
new Gene(0x1f9fa, "household - basket"),
new Gene(0x1f9fb, "household - roll of paper"),
new Gene(0x1f9fc, "household - soap"),
new Gene(0x1f9fd, "household - sponge"),
new Gene(0x1f9ef, "household - fire extinguisher"),
new Gene(0x1f6d2, "household - shopping cart"),
new Gene(0x1f6ac, "other-object - cigarette"),
new Gene(0x26b0, "other-object - coffin"),
new Gene(0x26b1, "other-object - funeral urn"),
new Gene(0x1f5ff, "other-object - moai"),
new Gene(0x1f3e7, "transport-sign - ATM sign"),
new Gene(0x1f6ae, "transport-sign - litter in bin sign"),
new Gene(0x1f6b0, "transport-sign - potable water"),
new Gene(0x267f, "transport-sign - wheelchair symbol"),
new Gene(0x1f6b9, "transport-sign - men‚Äôs room"),
new Gene(0x1f6ba, "transport-sign - women‚Äôs room"),
new Gene(0x1f6bb, "transport-sign - restroom"),
new Gene(0x1f6bc, "transport-sign - baby symbol"),
new Gene(0x1f6be, "transport-sign - water closet"),
new Gene(0x1f6c2, "transport-sign - passport control"),
new Gene(0x1f6c3, "transport-sign - customs"),
new Gene(0x1f6c4, "transport-sign - baggage claim"),
new Gene(0x1f6c5, "transport-sign - left luggage"),
new Gene(0x26a0, "warning - warning"),
new Gene(0x1f6b8, "warning - children crossing"),
new Gene(0x26d4, "warning - no entry"),
new Gene(0x1f6ab, "warning - prohibited"),
new Gene(0x1f6b3, "warning - no bicycles"),
new Gene(0x1f6ad, "warning - no smoking"),
new Gene(0x1f6af, "warning - no littering"),
new Gene(0x1f6b1, "warning - non-potable water"),
new Gene(0x1f6b7, "warning - no pedestrians"),
new Gene(0x1f4f5, "warning - no mobile phones"),
new Gene(0x1f51e, "warning - no one under eighteen"),
new Gene(0x2622, "warning - radioactive"),
new Gene(0x2623, "warning - biohazard"),
new Gene(0x2b06, "arrow - up arrow"),
new Gene(0x2197, "arrow - up-right arrow"),
new Gene(0x27a1, "arrow - right arrow"),
new Gene(0x2198, "arrow - down-right arrow"),
new Gene(0x2b07, "arrow - down arrow"),
new Gene(0x2199, "arrow - down-left arrow"),
new Gene(0x2b05, "arrow - left arrow"),
new Gene(0x2196, "arrow - up-left arrow"),
new Gene(0x2195, "arrow - up-down arrow"),
new Gene(0x2194, "arrow - left-right arrow"),
new Gene(0x21a9, "arrow - right arrow curving left"),
new Gene(0x21aa, "arrow - left arrow curving right"),
new Gene(0x2934, "arrow - right arrow curving up"),
new Gene(0x2935, "arrow - right arrow curving down"),
new Gene(0x1f503, "arrow - clockwise vertical arrows"),
new Gene(0x1f504, "arrow - counterclockwise arrows button"),
new Gene(0x1f519, "arrow - BACK arrow"),
new Gene(0x1f51a, "arrow - END arrow"),
new Gene(0x1f51b, "arrow - ON! arrow"),
new Gene(0x1f51c, "arrow - SOON arrow"),
new Gene(0x1f51d, "arrow - TOP arrow"),
new Gene(0x1f6d0, "religion - place of worship"),
new Gene(0x269b, "religion - atom symbol"),
new Gene(0x1f549, "religion - om"),
new Gene(0x2721, "religion - star of David"),
new Gene(0x2638, "religion - wheel of dharma"),
new Gene(0x262f, "religion - yin yang"),
new Gene(0x271d, "religion - latin cross"),
new Gene(0x2626, "religion - orthodox cross"),
new Gene(0x262a, "religion - star and crescent"),
new Gene(0x262e, "religion - peace symbol"),
new Gene(0x1f54e, "religion - menorah"),
new Gene(0x1f52f, "religion - dotted six-pointed star"),
new Gene(0x2648, "zodiac - Aries"),
new Gene(0x2649, "zodiac - Taurus"),
new Gene(0x264a, "zodiac - Gemini"),
new Gene(0x264b, "zodiac - Cancer"),
new Gene(0x264c, "zodiac - Leo"),
new Gene(0x264d, "zodiac - Virgo"),
new Gene(0x264e, "zodiac - Libra"),
new Gene(0x264f, "zodiac - Scorpio"),
new Gene(0x2650, "zodiac - Sagittarius"),
new Gene(0x2651, "zodiac - Capricorn"),
new Gene(0x2652, "zodiac - Aquarius"),
new Gene(0x2653, "zodiac - Pisces"),
new Gene(0x26ce, "zodiac - Ophiuchus"),
new Gene(0x1f500, "av-symbol - shuffle tracks button"),
new Gene(0x1f501, "av-symbol - repeat button"),
new Gene(0x1f502, "av-symbol - repeat single button"),
new Gene(0x25b6, "av-symbol - play button"),
new Gene(0x23e9, "av-symbol - fast-forward button"),
new Gene(0x23ed, "av-symbol - next track button"),
new Gene(0x23ef, "av-symbol - play or pause button"),
new Gene(0x25c0, "av-symbol - reverse button"),
new Gene(0x23ea, "av-symbol - fast reverse button"),
new Gene(0x23ee, "av-symbol - last track button"),
new Gene(0x1f53c, "av-symbol - upwards button"),
new Gene(0x23eb, "av-symbol - fast up button"),
new Gene(0x1f53d, "av-symbol - downwards button"),
new Gene(0x23ec, "av-symbol - fast down button"),
new Gene(0x23f8, "av-symbol - pause button"),
new Gene(0x23f9, "av-symbol - stop button"),
new Gene(0x23fa, "av-symbol - record button"),
new Gene(0x23cf, "av-symbol - eject button"),
new Gene(0x1f3a6, "av-symbol - cinema"),
new Gene(0x1f505, "av-symbol - dim button"),
new Gene(0x1f506, "av-symbol - bright button"),
new Gene(0x1f4f6, "av-symbol - antenna bars"),
new Gene(0x1f4f3, "av-symbol - vibration mode"),
new Gene(0x1f4f4, "av-symbol - mobile phone off"),
new Gene(0x2640, "gender - female sign"),
new Gene(0x2642, "gender - male sign"),
new Gene(0x2695, "other-symbol - medical symbol"),
new Gene(0x267e, "other-symbol - infinity"),
new Gene(0x267b, "other-symbol - recycling symbol"),
new Gene(0x269c, "other-symbol - fleur-de-lis"),
new Gene(0x1f531, "other-symbol - trident emblem"),
new Gene(0x1f4db, "other-symbol - name badge"),
new Gene(0x1f530, "other-symbol - Japanese symbol for beginner"),
new Gene(0x2b55, "other-symbol - heavy large circle"),
new Gene(0x2705, "other-symbol - white heavy check mark"),
new Gene(0x2611, "other-symbol - ballot box with check"),
new Gene(0x2714, "other-symbol - heavy check mark"),
new Gene(0x2716, "other-symbol - heavy multiplication x"),
new Gene(0x274c, "other-symbol - cross mark"),
new Gene(0x274e, "other-symbol - cross mark button"),
new Gene(0x2795, "other-symbol - heavy plus sign"),
new Gene(0x2796, "other-symbol - heavy minus sign"),
new Gene(0x2797, "other-symbol - heavy division sign"),
new Gene(0x27b0, "other-symbol - curly loop"),
new Gene(0x27bf, "other-symbol - double curly loop"),
new Gene(0x303d, "other-symbol - part alternation mark"),
new Gene(0x2733, "other-symbol - eight-spoked asterisk"),
new Gene(0x2734, "other-symbol - eight-pointed star"),
new Gene(0x2747, "other-symbol - sparkle"),
new Gene(0x203c, "other-symbol - double exclamation mark"),
new Gene(0x2049, "other-symbol - exclamation question mark"),
new Gene(0x2753, "other-symbol - question mark"),
new Gene(0x2754, "other-symbol - white question mark"),
new Gene(0x2755, "other-symbol - white exclamation mark"),
new Gene(0x2757, "other-symbol - exclamation mark"),
new Gene(0x3030, "other-symbol - wavy dash"),
new Gene(0x00a9, "other-symbol - copyright"),
new Gene(0x00ae, "other-symbol - registered"),
new Gene(0x2122, "other-symbol - trade mark"),
                        // vBaseEmojis.Add(0x0023 u+fe0f u+20e3);	// keycap - keycap: #
                        // vBaseEmojis.Add(0x002a u+fe0f u+20e3);	// keycap - keycap: *
                        // vBaseEmojis.Add(0x0030 u+fe0f u+20e3);	// keycap - keycap: 0
                        // vBaseEmojis.Add(0x0031 u+fe0f u+20e3);	// keycap - keycap: 1
                        // vBaseEmojis.Add(0x0032 u+fe0f u+20e3);	// keycap - keycap: 2
                        // vBaseEmojis.Add(0x0033 u+fe0f u+20e3);	// keycap - keycap: 3
                        // vBaseEmojis.Add(0x0034 u+fe0f u+20e3);	// keycap - keycap: 4
                        // vBaseEmojis.Add(0x0035 u+fe0f u+20e3);	// keycap - keycap: 5
                        // vBaseEmojis.Add(0x0036 u+fe0f u+20e3);	// keycap - keycap: 6
                        // vBaseEmojis.Add(0x0037 u+fe0f u+20e3);	// keycap - keycap: 7
                        // vBaseEmojis.Add(0x0038 u+fe0f u+20e3);	// keycap - keycap: 8
                        // vBaseEmojis.Add(0x0039 u+fe0f u+20e3);	// keycap - keycap: 9
new Gene(0x1f51f, "keycap - keycap: 10"),
new Gene(0x1f520, "alphanum - input latin uppercase"),
new Gene(0x1f521, "alphanum - input latin lowercase"),
new Gene(0x1f522, "alphanum - input numbers"),
new Gene(0x1f523, "alphanum - input symbols"),
new Gene(0x1f524, "alphanum - input latin letters"),
new Gene(0x1f170, "alphanum - A button (blood type)"),
new Gene(0x1f18e, "alphanum - AB button (blood type)"),
new Gene(0x1f171, "alphanum - B button (blood type)"),
new Gene(0x1f191, "alphanum - CL button"),
new Gene(0x1f192, "alphanum - COOL button"),
new Gene(0x1f193, "alphanum - FREE button"),
new Gene(0x2139, "alphanum - information"),
new Gene(0x1f194, "alphanum - ID button"),
new Gene(0x24c2, "alphanum - circled M"),
new Gene(0x1f195, "alphanum - NEW button"),
new Gene(0x1f196, "alphanum - NG button"),
new Gene(0x1f17e, "alphanum - O button (blood type)"),
new Gene(0x1f197, "alphanum - OK button"),
new Gene(0x1f17f, "alphanum - P button"),
new Gene(0x1f198, "alphanum - SOS button"),
new Gene(0x1f199, "alphanum - UP! button"),
new Gene(0x1f19a, "alphanum - VS button"),
new Gene(0x1f201, "alphanum - Japanese ‚Äúhere‚Äù button"),
new Gene(0x1f202, "alphanum - Japanese ‚Äúservice charge‚Äù button"),
new Gene(0x1f237, "alphanum - Japanese ‚Äúmonthly amount‚Äù button"),
new Gene(0x1f236, "alphanum - Japanese ‚Äúnot free of charge‚Äù button"),
new Gene(0x1f22f, "alphanum - Japanese ‚Äúreserved‚Äù button"),
new Gene(0x1f250, "alphanum - Japanese ‚Äúbargain‚Äù button"),
new Gene(0x1f239, "alphanum - Japanese ‚Äúdiscount‚Äù button"),
new Gene(0x1f21a, "alphanum - Japanese ‚Äúfree of charge‚Äù button"),
new Gene(0x1f232, "alphanum - Japanese ‚Äúprohibited‚Äù button"),
new Gene(0x1f251, "alphanum - Japanese ‚Äúacceptable‚Äù button"),
new Gene(0x1f238, "alphanum - Japanese ‚Äúapplication‚Äù button"),
new Gene(0x1f234, "alphanum - Japanese ‚Äúpassing grade‚Äù button"),
new Gene(0x1f233, "alphanum - Japanese ‚Äúvacancy‚Äù button"),
new Gene(0x3297, "alphanum - Japanese ‚Äúcongratulations‚Äù button"),
new Gene(0x3299, "alphanum - Japanese ‚Äúsecret‚Äù button"),
new Gene(0x1f23a, "alphanum - Japanese ‚Äúopen for business‚Äù button"),
new Gene(0x1f235, "alphanum - Japanese ‚Äúno vacancy‚Äù button"),
new Gene(0x1f534, "geometric - red circle"),
new Gene(0x1f535, "geometric - blue circle"),
new Gene(0x26aa, "geometric - white circle"),
new Gene(0x26ab, "geometric - black circle"),
new Gene(0x2b1c, "geometric - white large square"),
new Gene(0x2b1b, "geometric - black large square"),
new Gene(0x25fc, "geometric - black medium square"),
new Gene(0x25fb, "geometric - white medium square"),
new Gene(0x25fd, "geometric - white medium-small square"),
new Gene(0x25fe, "geometric - black medium-small square"),
new Gene(0x25ab, "geometric - white small square"),
new Gene(0x25aa, "geometric - black small square"),
new Gene(0x1f536, "geometric - large orange diamond"),
new Gene(0x1f537, "geometric - large blue diamond"),
new Gene(0x1f538, "geometric - small orange diamond"),
new Gene(0x1f539, "geometric - small blue diamond"),
new Gene(0x1f53a, "geometric - red triangle pointed up"),
new Gene(0x1f53b, "geometric - red triangle pointed down"),
new Gene(0x1f4a0, "geometric - diamond with a dot"),
new Gene(0x1f518, "geometric - radio button"),
new Gene(0x1f532, "geometric - black square button"),
new Gene(0x1f533, "geometric - white square button"),
new Gene(0x1f3c1, "flag - chequered flag"),
new Gene(0x1f6a9, "flag - triangular flag"),
new Gene(0x1f38c, "flag - crossed flags"),
new Gene(0x1f3f4, "flag - black flag"),
new Gene(0x1f3f3, "flag - white flag"),
                        // vBaseEmojis.Add(0x1f3f3 u+fe0f u+200d u+1f308);	// flag - rainbow flag
                        // vBaseEmojis.Add(0x1f3f4 u+200d u+2620 u+fe0f);	// flag - pirate flag
                        // vBaseEmojis.Add(0x1f1e6 u+1f1e8);	// country-flag - flag: Ascension Island
                        // vBaseEmojis.Add(0x1f1e6 u+1f1e9);	// country-flag - flag: Andorra
                        // vBaseEmojis.Add(0x1f1e6 u+1f1ea);	// country-flag - flag: United Arab Emirates
                        // vBaseEmojis.Add(0x1f1e6 u+1f1eb);	// country-flag - flag: Afghanistan
                        // vBaseEmojis.Add(0x1f1e6 u+1f1ec);	// country-flag - flag: Antigua & Barbuda
                        // vBaseEmojis.Add(0x1f1e6 u+1f1ee);	// country-flag - flag: Anguilla
                        // vBaseEmojis.Add(0x1f1e6 u+1f1f1);	// country-flag - flag: Albania
                        // vBaseEmojis.Add(0x1f1e6 u+1f1f2);	// country-flag - flag: Armenia
                        // vBaseEmojis.Add(0x1f1e6 u+1f1f4);	// country-flag - flag: Angola
                            // vBaseEmojis.Add(0x1f1e6 u+1f1f6);	// country-flag - flag: Antarctica
                            // vBaseEmojis.Add(0x1f1e6 u+1f1f7);	// country-flag - flag: Argentina
                            // vBaseEmojis.Add(0x1f1e6 u+1f1f8);	// country-flag - flag: American Samoa
                            // vBaseEmojis.Add(0x1f1e6 u+1f1f9);	// country-flag - flag: Austria
                            // vBaseEmojis.Add(0x1f1e6 u+1f1fa);	// country-flag - flag: Australia
                            // vBaseEmojis.Add(0x1f1e6 u+1f1fc);	// country-flag - flag: Aruba
                            // vBaseEmojis.Add(0x1f1e6 u+1f1fd);	// country-flag - flag: √Öland Islands
                            // vBaseEmojis.Add(0x1f1e6 u+1f1ff);	// country-flag - flag: Azerbaijan
                            // vBaseEmojis.Add(0x1f1e7 u+1f1e6);	// country-flag - flag: Bosnia & Herzegovina
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1e7);	// country-flag - flag: Barbados
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1e9);	// country-flag - flag: Bangladesh
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1ea);	// country-flag - flag: Belgium
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1eb);	// country-flag - flag: Burkina Faso
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1ec);	// country-flag - flag: Bulgaria
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1ed);	// country-flag - flag: Bahrain
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1ee);	// country-flag - flag: Burundi
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1ef);	// country-flag - flag: Benin
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f1);	// country-flag - flag: St. Barth√©lemy
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f2);	// country-flag - flag: Bermuda
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f3);	// country-flag - flag: Brunei
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f4);	// country-flag - flag: Bolivia
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f6);	// country-flag - flag: Caribbean Netherlands
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f7);	// country-flag - flag: Brazil
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f8);	// country-flag - flag: Bahamas
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1f9);	// country-flag - flag: Bhutan
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1fb);	// country-flag - flag: Bouvet Island
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1fc);	// country-flag - flag: Botswana
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1fe);	// country-flag - flag: Belarus
                                        // vBaseEmojis.Add(0x1f1e7 u+1f1ff);	// country-flag - flag: Belize
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1e6);	// country-flag - flag: Canada
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1e8);	// country-flag - flag: Cocos (Keeling) Islands
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1e9);	// country-flag - flag: Congo - Kinshasa
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1eb);	// country-flag - flag: Central African Republic
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1ec);	// country-flag - flag: Congo - Brazzaville
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1ed);	// country-flag - flag: Switzerland
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1ee);	// country-flag - flag: C√¥te d‚ÄôIvoire
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1f0);	// country-flag - flag: Cook Islands
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1f1);	// country-flag - flag: Chile
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1f2);	// country-flag - flag: Cameroon
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1f3);	// country-flag - flag: China
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1f4);	// country-flag - flag: Colombia
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1f5);	// country-flag - flag: Clipperton Island
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1f7);	// country-flag - flag: Costa Rica
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1fa);	// country-flag - flag: Cuba
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1fb);	// country-flag - flag: Cape Verde
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1fc);	// country-flag - flag: Cura√ßao
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1fd);	// country-flag - flag: Christmas Island
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1fe);	// country-flag - flag: Cyprus
                                        // vBaseEmojis.Add(0x1f1e8 u+1f1ff);	// country-flag - flag: Czechia
                                        // vBaseEmojis.Add(0x1f1e9 u+1f1ea);	// country-flag - flag: Germany
                                        // vBaseEmojis.Add(0x1f1e9 u+1f1ec);	// country-flag - flag: Diego Garcia
                                        // vBaseEmojis.Add(0x1f1e9 u+1f1ef);	// country-flag - flag: Djibouti
                                        // vBaseEmojis.Add(0x1f1e9 u+1f1f0);	// country-flag - flag: Denmark
                                        // vBaseEmojis.Add(0x1f1e9 u+1f1f2);	// country-flag - flag: Dominica
                                        // vBaseEmojis.Add(0x1f1e9 u+1f1f4);	// country-flag - flag: Dominican Republic
                                        // vBaseEmojis.Add(0x1f1e9 u+1f1ff);	// country-flag - flag: Algeria
                                        // vBaseEmojis.Add(0x1f1ea u+1f1e6);	// country-flag - flag: Ceuta & Melilla
                                        // vBaseEmojis.Add(0x1f1ea u+1f1e8);	// country-flag - flag: Ecuador
                                        // vBaseEmojis.Add(0x1f1ea u+1f1ea);	// country-flag - flag: Estonia
                                        // vBaseEmojis.Add(0x1f1ea u+1f1ec);	// country-flag - flag: Egypt
                                        // vBaseEmojis.Add(0x1f1ea u+1f1ed);	// country-flag - flag: Western Sahara
                                        // vBaseEmojis.Add(0x1f1ea u+1f1f7);	// country-flag - flag: Eritrea
                                        // vBaseEmojis.Add(0x1f1ea u+1f1f8);	// country-flag - flag: Spain
                                        // vBaseEmojis.Add(0x1f1ea u+1f1f9);	// country-flag - flag: Ethiopia
                                        // vBaseEmojis.Add(0x1f1ea u+1f1fa);	// country-flag - flag: European Union
                                        // vBaseEmojis.Add(0x1f1eb u+1f1ee);	// country-flag - flag: Finland
                                        // vBaseEmojis.Add(0x1f1eb u+1f1ef);	// country-flag - flag: Fiji
                                        // vBaseEmojis.Add(0x1f1eb u+1f1f0);	// country-flag - flag: Falkland Islands
                                        // vBaseEmojis.Add(0x1f1eb u+1f1f2);	// country-flag - flag: Micronesia
                                        // vBaseEmojis.Add(0x1f1eb u+1f1f4);	// country-flag - flag: Faroe Islands
                                        // vBaseEmojis.Add(0x1f1eb u+1f1f7);	// country-flag - flag: France
                                        // vBaseEmojis.Add(0x1f1ec u+1f1e6);	// country-flag - flag: Gabon
                                        // vBaseEmojis.Add(0x1f1ec u+1f1e7);	// country-flag - flag: United Kingdom
                                        // vBaseEmojis.Add(0x1f1ec u+1f1e9);	// country-flag - flag: Grenada
                                        // vBaseEmojis.Add(0x1f1ec u+1f1ea);	// country-flag - flag: Georgia
                                        // vBaseEmojis.Add(0x1f1ec u+1f1eb);	// country-flag - flag: French Guiana
                                        // vBaseEmojis.Add(0x1f1ec u+1f1ec);	// country-flag - flag: Guernsey
                                        // vBaseEmojis.Add(0x1f1ec u+1f1ed);	// country-flag - flag: Ghana
                                        // vBaseEmojis.Add(0x1f1ec u+1f1ee);	// country-flag - flag: Gibraltar
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f1);	// country-flag - flag: Greenland
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f2);	// country-flag - flag: Gambia
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f3);	// country-flag - flag: Guinea
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f5);	// country-flag - flag: Guadeloupe
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f6);	// country-flag - flag: Equatorial Guinea
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f7);	// country-flag - flag: Greece
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f8);	// country-flag - flag: South Georgia & South Sandwich Islands
                                        // vBaseEmojis.Add(0x1f1ec u+1f1f9);	// country-flag - flag: Guatemala
                                        // vBaseEmojis.Add(0x1f1ec u+1f1fa);	// country-flag - flag: Guam
                                        // vBaseEmojis.Add(0x1f1ec u+1f1fc);	// country-flag - flag: Guinea-Bissau
                                        // vBaseEmojis.Add(0x1f1ec u+1f1fe);	// country-flag - flag: Guyana
                                        // vBaseEmojis.Add(0x1f1ed u+1f1f0);	// country-flag - flag: Hong Kong SAR China
                                        // vBaseEmojis.Add(0x1f1ed u+1f1f2);	// country-flag - flag: Heard & McDonald Islands
                                        // vBaseEmojis.Add(0x1f1ed u+1f1f3);	// country-flag - flag: Honduras
                                        // vBaseEmojis.Add(0x1f1ed u+1f1f7);	// country-flag - flag: Croatia
                                        // vBaseEmojis.Add(0x1f1ed u+1f1f9);	// country-flag - flag: Haiti
                                        // vBaseEmojis.Add(0x1f1ed u+1f1fa);	// country-flag - flag: Hungary
                                        // vBaseEmojis.Add(0x1f1ee u+1f1e8);	// country-flag - flag: Canary Islands
                                        // vBaseEmojis.Add(0x1f1ee u+1f1e9);	// country-flag - flag: Indonesia
                                        // vBaseEmojis.Add(0x1f1ee u+1f1ea);	// country-flag - flag: Ireland
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f1);	// country-flag - flag: Israel
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f2);	// country-flag - flag: Isle of Man
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f3);	// country-flag - flag: India
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f4);	// country-flag - flag: British Indian Ocean Territory
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f6);	// country-flag - flag: Iraq
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f7);	// country-flag - flag: Iran
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f8);	// country-flag - flag: Iceland
                                        // vBaseEmojis.Add(0x1f1ee u+1f1f9);	// country-flag - flag: Italy
                                        // vBaseEmojis.Add(0x1f1ef u+1f1ea);	// country-flag - flag: Jersey
                                        // vBaseEmojis.Add(0x1f1ef u+1f1f2);	// country-flag - flag: Jamaica
                                        // vBaseEmojis.Add(0x1f1ef u+1f1f4);	// country-flag - flag: Jordan
                                        // vBaseEmojis.Add(0x1f1ef u+1f1f5);	// country-flag - flag: Japan
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1ea);	// country-flag - flag: Kenya
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1ec);	// country-flag - flag: Kyrgyzstan
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1ed);	// country-flag - flag: Cambodia
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1ee);	// country-flag - flag: Kiribati
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1f2);	// country-flag - flag: Comoros
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1f3);	// country-flag - flag: St. Kitts & Nevis
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1f5);	// country-flag - flag: North Korea
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1f7);	// country-flag - flag: South Korea
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1fc);	// country-flag - flag: Kuwait
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1fe);	// country-flag - flag: Cayman Islands
                                        // vBaseEmojis.Add(0x1f1f0 u+1f1ff);	// country-flag - flag: Kazakhstan
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1e6);	// country-flag - flag: Laos
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1e7);	// country-flag - flag: Lebanon
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1e8);	// country-flag - flag: St. Lucia
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1ee);	// country-flag - flag: Liechtenstein
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1f0);	// country-flag - flag: Sri Lanka
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1f7);	// country-flag - flag: Liberia
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1f8);	// country-flag - flag: Lesotho
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1f9);	// country-flag - flag: Lithuania
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1fa);	// country-flag - flag: Luxembourg
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1fb);	// country-flag - flag: Latvia
                                        // vBaseEmojis.Add(0x1f1f1 u+1f1fe);	// country-flag - flag: Libya
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1e6);	// country-flag - flag: Morocco
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1e8);	// country-flag - flag: Monaco
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1e9);	// country-flag - flag: Moldova
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1ea);	// country-flag - flag: Montenegro
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1eb);	// country-flag - flag: St. Martin
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1ec);	// country-flag - flag: Madagascar
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1ed);	// country-flag - flag: Marshall Islands
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f0);	// country-flag - flag: Macedonia
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f1);	// country-flag - flag: Mali
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f2);	// country-flag - flag: Myanmar (Burma)
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f3);	// country-flag - flag: Mongolia
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f4);	// country-flag - flag: Macau SAR China
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f5);	// country-flag - flag: Northern Mariana Islands
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f6);	// country-flag - flag: Martinique
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f7);	// country-flag - flag: Mauritania
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f8);	// country-flag - flag: Montserrat
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1f9);	// country-flag - flag: Malta
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1fa);	// country-flag - flag: Mauritius
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1fb);	// country-flag - flag: Maldives
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1fc);	// country-flag - flag: Malawi
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1fd);	// country-flag - flag: Mexico
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1fe);	// country-flag - flag: Malaysia
                                        // vBaseEmojis.Add(0x1f1f2 u+1f1ff);	// country-flag - flag: Mozambique
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1e6);	// country-flag - flag: Namibia
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1e8);	// country-flag - flag: New Caledonia
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1ea);	// country-flag - flag: Niger
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1eb);	// country-flag - flag: Norfolk Island
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1ec);	// country-flag - flag: Nigeria
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1ee);	// country-flag - flag: Nicaragua
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1f1);	// country-flag - flag: Netherlands
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1f4);	// country-flag - flag: Norway
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1f5);	// country-flag - flag: Nepal
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1f7);	// country-flag - flag: Nauru
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1fa);	// country-flag - flag: Niue
                                        // vBaseEmojis.Add(0x1f1f3 u+1f1ff);	// country-flag - flag: New Zealand
                                        // vBaseEmojis.Add(0x1f1f4 u+1f1f2);	// country-flag - flag: Oman
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1e6);	// country-flag - flag: Panama
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1ea);	// country-flag - flag: Peru
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1eb);	// country-flag - flag: French Polynesia
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1ec);	// country-flag - flag: Papua New Guinea
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1ed);	// country-flag - flag: Philippines
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1f0);	// country-flag - flag: Pakistan
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1f1);	// country-flag - flag: Poland
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1f2);	// country-flag - flag: St. Pierre & Miquelon
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1f3);	// country-flag - flag: Pitcairn Islands
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1f7);	// country-flag - flag: Puerto Rico
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1f8);	// country-flag - flag: Palestinian Territories
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1f9);	// country-flag - flag: Portugal
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1fc);	// country-flag - flag: Palau
                                        // vBaseEmojis.Add(0x1f1f5 u+1f1fe);	// country-flag - flag: Paraguay
                                        // vBaseEmojis.Add(0x1f1f6 u+1f1e6);	// country-flag - flag: Qatar
                                        // vBaseEmojis.Add(0x1f1f7 u+1f1ea);	// country-flag - flag: R√©union
                                        // vBaseEmojis.Add(0x1f1f7 u+1f1f4);	// country-flag - flag: Romania
                                        // vBaseEmojis.Add(0x1f1f7 u+1f1f8);	// country-flag - flag: Serbia
                                        // vBaseEmojis.Add(0x1f1f7 u+1f1fa);	// country-flag - flag: Russia
                                        // vBaseEmojis.Add(0x1f1f7 u+1f1fc);	// country-flag - flag: Rwanda
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1e6);	// country-flag - flag: Saudi Arabia
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1e7);	// country-flag - flag: Solomon Islands
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1e8);	// country-flag - flag: Seychelles
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1e9);	// country-flag - flag: Sudan
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1ea);	// country-flag - flag: Sweden
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1ec);	// country-flag - flag: Singapore
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1ed);	// country-flag - flag: St. Helena
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1ee);	// country-flag - flag: Slovenia
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1ef);	// country-flag - flag: Svalbard & Jan Mayen
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f0);	// country-flag - flag: Slovakia
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f1);	// country-flag - flag: Sierra Leone
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f2);	// country-flag - flag: San Marino
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f3);	// country-flag - flag: Senegal
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f4);	// country-flag - flag: Somalia
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f7);	// country-flag - flag: Suriname
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f8);	// country-flag - flag: South Sudan
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1f9);	// country-flag - flag: S√£o Tom√© & Pr√≠ncipe
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1fb);	// country-flag - flag: El Salvador
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1fd);	// country-flag - flag: Sint Maarten
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1fe);	// country-flag - flag: Syria
                                        // vBaseEmojis.Add(0x1f1f8 u+1f1ff);	// country-flag - flag: Swaziland
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1e6);	// country-flag - flag: Tristan da Cunha
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1e8);	// country-flag - flag: Turks & Caicos Islands
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1e9);	// country-flag - flag: Chad
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1eb);	// country-flag - flag: French Southern Territories
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1ec);	// country-flag - flag: Togo
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1ed);	// country-flag - flag: Thailand
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1ef);	// country-flag - flag: Tajikistan
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1f0);	// country-flag - flag: Tokelau
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1f1);	// country-flag - flag: Timor-Leste
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1f2);	// country-flag - flag: Turkmenistan
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1f3);	// country-flag - flag: Tunisia
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1f4);	// country-flag - flag: Tonga
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1f7);	// country-flag - flag: Turkey
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1f9);	// country-flag - flag: Trinidad & Tobago
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1fb);	// country-flag - flag: Tuvalu
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1fc);	// country-flag - flag: Taiwan
                                        // vBaseEmojis.Add(0x1f1f9 u+1f1ff);	// country-flag - flag: Tanzania
                                        // vBaseEmojis.Add(0x1f1fa u+1f1e6);	// country-flag - flag: Ukraine
                                        // vBaseEmojis.Add(0x1f1fa u+1f1ec);	// country-flag - flag: Uganda
                                        // vBaseEmojis.Add(0x1f1fa u+1f1f2);	// country-flag - flag: U.S. Outlying Islands
                                        // vBaseEmojis.Add(0x1f1fa u+1f1f3);	// country-flag - flag: United Nations
                                        // vBaseEmojis.Add(0x1f1fa u+1f1f8);	// country-flag - flag: United States
                                        // vBaseEmojis.Add(0x1f1fa u+1f1fe);	// country-flag - flag: Uruguay
                                        // vBaseEmojis.Add(0x1f1fa u+1f1ff);	// country-flag - flag: Uzbekistan
                                        // vBaseEmojis.Add(0x1f1fb u+1f1e6);	// country-flag - flag: Vatican City
                                        // vBaseEmojis.Add(0x1f1fb u+1f1e8);	// country-flag - flag: St. Vincent & Grenadines
                                        // vBaseEmojis.Add(0x1f1fb u+1f1ea);	// country-flag - flag: Venezuela
                                        // vBaseEmojis.Add(0x1f1fb u+1f1ec);	// country-flag - flag: British Virgin Islands
                                        // vBaseEmojis.Add(0x1f1fb u+1f1ee);	// country-flag - flag: U.S. Virgin Islands
                                        // vBaseEmojis.Add(0x1f1fb u+1f1f3);	// country-flag - flag: Vietnam
                                        // vBaseEmojis.Add(0x1f1fb u+1f1fa);	// country-flag - flag: Vanuatu
                                        // vBaseEmojis.Add(0x1f1fc u+1f1eb);	// country-flag - flag: Wallis & Futuna
                                        // vBaseEmojis.Add(0x1f1fc u+1f1f8);	// country-flag - flag: Samoa
                                        // vBaseEmojis.Add(0x1f1fd u+1f1f0);	// country-flag - flag: Kosovo
                                        // vBaseEmojis.Add(0x1f1fe u+1f1ea);	// country-flag - flag: Yemen
                                        // vBaseEmojis.Add(0x1f1fe u+1f1f9);	// country-flag - flag: Mayotte
                                        // vBaseEmojis.Add(0x1f1ff u+1f1e6);	// country-flag - flag: South Africa
                                        // vBaseEmojis.Add(0x1f1ff u+1f1f2);	// country-flag - flag: Zambia
                                        // vBaseEmojis.Add(0x1f1ff u+1f1fc);	// country-flag - flag: Zimbabwe
                                        // vBaseEmojis.Add(0x1f3f4 u+e0067 u+e0062 u+e0065 u+e006e u+e0067 u+e007f);	// subdivision-flag - flag: England
                                        // vBaseEmojis.Add(0x1f3f4 u+e0067 u+e0062 u+e0073 u+e0063 u+e0074 u+e007f);	// subdivision-flag - flag: Scotland
        };            

        }

        static readonly EmojiCombineAlgo _algo = new EmojiCombineAlgo(Gene.genes.Count-1);
    }
}
