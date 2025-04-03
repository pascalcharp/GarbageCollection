using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    internal static class Utils
    {
        public static void WritePercentage(string text, int numerator, int denominator)
        {
            double percentage = Math.Round(100.0 * numerator / denominator, 2);
            Console.WriteLine($"{text} : {numerator}/{denominator} ({percentage}%)");
        }
    }
}
