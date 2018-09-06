using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GRF
{
    public class GRF
    {
        private Stream _stream;
        private byte[] _encryptionKey;
        private int _fileTableOffset;
        private int _m1;
        private int _m2;
        private int _version;
        private int _compressedLength;
        private int _uncompressedLength;

        public int FileCount { get; private set; } = 0;
        public bool IsOpen { get; private set; } = false;
        public List<string> FileNames { get; } = new List<string>();
        public string Signature { get; private set; } = string.Empty;

        public void Open( string filePath )
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var absolutePath = Path.Combine( baseDirectory, filePath );
            if( !File.Exists( absolutePath ) )
                throw new FileNotFoundException( filePath );

            _stream = new MemoryStream( File.ReadAllBytes( absolutePath ) );
            var streamReader = new BinaryReader( _stream );

            var signatureBytes = streamReader.ReadBytes( 15 );
            Signature = Encoding.ASCII.GetString( signatureBytes );
            streamReader.ReadByte(); // advance position

            _encryptionKey = streamReader.ReadBytes( 14 );
            _fileTableOffset = streamReader.ReadInt32();
            _m1 = streamReader.ReadInt32();
            _m2 = streamReader.ReadInt32();
            _version = streamReader.ReadInt32();
            FileCount = _m2 - _m1 - 7;

            _stream.Seek( _fileTableOffset, SeekOrigin.Current );

            _compressedLength = streamReader.ReadInt32();
            _uncompressedLength = streamReader.ReadInt32();






            IsOpen = true;
        }

        public void Close()
        {
            if( IsOpen )
            {
                _stream.Close();
                Signature = string.Empty;
                FileCount = 0;
            }

            IsOpen = false;
        }
    }
}
