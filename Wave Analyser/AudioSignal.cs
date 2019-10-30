using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser.Classes
{
	public class AudioSignal
	{
		private int sampleRate;
		private int bitDepth;
		private double maxAmp;
		private double minAmp;
		private bool signed;
		private float[] samples;
		private float[] selection;
		private Random random;
		private float[] right;
		private float[] left;
		private int chunkID;
		private int fileSize;
		private int riffType;
		private int fmtID;
		private int fmtSize;
		private int fmtCode;
		private int channels;
		private int byteRate;
		private int fmtBlockAlign;
		private int fmtExtraSize;
		private int dataID;
		private int bytes;
		private bool leftSelected;
		private float[] clipboardL;
		private float[] clipboardR;
		private int clipStart;
		private int clipEnd;

		public AudioSignal(int sampleRate, int bitDepth, bool signed)
		{
			this.sampleRate = sampleRate;
			this.bitDepth = bitDepth;
			this.signed = signed;
			this.maxAmp = 1;
			this.minAmp = -1;
			this.random = new Random();
		}

		public void makeEcho()
		{
			float[] temp = new float[samples.Length];
		for (int i = 0; i < samples.Length; i++)
			{
				temp[i] = samples[i];
				if (i > sampleRate/2)
				{
					temp[i] += samples[i - sampleRate / 2] * (float)0.5 + temp[i-sampleRate/2] *(float)0.125;
				}
			
			}
			samples = temp;
		}
		
		public AudioSignal(byte[] byteArray, int channels, int bitDepth, int sampleRate)
		{
			this.maxAmp = 1;
			this.minAmp = -1;
			this.sampleRate = sampleRate;
			this.bitDepth = bitDepth;
			this.channels = channels;
			this.chunkID = 1179011410;
			this.riffType = 1163280727;
			this.fmtID = 544501094;
			this.fmtSize = 16;
			this.fmtCode = 1;
			this.byteRate = sampleRate * channels * bitDepth / 8;
			this.fmtBlockAlign = channels * bitDepth / 8;
			this.dataID = 1635017060;
			bytes = byteArray.Length;
			fileSize = bytes + 36;
			int bytesForSamp = bitDepth / 8;
			int samps = bytes / bytesForSamp;
			switch (bitDepth)
			{	
				case 16:
					signed = true;
					Int16[] asInt16 = new Int16[samps];
					Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
					samples = Array.ConvertAll(asInt16, e => (e < 0) ?
						-1 * e / (float)Int16.MinValue : e / (float)Int16.MaxValue);
					break;
				case 8:
					signed = false;
					samples = Array.ConvertAll(byteArray, e => ((e + SByte.MinValue) < 0) ?
					-1 * (e + SByte.MinValue) / (float)SByte.MinValue :
					(e + SByte.MinValue) / (float)SByte.MaxValue);
					break;
				default:
					break;
			}
			
			DeMux();
		}

		public double SampleRate { get => sampleRate; }
		public double MaxAmp { get => maxAmp; }
		public double MinAmp { get => minAmp; }
		public bool Signed { get => signed; }
		public float[] Samples { get => samples; }
		public float[] Left { get => left; }
		public float[] Right { get => right; }
		public float[] Selection { get => selection; }
		public bool LeftSelected { get => leftSelected; set => leftSelected = value; }
		public int Channels { get => channels; }
		public int BitDepth { get => bitDepth; }
		public int Bytes { get => bytes; }

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
			int bytesForSamp = bitDepth / 8;
			bytes = samples.Length * bytesForSamp;
			fileSize = bytes + 36;
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
			int bytesForSamp = bitDepth / 8;
			bytes = samples.Length * bytesForSamp;
			fileSize = bytes + 36;
		}


		public void GenerateSineData(double seconds, int[] freqs)
		{
			samples = new float[(int)(sampleRate * seconds)];

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
			samples = new float[(int)(sampleRate * seconds)];

			for (int i = 0; i < samples.Length; i++)
			{
				samples[i] = random.Next((int)minAmp, (int)maxAmp + 1);
			}
		}

		public void readFromFile(String filename)
		{
			try
			{
				using (FileStream fs = File.Open(filename, FileMode.Open))
				{
					BinaryReader reader = new BinaryReader(fs);

					chunkID = reader.ReadInt32();
					fileSize = reader.ReadInt32();
					riffType = reader.ReadInt32();
					fmtID = reader.ReadInt32();
					fmtSize = reader.ReadInt32();
					fmtCode = reader.ReadInt16();
					channels = reader.ReadInt16();
					sampleRate = reader.ReadInt32();
					byteRate = reader.ReadInt32();
					fmtBlockAlign = reader.ReadInt16();
					bitDepth = reader.ReadInt16();

					if (fmtSize == 18)
					{
						// Read any extra values
						fmtExtraSize = reader.ReadInt16();
						reader.ReadBytes(fmtExtraSize);
					}

					// chunk 2
					dataID = reader.ReadInt32();
					bytes = reader.ReadInt32();

					// DATA!
					byte[] byteArray = reader.ReadBytes(bytes);

					int bytesForSamp = bitDepth / 8;
					int samps = bytes / bytesForSamp;
					float[] asFloat = null;
					switch (bitDepth)
					{
						case 64:
							signed = true;
							double[] asDouble = new double[samps];
							Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
							asFloat = Array.ConvertAll(asDouble, e => (float)e);
							break;
						case 32:
							signed = true;
							asFloat = new float[samps];
							Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
							break;
						case 16:
							signed = true;
							Int16[] asInt16 = new Int16[samps];
							Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
							asFloat = Array.ConvertAll(asInt16, e => (e < 0) ?
								-1 * e / (float)Int16.MinValue : e / (float)Int16.MaxValue);
							break;
						case 8:
							signed = false;
							asFloat = Array.ConvertAll(byteArray, e => ((e + SByte.MinValue) < 0) ?
							-1 * (e + SByte.MinValue) / (float)SByte.MinValue :
							(e + SByte.MinValue) / (float)SByte.MaxValue);
							break;
						default:
							break;
					}
					samples = asFloat;
					DeMux();
				}
			}
			catch
			{
			}
		}
		public void DeMux()
		{
			switch (channels)
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
			switch (channels)
			{
				case 1:
					samples = left;
					break;
				case 2:
					/*int samps = samples.Length / 2;
					if (left.Length > right.Length)
					{
						float[] temp = new float[right.Length];
						Array.Copy(right, temp, right.Length);
						right = new float[left.Length];
						Array.Copy(temp, right, temp.Length);
					}
					else if (right.Length > left.Length)
					{
						float[] temp = new float[left.Length];
						Array.Copy(left, temp, left.Length);
						left = new float[right.Length];
						Array.Copy(temp, left, temp.Length);

					}*/
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
				wr.Write(chunkID);
				wr.Write(fileSize);
				wr.Write(riffType);
				wr.Write(fmtID);
				wr.Write(fmtSize);
				wr.Write((short)fmtCode);
				wr.Write((short)channels);
				wr.Write(sampleRate);
				wr.Write(byteRate);
				wr.Write((short)fmtBlockAlign);
				wr.Write((short)bitDepth);
				if (fmtSize == 18)
				{
					wr.Write((short)fmtExtraSize);
				}
				wr.Write(dataID);
				int bytesForSamp = bitDepth / 8;
				bytes = samples.Length * bytesForSamp;
				wr.Write(bytes);
				byte[] byteArray = floatToBytes();
				
				for (int i = 0; i < byteArray.Length; i++)
				{
					wr.Write(byteArray[i]);
				}

				
			}
		}
		public Byte[] floatToBytes()
		{
			byte[] byteArray = new Byte[bytes];
			switch (bitDepth)
			{
				case 64:
					Buffer.BlockCopy(samples, 0, byteArray, 0, bytes);
					break;
				case 32:
					Buffer.BlockCopy(samples, 0, byteArray, 0, bytes);
					break;
				case 16:
					Int16[] asInt16 = new Int16[samples.Length];
					asInt16 = Array.ConvertAll(samples, e => (e < 0) ?
						(short)(-1 * e * Int16.MinValue) : (short)(e * Int16.MaxValue));
					Buffer.BlockCopy(asInt16, 0, byteArray, 0, bytes);
					break;
				case 8:
					Byte[] asBytes = new byte[bytes];
					asBytes = Array.ConvertAll(samples, e => ((e + SByte.MinValue) < 0) ?
					(byte)((-1 * e * SByte.MinValue) - SByte.MinValue) :
					(byte)((e * SByte.MaxValue) - SByte.MinValue));
					byteArray = asBytes;
					break;
				default:
					break;
			}
			return byteArray;
		}
	}

}
