namespace GRF
{
    internal class GrfHeader
    {
        internal int Size => 46;
        internal int FileOffset { get; }
        internal int FileTablePosition => Size + FileOffset;
        internal int FileCount { get; }
        internal string Signature { get; }
        internal GrfFormat Version { get; }

        internal GrfHeader( GrfFormat version, string signature, int fileCount, int fileOffset )
        {
            Version = version;
            Signature = signature;
            FileCount = fileCount;
            FileOffset = fileOffset;
        }
    }
}
