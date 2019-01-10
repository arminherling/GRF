using System;
using System.Collections.Generic;
using System.Text;

namespace GRF
{
    public class GrfEntryHeader
    {
        public uint fileOffset { get; internal set; }
        public uint compressedSize { get; internal set; }
        public uint compressedSizeAligned { get; internal set; }
        public uint uncompressedSize { get; internal set; }
        public FileFlag flags { get; internal set; }
    }
}
