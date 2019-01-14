namespace GRF
{
    public class GrfHeader
    {
        public GrfFormat Version { get; internal set; }
        public string Signature { get; internal set; } = string.Empty;
        public string EncryptKey { get; internal set; }
        public uint Seed { get; internal set; }
        public uint FileCount { get; internal set; }
        public uint FileOffset { get; internal set; }
        public uint Size => 46;
    }
}
