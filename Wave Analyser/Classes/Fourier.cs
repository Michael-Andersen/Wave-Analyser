using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser
{
    class Fourier
    {
		public static Complex[] dft(int[] s, int N)
		{
			Complex[] results = new Complex[N];
			for (int f = 0; f < N; f++)
			{
				results[f] = new Complex(0, 0);
				for (int t = 0; t < N; t++)
				{
					results[f] += new Complex(s[t] * Math.Cos(t * 2 * Math.PI * (f) / N),
						-1 * s[t] * Math.Sin(t * 2 * Math.PI * (f) / N));
				}
				results[f] /= new Complex(N, 0);
			}
			return results;
		}

		public static double FrequencyFromIndex(int binNumber, double sampleRate, int numSamples)
		{
			return binNumber * sampleRate / numSamples;
		}

		public static double BinSize(double sampleRate, int numSamples)
		{
			return sampleRate / numSamples;
		}

		public static double[] idft(Complex[] amplitudes, int N)
		{
			Double[] results = new double[N];
			for (int t = 0; t < N; t++)
			{
				for (int f = 0; f < N; f++)
				{
					results[t] += ((amplitudes[f] * new Complex(Math.Cos(t * 2 * Math.PI * (f) / N), 0))
						+ (amplitudes[f] * new Complex(0, Math.Sin(t * 2 * Math.PI * (f) / N)))).real;
				}
			}
			return results;
		}
	}
}
