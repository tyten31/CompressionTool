namespace CompressionTool
{
    internal class Encoder
    {
        public void Encode(string path)
        {
            var charFrequency = GetCharacterFrequency(path);

            // Display Character Frequency List
            //foreach (var frequency in charFrequency)
            //{
            //    Console.WriteLine($"{frequency.Key}: {frequency.Value}");
            //}

            var huffmanTree = BuildTree(charFrequency);
            //PrintCodes(huffmanTree, "");
        }

        private LeafNode BuildTree(List<KeyValuePair<char, int>> characterFrequencies)
        {
            var top = new LeafNode();
            var minHeap = new List<LeafNode>();

            foreach (var character in characterFrequencies)
            {
                minHeap.Add(new LeafNode { Character = character.Key, Frequency = character.Value });
            }

            while (minHeap.Count != 1 && minHeap.Count > 0)
            {
                var left = minHeap[0];
                minHeap.RemoveAt(0);

                var right = minHeap[0];
                minHeap.RemoveAt(0);

                top = new LeafNode { Character = '$', Frequency = left.Frequency + right.Frequency, LeftNode = left, RightNode = right };

                minHeap.Add(top);
                minHeap.Sort((pair1, pair2) => pair1.Frequency.CompareTo(pair2.Frequency));
            }

            return top;
        }

        private List<KeyValuePair<char, int>> GetCharacterFrequency(string path)
        {
            var dictionary = new Dictionary<char, int>();

            if (File.Exists(path))
            {
                using StreamReader read = new(path);
                string? line;

                while ((line = read.ReadLine()) != null)
                {
                    foreach (var key in line)
                    {
                        if (dictionary.TryGetValue(key, out int value))
                        {
                            dictionary[key] = value + 1;
                        }
                        else
                        {
                            dictionary.Add(key, 1);
                        }
                    }
                }
            }

            var list = dictionary.ToList();

            list.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            return list;
        }

        private void PrintCodes(LeafNode root, string str)
        {
            if (root == null)
            {
                return;
            }

            if (root.Character != '$')
            {
                Console.WriteLine(root.Character + ": " + str);
            }

            PrintCodes(root.LeftNode, str + "0");
            PrintCodes(root.RightNode, str + "1");
        }
    }
}