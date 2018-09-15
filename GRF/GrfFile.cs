using Ionic.Zlib;
using System.IO;

namespace GRF
{
    public class GrfFile
    {
        private byte[] _data;

        public GrfFile( byte[] data, string filePath, int compressedSize, int uncompressedSize, FileFlag flags )
        {
            _data = data;
            FilePath = filePath;
            FileName = Path.GetFileName( filePath.Replace( "\\", "/" ) );
            FileType = Path.GetExtension( filePath ).TrimStart( '.' );
            CompressedSize = compressedSize;
            CompressedSizeAligned = _data.Length;
            UncompressedSize = uncompressedSize;
            Flags = flags;
        }

        public string FilePath { get; }
        public string FileName { get; }
        public string FileType { get; }
        public int CompressedSize { get; }
        public int CompressedSizeAligned { get; }
        public int UncompressedSize { get; }
        public FileFlag Flags { get; }

        public byte[] GetUncompressedData()
        {
            var decodedData = DataEncryptionStandard.DecodeGrfFile( _data, Flags, CompressedSize );
            return ZlibStream.UncompressBuffer( decodedData );
        }
    }
}
