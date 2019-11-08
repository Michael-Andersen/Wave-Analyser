using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser
{
	class Filter
	{
		private float[] timeDom;
		private Complex[] freqDom;
		private double[] test;
		private double[] test2;
		public Filter(int size, int passStart, int passEnd)
		{
			freqDom = new Complex[size];
			int mirrorStart = size - passEnd;
			int mirrorEnd = size - passStart;
			for (int i = 0; i < freqDom.Length; i++)
			{
				if (i >= passStart && i <= passEnd) {
					freqDom[i] = new Complex(1, 1);
				} else if (i >= mirrorStart && i <= mirrorEnd)
				{
					freqDom[i] = new Complex(1, -1);
				} else
				{
					freqDom[i] = new Complex(0, 0);
				}
			}
			test = new double[size];
			for (int i = 0; i < test.Length; i++)
			{
				test[i] = freqDom[i].real;
			}
			
			timeDom = Fourier.IDFT(freqDom, freqDom.Length);
			Complex[] testC = Fourier.DFT(timeDom, timeDom.Length);
			test2 = new double[testC.Length];
			for (int i = 0; i < test2.Length; i++)
			{
				test2[i] = testC[i].real;
			}
			for (int i = 0; i < timeDom.Length; i++)
			{
				timeDom[i] /= timeDom.Length;
			}
			
		}

		public float[] Convolve(float[] samples)
		{
			float[] results = new float[samples.Length];
			for (int i = 0; i < samples.Length; i++)
			{
				float sampleC = 0;
				for (int j = 0; j < timeDom.Length; j++)
				{
					float oldSample = ((i + j) < samples.Length) ? samples[i + j] : 0;
					sampleC += oldSample * timeDom[j];
				}
				results[i] = sampleC; // / timeDom.Length;
			}
			return results;
		}

		public float[] getFilter()
		{
			return timeDom;
		}

		public float[] filter(float[] samples) {
			
			for (int i = 0; i < samples.Length; i+=freqDom.Length)
			{
				int size = (i + freqDom.Length < samples.Length) ? freqDom.Length
					: samples.Length - i;
				float[] chunk = new float[size];
				for (int j = 0; j < chunk.Length; j++)
				{
					chunk[j] = samples[i + j];
				}
				Complex[] freqs = Fourier.DFT(chunk, freqDom.Length);
				for (int j = 0; j < freqs.Length; j++)
				{
					freqs[j] = freqs[j] * freqDom[j];
				}
				chunk = Fourier.IDFT(freqs, chunk.Length);
				for (int j = 0; j < chunk.Length; j++)
				{
					samples[i + j] = chunk[j];
				}
			}
			return samples;
		}
		
	}
}
