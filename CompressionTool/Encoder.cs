using System.Collections;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace CompressionTool
{
    internal class Encoder
    {
        private readonly string _compressed;
        private readonly string _header;
        private readonly string _input;

        public Encoder()
        {
            _input = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "input.txt");
            _header = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "header.txt");
            _compressed = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "compressed.comp");

            if (File.Exists(_header))
            {
                File.Delete(_header);
            }

            if (File.Exists(_compressed))
            {
                File.Delete(_compressed);
            }
        }

        public async Task Encode()
        {
            // 1. Get char frequencies
            var frequencies = await GetFrequencies();

            // 2. Build tree based on frequencies
            var tree = BuildTree(frequencies);

            // 3. Build char codes based on tree
            var codes = BuildCodes(tree);

            // 4. Build coded string representation of file
            var codedString = await BuildCodedString(codes);

            // 5. Compress codedString into byte[]
            var compressedBytes = Compress(codedString);

            // 6. Write frequencies to compression as the header
            await WriteFileHeader(frequencies, codedString.Length);

            // 7. Write compressed text
            await WriteEncodedData(compressedBytes);
        }

        private async Task<string> BuildCodedString(Dictionary<char, string> codes)
        {
            var stringBuilder = new StringBuilder();

            if (File.Exists(_input))
            {
                string? line;
                using StreamReader read = new(_input);

                while ((line = await read.ReadLineAsync()) != null)
                {
                    line += "\\n\\r";

                    foreach (var key in line)
                    {
                        stringBuilder.Append(codes.First(x => x.Key == key).Value);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        private Dictionary<char, string> BuildCodes(HuffmanNode root)
        {
            var codes = new Dictionary<char, string>();
            BuildCodes(codes, root);

            return codes;
        }

        private void BuildCodes(Dictionary<char, string> codes, HuffmanNode root, string value = "")
        {
            if (root == null)
            {
                return;
            }

            if (root.Character != '^')
            {
                codes.Add(root.Character, value);
            }

            BuildCodes(codes, root.LeftNode, value + "0");
            BuildCodes(codes, root.RightNode, value + "1");
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

        private byte[] Compress(string text)
        {
            var byteList = new List<byte>();

            foreach (var chunk in text.Chunk(8))
            {
                var chunckString = string.Empty;

                foreach (char c in chunk)
                {
                    chunckString += c;
                }

                byteList.Add(Convert.ToByte(new string(chunckString), 2));
            }

            return [.. byteList];
        }

        private async Task<Dictionary<char, int>> GetFrequencies()
        {
            var frequencies = new Dictionary<char, int>();

            if (File.Exists(_input))
            {
                string? line;
                using StreamReader read = new(_input);

                while ((line = await read.ReadLineAsync()) != null)
                {
                    line += "\\n\\r";

                    foreach (var key in line)
                    {
                        if (frequencies.TryGetValue(key, out int value))
                        {
                            frequencies[key] = value + 1;
                        }
                        else
                        {
                            frequencies.Add(key, 1);
                        }
                    }
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

        private async Task WriteEncodedData(byte[] text)
        {
            await using FileStream stream = new(_compressed, FileMode.Append);
            await stream.WriteAsync(text);
        }

        private async Task WriteFileHeader(Dictionary<char, int> frequencies, int codesStringLength)
        {
            if (File.Exists(_input))
            {
                // Write out frequencies in header
                await using StreamWriter compressedFile = new(_header, true);
                foreach (var item in frequencies.OrderBy(x => x.Value))
                {
                    await compressedFile.WriteLineAsync($"{item.Key}{item.Value}");
                }

                // Write coded string length
                await compressedFile.WriteLineAsync("|*|");
                await compressedFile.WriteLineAsync($"{codesStringLength}");
            }
        }
    }
}