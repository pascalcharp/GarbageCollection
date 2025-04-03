using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    internal class RandomGenerator(int seed)
    {
        public const double TOLERANCE = 0.001;
        public const int MAX_ATTEMPTS = 3;

        private readonly Random _random = new(seed);

        public int GenerateUniform(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        public int GenerateCustom(int min, List<double> probabilities)
        {
            double total = probabilities.Sum();
            if (Math.Abs(total - 1.0) >= TOLERANCE)
            {
                throw new Exception($"Probabilities must sum to 1.0, got total={total}.");
            }

            double unitValue = _random.NextDouble();

            double lastFloor = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                double probability = probabilities[i];
                if (unitValue < lastFloor + probability)
                {
                    return min + i;
                }
                lastFloor += probability;
            }

            throw new Exception("Unexpected error.");
        }

        public int GenerateNormal(int min, int max, double mean, double standardDeviation)
        {
            int value;
            int attempt = 0;

            do
            {
                attempt++;
                if (attempt <= MAX_ATTEMPTS)
                {
                    // Get value in standard normal distribution instead of uniform distribution using Box-Muller transform.
                    double u1 = _random.NextDouble();
                    double u2 = _random.NextDouble();
                    double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

                    // Scale, shift and round value.
                    value = (int)Math.Round(z0 * standardDeviation + mean);
                }
                else
                {
                    // Fallback to uniform distribution.
                    return GenerateUniform(min, max);
                }
            }
            while (value < min || value > max);

            return value;
        }
    }
}
