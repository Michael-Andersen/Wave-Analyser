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
		private byte[] byteArr;
        private float[] clipboardL;
        private float[] clipboardR;
        private int clipStart;
        private int clipEnd;

        private AudioFile(AudioFileHeader header)
        {
            this.header = header;
            this.nyquistLimit = (int)header.sampleRate / 2;
            maxAmp = 1;
            minAmp = -1;
            this.random = new Random();
        }

        public AudioFile(byte[] byteArray, int channels, int bitDepth, int sampleRate)
        {
            header = new AudioFileHeader();
            this.maxAmp = 1;
            this.minAmp = -1;
            header.sampleRate = sampleRate;
            header.bitDepth = bitDepth;
            header.channels = channels;
            header.chunkID = 1179011410;
            header.riffType = 1163280727;
            header.fmtID = 544501094;
            header.fmtSize = 16;
            header.fmtCode = 1;
            header.byteRate = sampleRate * channels * bitDepth / 8;
            header.fmtBlockAlign = channels * bitDepth / 8;
            header.dataID = 1635017060;
            header.bytes = byteArray.Length;
            header.fileSize = header.bytes + 36;
            int bytesForSamp = bitDepth / 8;
            int samps = header.bytes / bytesForSamp;
            switch (bitDepth)
            {
                case 16:
                    header.signed = true;
                    Int16[] asInt16 = new Int16[samps];
                    Buffer.BlockCopy(byteArray, 0, asInt16, 0, header.bytes);
                    samples = Array.ConvertAll(asInt16, e => (e < 0) ?
                        -1 * e / (float)Int16.MinValue : e / (float)Int16.MaxValue);
                    break;
                case 8:
                    header.signed = false;
                    samples = Array.ConvertAll(byteArray, e => ((e + SByte.MinValue) < 0) ?
                    -1 * (e + SByte.MinValue) / (float)SByte.MinValue :
                    (e + SByte.MinValue) / (float)SByte.MaxValue);
                    break;
                default:
                    break;
            }

            DeMux();
        }

        public int SampleRate { get => header.sampleRate; }
        public int NyquistLimit { get => nyquistLimit; }
		public ref byte[] ByteArr { get => ref byteArr; }

		public void makeEcho()
		{
			float[] temp = new float[samples.Length];
		for (int i = 0; i < samples.Length; i++)
			{
				temp[i] = samples[i];
				if (i > header.sampleRate / 4.0)
				{
					temp[i] += samples[i - (header.sampleRate / 4)] * (float)0.5 + temp[i - header.sampleRate / 4] *(float)0.125;
				}
			
			}
			samples = temp;
			floatToBytes();
		}
		
		public double MaxAmp { get => maxAmp; }
		public double MinAmp { get => minAmp; }
		public bool Signed { get => header.signed; }
		public float[] Samples { get => samples; set => samples = value; }
		public float[] Left { get => left; set => left = value; }
		public float[] Right { get => right; set => right = value; }
		public float[] Selection { get => selection; }
		public bool LeftSelected { get => leftSelected; set => leftSelected = value; }
		public int Channels { get => header.channels; }
		public int BitDepth { get => header.bitDepth; }
		public int Bytes { get => header.bytes; }

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

		public void SetClipboard(int start, int end)
		{
			int j = 0;
			clipboardL = new float[end - start];
			clipboardR = new float[end - start];
			for (int i = start; i < end; i++)
			{
				clipboardL[j] = left[i];
				clipboardR[j] = right[i];
				j++;
			}
			clipStart = start;
			clipEnd = end;
		}

		public void Paste(int start)
		{
			int newEnd = clipboardL.Length + left.Length;
			float[] temp = new float[left.Length];
			Array.Copy(left, temp, left.Length);
			left = new float[newEnd];
			float[] temp2 = new float[right.Length];
			Array.Copy(right, temp2, right.Length);
			right = new float[newEnd];
			Array.Copy(temp2, right, start);
			Array.Copy(temp, left, start);
			Array.Copy(clipboardR, 0, right, start, clipboardR.Length);
			Array.Copy(clipboardL, 0, left, start, clipboardL.Length);
			Array.Copy(temp2, start, right, start + clipboardR.Length, temp2.Length - start);
			Array.Copy(temp, start, left, start + clipboardL.Length, temp.Length - start);
			Mux();
			int bytesForSamp = header.bitDepth / 8;
			header.bytes = samples.Length * bytesForSamp;
			header.fileSize = header.bytes + 36;
		}

		public void Cut(int start)
		{
			int newEnd =  left.Length - clipboardL.Length;
			float[] temp = new float[left.Length];
			Array.Copy(left, temp, left.Length);
			left = new float[newEnd];
			float[] temp2 = new float[right.Length];
			Array.Copy(right, temp2, right.Length);
			right = new float[newEnd];
			Array.Copy(temp2, right, start);
			Array.Copy(temp, left, start);
			Array.Copy(temp2, start + clipboardR.Length, right, start, temp2.Length - start - clipboardL.Length);
			Array.Copy(temp, start + clipboardL.Length, left, start, temp.Length - start - clipboardR.Length);
			Mux();
			int bytesForSamp = header.bitDepth / 8;
			header.bytes = samples.Length * bytesForSamp;
			header.fileSize = header.bytes + 36;
		}


		public void GenerateSineData(double[] freqs)
		{
			left = new float[(int)(left.Length)];
			for (int i = 0; i < left.Length; i++)
			{
				double time = i / (double)header.sampleRate;
				left[i] = 0;
				for (int j = 0; j < freqs.Length; j++)
				{
					float amp = (float)(maxAmp/(freqs.Length * 2) * Math.Sin(2 * Math.PI * freqs[j] * time));
					left[i] += (float)amp;
				}
			}
			right = left;

		}

		public void GenerateStabData(double[] freqs)
		{
			samples = new float[(int)(samples.Length)];
			bool silence = true;
			double decay = 1;
			for (int i = 0; i < samples.Length; i++)
			{
				double time = i / (double)header.sampleRate;
				samples[i] = 0;
				if (i % (header.sampleRate/2.0) == 0)
				{
					silence = !silence;
					decay = 1;
				}
				decay -= (1 / (header.sampleRate / 2.0));
				if (silence)
				{
					continue;
				}
				for (int j = 0; j < freqs.Length; j++)
				{
					int k = j % freqs.Length;
					if (decay == 0)
					{
						continue;
					}
					float amp = (float)(decay * maxAmp / (Math.Pow(2, j + 2)) * Math.Sin(2 * Math.PI * freqs[j] * time));
					samples[i] += (float)amp;
				}
			}

		}

		public void GenerateRandomData(double seconds)
		{
			samples = new float[(int)(header.sampleRate * seconds)];

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

		public void Mux()
		{
			switch (header.channels)
			{
				case 1:
					samples = left;
					break;
				case 2:
					samples = new float[left.Length * 2];
					for (int i = 0, s = 0; i < left.Length; i++)
					{
						samples[s] = left[i];
						s++;
						samples[s] = right[i];
						s++;
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
				floatToBytes();
				
				for (int i = 0; i < byteArr.Length; i++)
				{
					wr.Write(byteArr[i]);
				}
			}
		}

		public byte[] floatToBytes()
		{
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
			
			byteArr =  byteArray;
			return byteArray;
		}
	}

}
