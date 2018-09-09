using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace GRF
{
    public class GRF
    {
        private Stream _stream;

        public bool IsOpen { get; private set; }
        public string Signature { get; private set; } = string.Empty;
        private Dictionary<string, object> Files { get; set; } = new Dictionary<string, object>();
        public int FileCount => Files.Count;
        public List<string> FileNames => Files.Keys.ToList();

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
            streamReader.ReadByte(); // string null terminator

            var encryptionKey = streamReader.ReadBytes( 14 );
            var fileTableOffset = streamReader.ReadInt32();
            var distortedFileCountSeed = streamReader.ReadInt32();
            var distortedFileCount = streamReader.ReadInt32();
            var version = streamReader.ReadInt32();

            _stream.Seek( fileTableOffset, SeekOrigin.Current );

            var compressedLength = streamReader.ReadInt32();
            var uncompressedLength = streamReader.ReadInt32();

            var compressedBodyBytes = streamReader.ReadBytes( compressedLength );
            var bodyBytes = ZlibStream.UncompressBuffer( compressedBodyBytes );

            var bodyStream = new MemoryStream( bodyBytes );
            var bodyReader = new BinaryReader( bodyStream );

            var fileCount = distortedFileCount - distortedFileCountSeed - 7;
            for( int i = 0; i < fileCount; i++ )
            {
                var fileName = string.Empty;
                char currentChar;
                while( ( currentChar = (char)bodyReader.ReadByte() ) != 0 )
                {
                    fileName += currentChar;
                }

                var fileCompressedLength = bodyReader.ReadInt32();
                var fileCompressedLengthAligned = bodyReader.ReadInt32();
                var fileUncompressedLength = bodyReader.ReadInt32();
                var fileFlags = bodyReader.ReadByte();
                var fileOffset = bodyReader.ReadInt32();

                if( fileFlags == 2 ) // folder
                    continue;

                Files.Add( fileName, null );
            }

            IsOpen = true;
        }

        public void Close()
        {
            if( IsOpen )
            {
                _stream.Close();
                Signature = string.Empty;
                Files.Clear();
            }

            IsOpen = false;
        }
    }
}
