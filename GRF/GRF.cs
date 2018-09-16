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

            for( int i = 0, fileEntryHeader = 0; i < fileCount; i++ )
            {
                bodyReader.BaseStream.Seek( fileEntryHeader, SeekOrigin.Begin );
                int nameLength = bodyReader.PeekChar() - 6;
                int fileEntryData = fileEntryHeader + bodyReader.ReadInt32() + 4;

                bodyReader.BaseStream.Seek( fileEntryHeader + 6, SeekOrigin.Begin );
                var encodedName = bodyReader.ReadBytes( nameLength );
                var fileName = DataEncryptionStandard.DecodeFileName( encodedName );

                //bodyReader.BaseStream.Seek( fileEntryData, SeekOrigin.Begin );
                //var compressedFileSizeBase = bodyReader.ReadInt32();
                //var compressedFileSizeAligned = bodyReader.ReadInt32() - 37579;
                //var uncompressedFileSize = bodyReader.ReadInt32();
                //var compressedFileSize = compressedFileSizeBase - uncompressedFileSize - 715;
                //var fileFlags = (FileFlag)bodyReader.ReadByte();
                //var fileDataOffset = bodyReader.ReadInt32() + GrfHeaderSize;

                fileEntryHeader = fileEntryData + 17;
                Files.Add( fileName, new GrfFile( new byte[] { }, fileName, 0, 0, 0 ) );
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
                var fileDataOffset = bodyReader.ReadInt32();

                // skip directories and files with zero size
                if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                    continue;

                streamReader.BaseStream.Seek( GrfHeaderSize + fileDataOffset, SeekOrigin.Begin );

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
