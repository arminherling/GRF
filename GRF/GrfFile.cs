namespace GRF
{
    public class GrfFile
    {
        private byte[] _data;

        public GrfFile( byte[] data, string name, int uncompressedSize, FileFlag flags )
        {
            _data = data;
            Name = name;
            CompressedSize = data.Length;
            UncompressedSize = uncompressedSize;
            Flags = flags;
        }

        public string Name { get; }
        public int CompressedSize { get; }
        public int UncompressedSize { get; }
        public FileFlag Flags { get; }
    }
}
