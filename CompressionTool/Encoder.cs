namespace CompressionTool
{
    internal class Encoder
    {
        public void Encode(string path)
        {
            var charFrequency = GetCharacterFrequency(path);

            foreach (var frequency in charFrequency)
            {
                Console.WriteLine($"{frequency.Key}: {frequency.Value}");
            }
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
    }
}