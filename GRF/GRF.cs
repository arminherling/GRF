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
        public Dictionary<string, GrfEntry> Entries { get; set; } = new Dictionary<string, GrfEntry>();
        public int EntryCount => Entries.Count;
        public List<string> EntryNames => Entries.Keys.ToList();
        public string FilePath { get; private set; }

        public Grf() { }
        public Grf( string grfFilePath ) => Load( grfFilePath );

        public void Load( string grfFilePath )
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            FilePath = Path.Combine( baseDirectory, grfFilePath );
            if( !File.Exists( FilePath ) )
                throw new FileNotFoundException( grfFilePath );

            Stream stream = new MemoryStream( File.ReadAllBytes( FilePath ) );
            var streamReader = new BinaryReader( stream );

            var signatureBytes = streamReader.ReadBytes( 15 );
            Signature = Encoding.ASCII.GetString( signatureBytes );
            streamReader.ReadByte(); // string null terminator

            var encryptionKey = streamReader.ReadBytes( 14 );
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
            streamReader.Close();
            IsLoaded = true;
        }

        public void Unload()
        {
            Entries.Clear();
            FilePath = string.Empty;
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
                var fileName = DecodeFileName( encodedName.AsSpan() );

                bodyReader.BaseStream.Seek( fileEntryData, SeekOrigin.Begin );
                var compressedFileSizeBase = bodyReader.ReadInt32();
                var compressedFileSizeAligned = bodyReader.ReadInt32() - 37579;
                var uncompressedFileSize = bodyReader.ReadInt32();
                var compressedFileSize = compressedFileSizeBase - uncompressedFileSize - 715;
                var fileFlags = (FileFlag)bodyReader.ReadByte();
                fileFlags |= IsFullEncrypted( fileName )
                    ? FileFlag.Mixed
                    : FileFlag.DES;
                var fileDataOffset = bodyReader.ReadInt32() + GrfHeaderSize;

                // skip directories and files with zero size
                if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                    continue;

                streamReader.BaseStream.Seek( fileDataOffset, SeekOrigin.Begin );

                Entries.Add(
                    fileName,
                    new GrfEntry(
                        streamReader.ReadBytes( compressedFileSizeAligned ),
                        fileName,
                        compressedFileSize,
                        uncompressedFileSize,
                        fileFlags ) );

                fileEntryHeader = fileEntryData + 17;
            }
            bodyReader.Close();
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

                Entries.Add(
                    fileName,
                    new GrfEntry(
                        streamReader.ReadBytes( compressedFileSizeAligned ),
                        fileName,
                        compressedFileSize,
                        uncompressedFileSize,
                        fileFlags ) );
            }

            bodyReader.Close();
        }

        private string DecodeFileName( Span<byte> encodedName )
        {
            for( int i = 0; i < encodedName.Length; i++ )
            {
                // swap nibbles
                encodedName[i] = (byte)( ( encodedName[i] & 0x0F ) << 4 | ( encodedName[i] & 0xF0 ) >> 4 );
            }
            for( int i = 0; i < encodedName.Length / DataEncryptionStandard.BlockSize; i++ )
            {
                DataEncryptionStandard.DecryptBlock( encodedName.Slice(
                    i * DataEncryptionStandard.BlockSize,
                    DataEncryptionStandard.BlockSize ) );
            }

            string fileName = string.Empty;
            for( int i = 0; i < encodedName.Length; i++ )
            {
                if( (char)encodedName[i] == 0 )
                    break;

                fileName += (char)encodedName[i];
            }

            return fileName;
        }

        private bool IsFullEncrypted( string fileName )
        {
            string[] extensions = { ".gnd", ".gat", ".act", ".str" };
            foreach( var extension in extensions )
            {
                if( fileName.EndsWith( extension ) )
                    return false;
            }
            return true;
        }
    }
}
