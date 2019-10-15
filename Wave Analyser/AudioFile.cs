using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser.Classes
{
    public class AudioFile
	{
        private AudioFileHeader header;
        private Random random;
        private int nyquistLimit;
		private double maxAmp;
		private double minAmp;
        private float[] samples;
        private float[] selection;
		private float[] right;
		private float[] left;
		private bool leftSelected;
        
		private AudioFile(AudioFileHeader header)
		{
            this.header = header;
            this.nyquistLimit = (int) header.sampleRate / 2;
			this.maxAmp = 1;
			this.minAmp = -1;
			this.random = new Random();
		}

		public int SampleRate { get => header.sampleRate; }
        public int NyquistLimit { get => nyquistLimit; }
		public double MaxAmp { get => maxAmp; }
		public double MinAmp { get => minAmp; }
		public bool Signed { get => header.signed; }
		public float[] Samples { get => samples; set => samples = value; }
		public float[] Left { get => left; }
		public float[] Right { get => right; }
		public float[] Selection { get => selection; }
		public bool LeftSelected { get => leftSelected; set => leftSelected = value; }

		public void SetSelection(int start, int end, bool isleftChannel)
		{
			int j = 0;
			selection = new float[end - start];
			for (int i = start; i < end; i++)
			{
				selection[j] = (isleftChannel) ? left[i] : right[i];
				j++;
			}
		}

		public void GenerateSineData(double seconds, int[] freqs)
		{
			samples = new float[(int)(SampleRate * seconds)];

			for (int i = 0; i < samples.Length; i++)
			{
				double time = i / SampleRate;
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
			samples = new float[(int)(SampleRate * seconds)];

			for (int i = 0; i < samples.Length; i++)
			{
				samples[i] = random.Next((int)minAmp, (int)maxAmp + 1);
			}
		}

		public static AudioFile ReadFromFile(String filename)
		{
            AudioFileHeader header = new AudioFileHeader();
            AudioFile audio = null;

            try
			{
				using (FileStream fs = File.Open(filename, FileMode.Open))
				{
					BinaryReader reader = new BinaryReader(fs);

					header.chunkID = reader.ReadInt32();
					header.fileSize = reader.ReadInt32();
                    header.riffType = reader.ReadInt32();
                    header.fmtID = reader.ReadInt32();
                    header.fmtSize = reader.ReadInt32();
                    header.fmtCode = reader.ReadInt16();
					header.channels = reader.ReadInt16();
					header.sampleRate = reader.ReadInt32();
					header.byteRate = reader.ReadInt32();
					header.fmtBlockAlign = reader.ReadInt16();
					header.bitDepth = reader.ReadInt16();

					if (header.fmtSize == 18)
					{
						// Read any extra values
						header.fmtExtraSize = reader.ReadInt16();
						reader.ReadBytes(header.fmtExtraSize);
					}

					// chunk 2
					header.dataID = reader.ReadInt32();
					header.bytes = reader.ReadInt32();

					// DATA!
					byte[] byteArray = reader.ReadBytes(header.bytes);

					int bytesForSamp = header.bitDepth / 8;
					int samps = header.bytes / bytesForSamp;
					float[] asFloat = null;

					switch (header.bitDepth)
					{
						case 64:
							header.signed = true;
							double[] asDouble = new double[samps];
							Buffer.BlockCopy(byteArray, 0, asDouble, 0, header.bytes);
							asFloat = Array.ConvertAll(asDouble, e => (float)e);
							break;
						case 32:
                            header.signed = true;
							asFloat = new float[samps];
							Buffer.BlockCopy(byteArray, 0, asFloat, 0, header.bytes);
							break;
						case 16:
                            header.signed = true;
							Int16[] asInt16 = new Int16[samps];
							Buffer.BlockCopy(byteArray, 0, asInt16, 0, header.bytes);
							asFloat = Array.ConvertAll(asInt16, e => (e < 0) ?
								-1 * e / (float)Int16.MinValue : e / (float)Int16.MaxValue);
							break;
						case 8:
                            header.signed = false;
							asFloat = Array.ConvertAll(byteArray, e => ((e + SByte.MinValue) < 0) ?
							-1 * (e + SByte.MinValue) / (float)SByte.MinValue :
							(e + SByte.MinValue) / (float)SByte.MaxValue);
							break;
						default:
							break;
					}

                    audio = new AudioFile(header);
					audio.Samples = asFloat;
					audio.DeMux();
				}
			}
			catch (Exception e)
			{
                Console.WriteLine(e.Message);
			}

            return audio;
		}

		public void DeMux()
		{
			switch (header.channels)
			{
				case 1:
					left = samples;
					right = samples;
					break;
				case 2:
					int samps = samples.Length / 2;
					left = new float[samps];
					right = new float[samps];
					for (int i = 0, s = 0; i < samps; i++)
					{
						left[i] = samples[s++];
						right[i] = samples[s++];
					}
					break;
				default:
					break;
			}
		}

		public void WriteToFile(String filename)
		{
			using (FileStream fs = new FileStream(filename, FileMode.Create))
			{
				BinaryWriter wr = new BinaryWriter(fs);
				wr.Write(header.chunkID);
				wr.Write(header.fileSize);
				wr.Write(header.riffType);
				wr.Write(header.fmtID);
				wr.Write(header.fmtSize);
				wr.Write((short) header.fmtCode);
				wr.Write((short) header.channels);
				wr.Write(header.sampleRate);
				wr.Write(header.byteRate);
				wr.Write((short) header.fmtBlockAlign);
				wr.Write((short) header.bitDepth);

				if (header.fmtSize == 18)
				{
					wr.Write((short) header.fmtExtraSize);
				}

				wr.Write(header.dataID);
				int bytesForSamp = header.bitDepth / 8;
                header.bytes = samples.Length * bytesForSamp;
				wr.Write(header.bytes);
				byte[] byteArray = new Byte[header.bytes];
				switch (header.bitDepth)
				{
					case 64:
						Buffer.BlockCopy(samples, 0, byteArray, 0, header.bytes);
						break;
					case 32:
						Buffer.BlockCopy(samples, 0, byteArray, 0, header.bytes);
						break;
					case 16:
						Int16[] asInt16 = new Int16[samples.Length];
						asInt16 = Array.ConvertAll(samples, e => (e < 0) ?
							(short)(-1 * e * Int16.MinValue) : (short)(e * Int16.MaxValue));
						Buffer.BlockCopy(asInt16, 0, byteArray, 0, header.bytes);
						break;
					case 8:
						Byte[] asBytes = new byte[header.bytes];
						asBytes = Array.ConvertAll(samples, e => ((e + SByte.MinValue) < 0) ?
						(byte)((-1 * e * SByte.MinValue) - SByte.MinValue) :
						(byte)((e * SByte.MaxValue) - SByte.MinValue));
						byteArray = asBytes;
						break;
					default:
						break;
				}
					for (int i = 0; i < byteArray.Length; i++)
				{
					wr.Write(byteArray[i]);
				}
			}
		}
	}
}
