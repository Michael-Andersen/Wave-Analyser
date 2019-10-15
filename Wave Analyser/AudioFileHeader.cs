using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser
{
    struct AudioFileHeader
    {
        public int sampleRate;
        public int bitDepth;
        public bool signed;
        public int chunkID;
        public int fileSize;
        public int riffType;
        public int fmtID;
        public int fmtSize;
        public int fmtCode;
        public int channels;
        public int byteRate;
        public int fmtBlockAlign;
        public int fmtExtraSize;
        public int dataID;
        public int bytes;
    }
}
