namespace CompressionTool
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var decoder = new Decoder();
            var encoder = new Encoder();
            var spinner = new ConsoleSpinner();
            var spinnerTask = Task.Run(spinner.Start);

            try
            {
                Console.WriteLine("Encoding");
                await encoder.Encode();

                Console.WriteLine("Decoding");
                await decoder.Decode();
            }
            finally
            {
                spinner.Stop();
                await spinnerTask;
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}