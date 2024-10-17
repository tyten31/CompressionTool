using System.Reflection;

namespace CompressionTool
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var encoder = new Encoder();
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "text.txt");

            encoder.Encode(path);


            Console.ReadLine();
        }
    }
}