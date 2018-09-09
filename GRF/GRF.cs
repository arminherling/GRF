using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace GRF
{
    public class Grf
    {
        static readonly int GrfHeaderSize = 46;

        public bool IsLoaded { get; private set; }
        public string Signature { get; private set; } = string.Empty;
        public Dictionary<string, GrfFile> Files { get; set; } = new Dictionary<string, GrfFile>();
        public int FileCount => Files.Count;
        public List<string> FileNames => Files.Keys.ToList();

        public void Load( string grfFilePath )
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var absolutePath = Path.Combine( baseDirectory, grfFilePath );
            if( !File.Exists( absolutePath ) )
                throw new FileNotFoundException( grfFilePath );

            Stream stream = new MemoryStream( File.ReadAllBytes( absolutePath ) );
            var streamReader = new BinaryReader( stream );

            var signatureBytes = streamReader.ReadBytes( 15 );
            Signature = Encoding.ASCII.GetString( signatureBytes );
            streamReader.ReadByte(); // string null terminator

            var encryptionKey = streamReader.ReadBytes( 14 );
            var fileTableOffset = streamReader.ReadInt32();
            var distortedFileCountSeed = streamReader.ReadInt32();
            var distortedFileCount = streamReader.ReadInt32();
            var version = streamReader.ReadInt32();

            stream.Seek( fileTableOffset, SeekOrigin.Current );

            var compressedBodySize = streamReader.ReadInt32();
            var bodySize = streamReader.ReadInt32();

            var compressedBody = streamReader.ReadBytes( compressedBodySize );
            var bodyData = ZlibStream.UncompressBuffer( compressedBody );

            var bodyStream = new MemoryStream( bodyData );
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

                var compressedFileSize = bodyReader.ReadInt32();
                var compressedFileSizeAligned = bodyReader.ReadInt32();
                var uncompressedFileSize = bodyReader.ReadInt32();
                var fileFlags = (FileFlag)bodyReader.ReadByte();
                var fileOffset = bodyReader.ReadInt32();

                // skip directories and files with zero size
                if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                    continue;

                stream.Seek( GrfHeaderSize + fileOffset, SeekOrigin.Begin );

                Files.Add( 
                    fileName,
                    new GrfFile(
                        streamReader.ReadBytes( compressedFileSizeAligned ),
                        fileName,
                        compressedFileSize,
                        uncompressedFileSize,
                        fileFlags ) );
            }
            stream.Close();
            bodyStream.Close();
            IsLoaded = true;
        }

        public void Unload()
        {
            Files.Clear();
            Signature = string.Empty;
            IsLoaded = false;
        }
    }
}
