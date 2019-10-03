using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser.Classes
{
    public class AudioSignal
    {
        private double sampleRate;
        private int bitDepth;
        private int maxAmp;
        private int minAmp;
        private bool signed;
        private int[] samples;
        private int[] selection;
        private Random random;

        public AudioSignal(double sampleRate, int bitDepth, bool signed)
        {
            this.sampleRate = sampleRate;
            this.bitDepth = bitDepth;
            this.signed = signed;
            this.maxAmp = signed ? (int)Math.Pow(2, bitDepth - 1) : (int)Math.Pow(2, bitDepth);
            this.minAmp = signed ? -maxAmp-- : 0;
            this.random = new Random();
        }

        public double SampleRate { get => sampleRate; }
        public int MaxAmp { get => maxAmp; }
        public int MinAmp { get => minAmp; }
        public bool Signed { get => signed; }
        public int[] Samples { get => samples; }
        public int[] Selection { get => selection; }

        public void SetSelection(int start, int end)
        {
            int j = 0;
            selection = new int[end - start];
            for (int i = start; i < end; i++)
            {
                selection[j++] = samples[i];
            }
        }

        public void GenerateSineData(double seconds, int[] freqs)
        {
            samples = new int[(int)(sampleRate * seconds)];
            
            for (int i = 0; i < samples.Length; i++)
            {
                double time = i / sampleRate;
                samples[i] = 0;
                for (int j = 0; j < freqs.Length; j++)
                {
                    double amp = (maxAmp) / freqs.Length * Math.Sin(2 * Math.PI * freqs[j] * time);
                    samples[i] += (int)amp;
                }
            }
        }

        public void GenerateRandomData(double seconds)
        {
            samples = new int[(int)(sampleRate * seconds)];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = random.Next(minAmp, maxAmp + 1);
            }
        }
    }
}
