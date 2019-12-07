using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wave_Analyser
{
	class Filter
	{
		[DllImport("RecordingLibrary.dll")]
		public static extern IntPtr convolveSSE(IntPtr samples, uint slength, 
			IntPtr filter, uint fsize, ushort depth, IntPtr results);

		private float[] results;
		private float[] timeDom;
		private Complex[] freqDom;

		public float[] TimeDom { get => timeDom; }

		public Filter(int size, int passStart, int passEnd, int threads)
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
			timeDom = Fourier.IDFT_Thread(freqDom, freqDom.Length, threads, true);			
		}

		public float[] ConvolveDLL(float[] samples, int length, int bitDepth)
		{
			IntPtr floatIn = Marshal.AllocHGlobal(Marshal.SizeOf(samples[0])*samples.Length);
			Marshal.Copy(samples, 0, floatIn, samples.Length);
			IntPtr filterIn = Marshal.AllocHGlobal(Marshal.SizeOf(timeDom[0])*timeDom.Length);
			Marshal.Copy(timeDom, 0, filterIn, timeDom.Length);
			results = new float[length];
			IntPtr resultsIn = Marshal.AllocHGlobal(Marshal.SizeOf(results[0])*length);
			Marshal.Copy(results, 0, resultsIn, length);
			IntPtr pfloat = convolveSSE(floatIn, Convert.ToUInt32(length), filterIn, Convert.ToUInt32(timeDom.Length), Convert.ToUInt16(bitDepth), resultsIn);
			Marshal.FreeHGlobal(filterIn);
			Marshal.FreeHGlobal(floatIn);
			Marshal.Copy(pfloat, results, 0, length);
			Marshal.FreeHGlobal(resultsIn);
			return results;
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
				results[i] = sampleC; 
			}
			return results;
		}

		public void Convolve_ThreadH(float[] samples, float[] results, int start, int size)
		{
			for (int i = start; i < start + size; i++)
			{
				float sampleC = 0;
				for (int j = 0; j < timeDom.Length; j++)
				{
					float oldSample = ((i + j) < samples.Length) ? samples[i + j] : 0;
					sampleC += oldSample * timeDom[j];
				}
				results[i] = sampleC;
			}
		}

		public float[] Convolve_Thread(float[] samples, int threads)
		{
			Thread[] threadArr = new Thread[threads];
			int NperThread = samples.Length / threads;
			float[] results = new float[samples.Length];
			for (int i = 0; i < threads; i++)
			{
				int start = NperThread * i;
				if (i == (threads - 1))
				{
					NperThread += (samples.Length - NperThread * threads);
				}
				threadArr[i] = new Thread(() => Convolve_ThreadH(samples, results, start, NperThread));
				threadArr[i].Start();
			}
			for (int i = 0; i < threadArr.Length; i++)
			{
				threadArr[i].Join();
			}
			return results;
		}

		//deprecated for filtering in freq domain without convolution
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
