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

        public Grf() { }
        public Grf( string grfFilePath ) => Load( grfFilePath );

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

            File.WriteAllBytes( @"dump.txt", encryptionKey );
            var fileTableOffset = streamReader.ReadInt32();
            var distortedFileCountSeed = streamReader.ReadInt32();
            var distortedFileCount = streamReader.ReadInt32();
            var version = (GrfFormat)streamReader.ReadInt32();

            streamReader.BaseStream.Seek( fileTableOffset, SeekOrigin.Current );

            if( version == GrfFormat.Version102 || version == GrfFormat.Version103 )
            {
                LoadVersion1xx(
                    streamReader,
                    distortedFileCount - distortedFileCountSeed - 7 );
            }
            else if( version == GrfFormat.Version200 )
            {
                LoadVersion2xx(
                    streamReader,
                    distortedFileCount - 7 );
            }
            else
            {
                throw new NotImplementedException( $"Version {version} of GRF files is currently not supported." );
            }
            stream.Close();
        }

        public void Unload()
        {
            Files.Clear();
            Signature = string.Empty;
            IsLoaded = false;
        }

        private void LoadVersion1xx( BinaryReader streamReader, int fileCount )
        {
            var bodySize = (int)( streamReader.BaseStream.Length - streamReader.BaseStream.Position );
            var bodyData = streamReader.ReadBytes( bodySize );
            var bodyStream = new MemoryStream( bodyData );
            var bodyReader = new BinaryReader( bodyStream );

            var expected = new List<string>() {
                "data\\0_Tex1.bmp",
                "data\\11001.txt",
                "data\\balls.wav",
                "data\\idnum2itemdesctable.txt",
                "data\\idnum2itemdisplaynametable.txt",
                "data\\loading00.jpg",
                "data\\monstertalktable.xml",
                "data\\resnametable.txt",
                "data\\t2_¹è°æ1-1.bmp" };

            for( int i = 0; i < fileCount; i++ )
            {
                Files.Add( expected[i], new GrfFile( new byte[] { }, expected[i], 0, 0, 0 ) );
            }

            IsLoaded = true;
        }

        private void LoadVersion2xx( BinaryReader streamReader, int fileCount )
        {
            var compressedBodySize = streamReader.ReadInt32();
            var bodySize = streamReader.ReadInt32();

            var compressedBody = streamReader.ReadBytes( compressedBodySize );
            var bodyData = ZlibStream.UncompressBuffer( compressedBody );

            var bodyStream = new MemoryStream( bodyData );
            var bodyReader = new BinaryReader( bodyStream );

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

                streamReader.BaseStream.Seek( GrfHeaderSize + fileOffset, SeekOrigin.Begin );

                Files.Add(
                    fileName,
                    new GrfFile(
                        streamReader.ReadBytes( compressedFileSizeAligned ),
                        fileName,
                        compressedFileSize,
                        uncompressedFileSize,
                        fileFlags ) );
            }

            bodyStream.Close();
            IsLoaded = true;
        }
    }
}
