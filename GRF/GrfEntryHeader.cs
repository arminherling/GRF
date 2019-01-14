namespace GRF
{
    public class GrfEntryHeader
    {
        public uint FileOffset { get; internal set; }
        public uint CompressedSize { get; internal set; }
        public uint CompressedSizeAligned { get; internal set; }
        public uint UncompressedSize { get; internal set; }
        public FileFlag Flags { get; internal set; }
    }
}
