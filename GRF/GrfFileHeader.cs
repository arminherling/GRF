using System;
using System.Collections.Generic;
using System.Text;

namespace GRF
{
    public class GrfFileHeader
    {
        public byte[] signature { get; internal set; }
        public byte[] encryptKey { get; internal set; }
        public uint fileOffset { get; internal set; }
        public uint seed { get; internal set; }
        public uint fileCount { get; internal set; }
        public GrfFormat version { get; internal set; }
    }
}
