namespace RaspbianFezHat
{
    using System;

    public class SimpleLogger
    {
        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine("ERROR: " + format, args);
        }
    }
}
