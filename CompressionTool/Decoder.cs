using System.Collections;
using System.Reflection;
using System.Text;

namespace CompressionTool
{
    internal class Decoder
    {
        private readonly string _compressed;
        private readonly string _header;
        private readonly string _output;

        public Decoder()
        {
            _output = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "output.txt");
            _header = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "header.txt");
            _compressed = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "compressed.comp");

            if (File.Exists(_output))
            {
                File.Delete(_output);
            }
        }

        public async Task Decode()
        {
            var header = await File.ReadAllLinesAsync(_header);

            // 1. Get the character frequencies from the header file
            var frequencies = GetFrequencies(header);

            // 2. Get the code string length from the header file
            var length = GetCodedStringLength(header);

            // 3. Get the coded string from compressed file
            var codedString = await GetCodedString(length);

            // 4. Build tree based on frequencies
            var tree = BuildTree(frequencies);

            // 5. Write decoded data
            await WriteDecodedData(tree, codedString);
        }

        private HuffmanNode BuildTree(Dictionary<char, int> frequencies)
        {
            var tree = new List<HuffmanNode>();

            foreach (var character in frequencies)
            {
                tree.Add(new HuffmanNode
                {
                    Character = character.Key,
                    Frequency = character.Value
                });
            }

            tree.Sort();

            while (tree.Count != 1 && tree.Count > 0)
            {
                var left = tree[0]; tree.RemoveAt(0);
                var right = tree[0]; tree.RemoveAt(0);

                tree.Add(new HuffmanNode
                {
                    Character = '^',
                    Frequency = left.Frequency + right.Frequency,
                    LeftNode = left,
                    RightNode = right
                });

                tree.Sort();
            }

            return tree[0];
        }

        private async Task<string> GetCodedString(int length)
        {
            var byteString = new StringBuilder();

            if (File.Exists(_compressed))
            {
                await using FileStream fileStream = File.OpenRead(_compressed);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);

                foreach (byte b in buffer)
                {
                    byteString.Append(ToBitString(Reverse(new BitArray(new byte[] { b }))));
                }
            }

            // Trim off extra characters from last byte
            var readString = byteString.ToString();
            var lastByte = readString.Substring(readString.Length - 8);
            var diff = readString.Length - length;

            lastByte = lastByte[diff..];
            readString = readString.Remove(readString.Length - 8);
            readString += lastByte;

            return readString;
        }

        private int GetCodedStringLength(string[] header)
        {
            var index = header.ToList().FindIndex(x => x.Equals("|*|"));

            if (index < 0)
            {
                return 0;
            }

            return Convert.ToInt32(header[index + 1]);
        }

        private Dictionary<char, int> GetFrequencies(string[] header)
        {
            var frequencies = new Dictionary<char, int>();
            var index = header.ToList().FindIndex(x => x.Equals("|*|"));

            for (int i = 0; i < index; i++)
            {
                frequencies.Add(header[i][0], Convert.ToInt32(header[i][1..]));
            }

            return frequencies;
        }

        private BitArray Reverse(BitArray array)
        {
            var length = array.Length;
            var mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                (array[length - i - 1], array[i]) = (array[i], array[length - i - 1]);
            }

            return array;
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

        private async Task WriteDecodedData(HuffmanNode root, string codedString)
        {
            var text = new StringBuilder();
            var currentNode = root;

            foreach (var character in codedString)
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