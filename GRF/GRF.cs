using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zlib;

namespace GRF
{
    public class Grf
    {
        private GrfHeader Header { get; set; }
        public string Signature => Header?.Signature ?? string.Empty;

        public List<GrfEntry> Entries { get; private set; } = new List<GrfEntry>();
        public int Count => Entries.Count;
        public List<string> EntryNames => Entries.ConvertAll( f => f.Path );

        public bool IsLoaded { get; private set; }

        private string _filePath;

        public Grf() { }
        public Grf( string grfFilePath ) => Load( grfFilePath );

        public void Load( string grfFilePath )
        {
            _filePath = grfFilePath;
            Header = GrfFileReader.ReadHeader( grfFilePath );

            using( var fileStream = File.OpenRead( grfFilePath ) )
            using( var binaryReader = new BinaryReader( fileStream ) )
            {
                binaryReader.BaseStream.Seek( Header.FileTablePosition, SeekOrigin.Begin );

                if( Header.Version == GrfFormat.Version102 || Header.Version == GrfFormat.Version103 )
                {
                    LoadVersion1xx(
                        binaryReader,
                        (uint)Header.FileCount );
                }
                else if( Header.Version == GrfFormat.Version200 )
                {
                    LoadVersion2xx(
                        binaryReader,
                        (uint)Header.FileCount );
                }

                IsLoaded = true;
            }
        }

        public void Unload()
        {
            Header = null;
            Entries.Clear();
            _filePath = string.Empty;
            IsLoaded = false;
        }

        public bool Find( string entryName, out GrfEntry entry )
        {
            int hashCode = entryName.GetHashCode();
            entry = Entries.FirstOrDefault( x => x.GetHashCode() == hashCode );

            return !( entry is null );
        }

        private void LoadVersion1xx( BinaryReader streamReader, uint fileCount )
        {
            var fileTableOffset = (int)( Header.Size + Header.FileOffset );
            for( int i = 0, fileEntryHeader = 0; i < fileCount; i++ )
            {
                streamReader.BaseStream.Seek( fileTableOffset + fileEntryHeader, SeekOrigin.Begin );
                int nameLength = streamReader.PeekChar() - 6;
                int fileEntryData = fileEntryHeader + streamReader.ReadInt32() + 4;

                streamReader.BaseStream.Seek( fileTableOffset + fileEntryHeader + 6, SeekOrigin.Begin );
                var encodedName = streamReader.ReadBytes( nameLength );
                var fileName = DecodeFileName( encodedName.AsSpan() );

                streamReader.BaseStream.Seek( fileTableOffset + fileEntryData, SeekOrigin.Begin );
                uint compressedFileSizeBase = streamReader.ReadUInt32();
                uint compressedFileSizeAligned = streamReader.ReadUInt32() - 37579;
                uint uncompressedFileSize = streamReader.ReadUInt32();
                uint compressedFileSize = compressedFileSizeBase - uncompressedFileSize - 715;
                var fileFlags = (FileFlag)streamReader.ReadByte();
                fileFlags |= IsFullEncrypted( fileName )
                    ? FileFlag.Mixed
                    : FileFlag.DES;
                uint fileDataOffset = streamReader.ReadUInt32() + (uint)Header.Size;

                // skip directories and files with zero size
                if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                    continue;

                Entries.Add(
                    new GrfEntry(
                        fileName,
                        fileDataOffset,
                        compressedFileSize,
                        compressedFileSizeAligned,
                        uncompressedFileSize,
                        fileFlags,
                        this ) );

                fileEntryHeader = fileEntryData + 17;
            }
        }

        private void LoadVersion2xx( BinaryReader streamReader, uint fileCount )
        {
            var compressedBodySize = streamReader.ReadUInt32();
            var bodySize = streamReader.ReadUInt32();

            using( var bodyStream = new ZlibStream( streamReader.BaseStream, CompressionMode.Decompress ) )
            using( var bodyReader = new BinaryReader( bodyStream ) )
            {
                for( int i = 0; i < fileCount; i++ )
                {
                    var fileName = string.Empty;
                    char currentChar;
                    while( ( currentChar = (char)bodyReader.ReadByte() ) != 0 )
                    {
                        fileName += currentChar;
                    }

                    var compressedFileSize = bodyReader.ReadUInt32();
                    var compressedFileSizeAligned = bodyReader.ReadUInt32();
                    var uncompressedFileSize = bodyReader.ReadUInt32();
                    var fileFlags = (FileFlag)bodyReader.ReadByte();
                    var fileDataOffset = bodyReader.ReadUInt32();

                    // skip directories and files with zero size
                    if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                        continue;

                    Entries.Add(
                        new GrfEntry(
                            fileName,
                            (uint)Header.Size + fileDataOffset,
                            compressedFileSize,
                            compressedFileSizeAligned,
                            uncompressedFileSize,
                            fileFlags,
                            this ) );
                }
            }
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

            var fileName = string.Empty;
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
            var extensions = new string[] { ".gnd", ".gat", ".act", ".str" };
            foreach( var extension in extensions )
            {
                if( fileName.EndsWith( extension ) )
                    return false;
            }

            return true;
        }

        internal byte[] GetCompressedBytes( uint offset, uint lenght )
        {
            using( var stream = new FileStream( _filePath, FileMode.Open ) )
            using( var binaryReader = new BinaryReader( stream ) )
            {
                binaryReader.BaseStream.Seek( offset, SeekOrigin.Begin );
                return binaryReader.ReadBytes( (int)lenght );
            }
        }
    }
}
