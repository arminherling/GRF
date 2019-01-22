using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace GRF
{
    internal class GrfFileReader
    {
        internal static GrfHeader ReadHeader( string grfFilePath )
        {
            using( var fileStream = File.OpenRead( grfFilePath ) )
            using( var fileReader = new BinaryReader( fileStream ) )
            {
                var signature = Encoding.ASCII.GetString( fileReader.ReadBytes( 16 ), 0, 15 );
                var encryptKey = Encoding.ASCII.GetString( fileReader.ReadBytes( 14 ) );
                var fileOffset = fileReader.ReadInt32();
                var seed = fileReader.ReadInt32();
                var distortedFileCount = fileReader.ReadInt32();
                var version = (GrfFormat)fileReader.ReadInt32();
                int fileCount;

                switch( version )
                {
                    case GrfFormat.Version102:
                    case GrfFormat.Version103:
                        fileCount = distortedFileCount - seed - 7;
                        break;
                    case GrfFormat.Version200:
                        fileCount = distortedFileCount - 7;
                        break;
                    default:
                        throw new NotImplementedException( $"Version {version} of GRF files is currently not supported." );
                }

                return new GrfHeader( version, signature, fileCount, fileOffset );
            }
        }

        internal static List<string> ReadFileNames( string grfFilePath, GrfHeader header )
        {
            switch( header.Version)
            {
                case GrfFormat.Version102:
                case GrfFormat.Version103:
                    return ReadVersion1xxFileNames( grfFilePath, header );
                case GrfFormat.Version200:
                    return ReadVersion2xxFileNames( grfFilePath, header );
                default:
                    throw new NotImplementedException( $"Version {header.Version} of GRF files is currently not supported." );
            }
        }

        private static List<string> ReadVersion1xxFileNames( string grfFilePath, GrfHeader header )
        {
            var fileNames = new List<string>();

            using( var fileStream = File.OpenRead( grfFilePath ) )
            using( var binaryReader = new BinaryReader( fileStream ) )
            {
                for( int i = 0, fileEntryHeader = 0; i < header.FileCount; i++ )
                {
                    fileStream.Seek( header.FileTablePosition + fileEntryHeader, SeekOrigin.Begin );
                    int nameLength = binaryReader.PeekChar() - 6;
                    int fileEntryData = fileEntryHeader + binaryReader.ReadInt32() + 4;

                    binaryReader.ReadBytes( 2 );
                    var encodedName = binaryReader.ReadBytes( nameLength );
                    var fileName = DecodeFileName( encodedName.AsSpan() );
                    fileNames.Add( fileName );

                    fileEntryHeader = fileEntryData + 17;
                }
            }

            return fileNames;
        }

        private static List<string> ReadVersion2xxFileNames( string grfFilePath, GrfHeader header )
        {
            var fileNames = new List<string>();
            using( var fileStream = File.OpenRead( grfFilePath ) )
            {
                // 4 byte for compressed body size
                // 4 bytes for uncompressed body size 
                fileStream.Seek( header.FileTablePosition + 8, SeekOrigin.Begin );

                using( var bodyStream = new ZlibStream( fileStream, CompressionMode.Decompress ) )
                using( var bodyReader = new BinaryReader( bodyStream ) )
                {
                    for( int i = 0; i < header.FileCount; i++ )
                    {
                        var fileName = string.Empty;
                        char currentChar;
                        while( ( currentChar = (char)bodyReader.ReadByte() ) != 0 )
                        {
                            fileName += currentChar;
                        }
                        fileNames.Add( fileName );

                        // 4 byte compressed size
                        // 4 byte compressed size aligned
                        // 4 byte uncompressed size
                        // 1 byte flags
                        // 4 byte data offset
                        bodyReader.ReadBytes( 17 );
                    }
                }
            }
            return fileNames;
        }

        internal static string DecodeFileName( Span<byte> encodedName )
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
    }
}
