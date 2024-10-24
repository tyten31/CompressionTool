namespace CompressionTool
{
    internal class HuffmanNode : IComparable<HuffmanNode>
    {
        public char Character { get; set; }

        public int Frequency { get; set; }

        public HuffmanNode LeftNode { get; set; }

        public HuffmanNode RightNode { get; set; }

        public int CompareTo(HuffmanNode other)
        {
            var frequencyDiff = Frequency - other.Frequency;

            if (frequencyDiff == 0)
            {
                return Character - other.Character;
            }

            return frequencyDiff;
        }
    }
}