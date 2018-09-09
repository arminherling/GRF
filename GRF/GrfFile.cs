using Ionic.Zlib;

namespace GRF
{
    public class GrfFile
    {
        private byte[] _data;

        public GrfFile( byte[] data, string name, int compressedSize, int uncompressedSize, FileFlag flags )
        {
            _data = data;
            Name = name;
            CompressedSize = compressedSize;
            CompressedSizeAligned = _data.Length;
            UncompressedSize = uncompressedSize;
            Flags = flags;
        }

        public string Name { get; }
        public int CompressedSize { get; }
        public int CompressedSizeAligned { get; }
        public int UncompressedSize { get; }
        public FileFlag Flags { get; }

        public byte[] GetUncompressedData()
        {
            var decodedData = DataEncryptionStandard.DecodeGrfFile( _data, Flags, CompressedSize );
            return ZlibStream.UncompressBuffer( _data );
        }
    }
}
