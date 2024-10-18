namespace CompressionTool
{
    internal class LeafNode
    {
        public char Character { get; set; }

        public int Frequency { get; set; }

        public LeafNode LeftNode { get; set; }

        public LeafNode RightNode { get; set; }
    }
}