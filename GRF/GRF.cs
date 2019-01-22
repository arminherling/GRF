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

        private List<GrfEntry> Entries { get; set; } = new List<GrfEntry>();
        public int Count => Entries.Count;
        public List<string> EntryNames { get; private set; } = new List<string>();

        public bool IsLoaded { get; private set; }

        private string _filePath;

        public Grf() { }
        public Grf( string grfFilePath ) => Load( grfFilePath );

        public void Load( string grfFilePath, LoadingMode loadingMode = LoadingMode.Deferred )
        {
            _filePath = grfFilePath;
            Header = GrfFileReader.ReadHeader( grfFilePath );

            if( loadingMode == LoadingMode.Deferred )
            {
                EntryNames = GrfFileReader.ReadFileNames( grfFilePath, Header );
            }
            //else if( loadingMode == LoadingMode.Immediate )
            {
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
                //EntryNames = Entries.ConvertAll( x => x.Path );
            }

            if( loadingMode == LoadingMode.Immediate )
                EntryNames = Entries.ConvertAll( x => x.Path );
        }

        public void Unload()
        {
            Header = null;
            Entries.Clear();
            EntryNames.Clear();
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
            for( int i = 0, fileEntryHeader = 0; i < fileCount; i++ )
            {
                streamReader.BaseStream.Seek( Header.FileTablePosition + fileEntryHeader, SeekOrigin.Begin );
                int nameLength = streamReader.PeekChar() - 6;
                int fileEntryData = fileEntryHeader + streamReader.ReadInt32() + 4;

                streamReader.ReadBytes( 2 );
                var encodedName = streamReader.ReadBytes( nameLength );
                var fileName = GrfFileReader.DecodeFileName( encodedName.AsSpan() );

                streamReader.BaseStream.Seek( Header.FileTablePosition + fileEntryData, SeekOrigin.Begin );
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
