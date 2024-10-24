namespace CompressionTool
{
    internal class ConsoleSpinner
    {
        private readonly int _delay = 100;
        private int _counter;
        private bool _running;

        public ConsoleSpinner()
        {
            _counter = 0;
            _running = true;
        }

        public void Start()
        {
            while (_running)
            {
                Console.CursorVisible = false;
                //Console.Clear();

                Turn();

                Thread.Sleep(_delay);
            }
        }

        public void Stop()
        {
            _running = false;

            //Console.Clear();
            Console.CursorVisible = true;
        }

        private void Turn()
        {
            _counter++;

            switch (_counter % 10)
            {
                case 0: Console.Write("=>    "); break;
                case 1: Console.Write(" =>   "); break;
                case 2: Console.Write("  =>  "); break;
                case 3: Console.Write("   => "); break;
                case 4: Console.Write("    =>"); break;
                case 5: Console.Write("    <="); break;
                case 6: Console.Write("   <= "); break;
                case 7: Console.Write("  <=  "); break;
                case 8: Console.Write(" <=   "); break;
                case 9: Console.Write("<=    "); break;
            }

            Console.SetCursorPosition(Console.CursorLeft -6, Console.CursorTop);
        }
    }
}