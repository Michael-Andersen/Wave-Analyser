using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wave_Analyser
{
    class Fourier
    {
		public static Complex[] DFT(float[] s, int N)
		{
			Complex[] results = new Complex[N];
			for (int f = 0; f < N; f++)
			{
				results[f] = new Complex(0, 0);
				for (int t = 0; t < N; t++) 
				{
                    try
                    {
                        results[f] += new Complex(s[t] * Math.Cos(t * 2 * Math.PI * (f) / N),
                            -1 * s[t] * Math.Sin(t * 2 * Math.PI * (f) / N));
                    }
                    catch (IndexOutOfRangeException e){ 
                    }
				}
				results[f] /= new Complex(N, 0);
			}
			return results;
		}

		public static void DFT_ThreadH(float[] s, int N, Complex[] results, int start, int fullN)
		{
			for (int f = start; f < N; f++)
			{
				results[f] = new Complex(0, 0);
				for (int t = 0; t < fullN; t++) //s.Length
				{
					results[f] += new Complex(s[t] * Math.Cos((t) * 2 * Math.PI * (f) / fullN),
						-1 * s[t] * Math.Sin((t) * 2 * Math.PI * (f) / fullN));
				}
				results[f] /= new Complex(fullN, 0);
			}
		}

		public static Complex[] DFT_Thread(float[] s, int N, int threads)
		{
			Thread[] threadArr = new Thread[threads];
			int NperThread = N / threads;
		    Complex[] results = new Complex[N];
			for (int i = 0; i < threads; i++)
			{
				int start = NperThread * i;
				if (i == (threads - 1))
				{
					NperThread += (N - NperThread * threads);
				}
				threadArr[i] = new Thread(()=>DFT_ThreadH(s, NperThread + start, results, start, N));
				threadArr[i].Start();
			}
			for (int i = 0; i < threadArr.Length; i++)
			{
				threadArr[i].Join();
			}
			return results;
		}

		public static double FrequencyFromIndex(int binNumber, double sampleRate, int numSamples)
		{
			return binNumber * sampleRate / numSamples;
		}

		public static double BinSize(double sampleRate, int numBins)
		{
			return sampleRate / numBins;
		}

		public static float[] IDFT(Complex[] amplitudes, int N)
		{
			float[] results = new float[N];
			for (int t = 0; t < N; t++)
			{
				Complex c = new Complex(0, 0);
				for (int f = 0; f < N; f++)
				{
					
					c += (amplitudes[f] * new Complex(Math.Cos(t * 2 * Math.PI * (f) / N), 0))
						+ (amplitudes[f] * new Complex(0, Math.Sin(t * 2 * Math.PI * (f) / N)));
				}
				results[t] = (float)c.real;
			}
			return results;
		}

		public static void IDFT_ThreadH(Complex[] amplitudes, int N, float[] results, int start, int finish, Boolean divideByN )
		{
			for (int t = start; t < finish; t++)
			{
				Complex c = new Complex(0, 0);
				for (int f = 0; f < N; f++)
				{

					c += (amplitudes[f] * new Complex(Math.Cos(t * 2 * Math.PI * (f) / N), 0))
						+ (amplitudes[f] * new Complex(0, Math.Sin(t * 2 * Math.PI * (f) / N)));
				}
				results[t] = (float)c.real;
				if (divideByN)
				{
					results[t] /= N;
				}
			}
		}

		public static float[] IDFT_Thread(Complex[] amplitudes, int N, int threads, Boolean divideByN)
		{
			Thread[] threadArr = new Thread[threads];
			int NperThread = N / threads;
			float[] results = new float[N];
			for (int i = 0; i < threads; i++)
			{
				int start = NperThread * i;
				if (i == (threads - 1))
				{
					NperThread += (N - NperThread * threads);
				}
				threadArr[i] = new Thread(() => IDFT_ThreadH(amplitudes, N, results, start, NperThread + start, divideByN));
				threadArr[i].Start();
			}
			for (int i = 0; i < threadArr.Length; i++)
			{
				threadArr[i].Join();
			}
			return results;
		}
	}
}
