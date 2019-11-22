using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser
{
    class Windowing
    {
        public static float[] Apply(float[] samples, WindowingMode mode)
        {
            int numSamples = samples.Length;
            float[] weights = new float[numSamples];

            switch (mode)
            {
                case WindowingMode.Triangular:
                    weights = GenerateTriangularWindow(numSamples);
                    break;
                case WindowingMode.Welch:
                    weights = GenerateWelchWindow(numSamples);
                    break;
                default:
                    return samples;
            }

            for (int i = 0; i < numSamples; i++)
            {
                samples[i] *= weights[i];
            }
            return samples;
        }

        private static float[] GenerateTriangularWindow(int N)
        {
            float[] weights = new float[N];
            for (int n = 0; n < N; n++)
            {
                weights[n] = 1 - Math.Abs((n - (N / 2f)) / (N / 2f));
            }
            return weights;
        }

        private static float[] GenerateWelchWindow(int N)
        {
            float[] weights = new float[N];
            for (int n = 0; n < N; n++)
            {
                weights[n] = 1 - (float)Math.Pow((n - (N / 2f)) / (N / 2f), 2);
            }
            return weights;
        }
    }
}
