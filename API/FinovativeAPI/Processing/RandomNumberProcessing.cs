using System;
using System.Threading;

namespace Project.Processing
{
    public class RandomNumberProcessing
    {
        private static readonly Random random = new Random();

        public static void RandomDelay(int min, int max)// miliseconds
        {
            int randomMiliSeconds = random.Next(min, max);
            Thread.Sleep(randomMiliSeconds);
        }
    }
}