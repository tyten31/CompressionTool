using System.Collections;
using System.Reflection;
using System.Text;

namespace CompressionTool
{
    internal class Decoder
    {
        private readonly Dictionary<char, string> _codes;
        private readonly string _compressed;
        private readonly Dictionary<char, int> _frequencies;
        private readonly string _output;
        private readonly List<HuffmanNode> _tree;
        private string _encodedString;

        public Decoder()
        {
            _output = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "output.txt");
            _compressed = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "compressed.txt");
            _codes = [];
            _tree = [];
            _frequencies = [];

            if (File.Exists(_output))
            {
                File.Delete(_output);
            }
        }

        public async Task Decode()
        {
            // 1. Get the character frequencies from the file header
            var frequencies = await GetFrequencies();

            // 2. Get the compressed text
            var codedString = await GetCodedString();

            // 3. Build tree based on frequencies
            var tree = BuildTree(frequencies);

            // 4. Write decoded data
            //await WriteDecodedData(tree);
        }

        private HuffmanNode BuildTree(Dictionary<char, int> frequencies)
        {
            var tree = new List<HuffmanNode>();

            foreach (var character in frequencies)
            {
                tree.Add(new HuffmanNode { Character = character.Key, Frequency = character.Value });
            }

            tree.Sort();

            while (tree.Count != 1 && tree.Count > 0)
            {
                var left = tree[0];
                tree.RemoveAt(0);

                var right = tree[0];
                tree.RemoveAt(0);

                tree.Add(new HuffmanNode { Character = '^', Frequency = left.Frequency + right.Frequency, LeftNode = left, RightNode = right });
                tree.Sort();
            }

            return tree[0];
        }

        private async Task<string> GetCodedString()
        {
            var byteString = string.Empty;
            if (File.Exists(_compressed))
            {
                string? line;
                var header = true;

                using StreamReader read = new(_compressed);

                while ((line = await read.ReadLineAsync()) != null)
                {
                    if (line.Equals("|*|"))
                    {
                        header = false;
                    }
                    else if (!header)
                    {
                        byteString += line;
                    }
                }
            }

            var a1 = Encoding.ASCII.GetBytes(byteString.ToString().Trim()).Reverse().ToArray();
            var a2 = new BitArray(a1);
            var a3 = ToBitString(a2);

            return ToBitString(new BitArray(Encoding.ASCII.GetBytes(byteString.ToString().Trim())));
        }

        private async Task<Dictionary<char, int>> GetFrequencies()
        {
            var frequencies = new Dictionary<char, int>();

            if (File.Exists(_compressed))
            {
                string? line;
                using StreamReader read = new(_compressed);

                while ((line = await read.ReadLineAsync()) != null)
                {
                    if (line.Equals("|*|"))
                    {
                        break;
                    }

                    frequencies.Add(line[0].ToString()[0], Convert.ToInt32(line[1..]));
                }
            }

            return frequencies;
        }

        private string ToBitString(BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

        private async Task WriteDecodedData(HuffmanNode root)
        {
            var text = new StringBuilder();
            var currentNode = root;

            foreach (var character in _encodedString)
            {
                if (character.Equals('0'))
                {
                    currentNode = currentNode.LeftNode;
                }
                else
                {
                    currentNode = currentNode.RightNode;
                }

                if (currentNode.LeftNode == null && currentNode.RightNode == null)
                {
                    text.Append(currentNode.Character);
                    currentNode = root;
                }
            }

            await using StreamWriter outputFile = new(_output, true);

            foreach (var t in text.ToString().Split("\\n\\r"))
            {
                await outputFile.WriteLineAsync(t);
            }
        }
    }
}