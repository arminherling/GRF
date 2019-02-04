using Ionic.Zlib;
using System.Collections.Generic;
using System.IO;

namespace GRF
{
    internal class Grf2xxFileReader
    {
        internal static Dictionary<string, GrfEntry> ReadEntries( string grfFilePath, GrfHeader header, LoadingMode loadingMode )
        {
            var entries = new Dictionary<string, GrfEntry>();

            using( var fileStream = File.OpenRead( grfFilePath ) )
            using( var bodyStream = new ZlibStream( fileStream, CompressionMode.Decompress ) )
            using( var bodyReader = new BinaryReader( bodyStream ) )
            {
                // skip 4 byte compressed body size
                // skip 4 byte uncompressed body size
                fileStream.Seek( header.FileTablePosition + 8, SeekOrigin.Begin );

                for( int i = 0; i < header.FileCount; i++ )
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

                    entries.Add(
                        fileName,
                        new GrfEntry(
                            fileName,
                            (uint)header.Size + fileDataOffset,
                            compressedFileSize,
                            compressedFileSizeAligned,
                            uncompressedFileSize,
                            fileFlags,
                            null ) );
                }
            }

            return entries;
        }
        
        //private static Dictionary<string, GrfEntry> ReadVersion2xxFileNames( string grfFilePath, GrfHeader header )
        //{
        //    var fileNames = new Dictionary<string, GrfEntry>();
        //    using( var fileStream = File.OpenRead( grfFilePath ) )
        //    {
        //        // 4 byte for compressed body size
        //        // 4 bytes for uncompressed body size 
        //        fileStream.Seek( header.FileTablePosition + 8, SeekOrigin.Begin );

        //        using( var bodyStream = new ZlibStream( fileStream, CompressionMode.Decompress ) )
        //        using( var bodyReader = new BinaryReader( bodyStream ) )
        //        {
        //            for( int i = 0; i < header.FileCount; i++ )
        //            {
        //                var fileName = string.Empty;
        //                char currentChar;
        //                while( ( currentChar = (char)bodyReader.ReadByte() ) != 0 )
        //                {
        //                    fileName += currentChar;
        //                }
        //                fileNames.Add( fileName, null );

        //                // 4 byte compressed size
        //                // 4 byte compressed size aligned
        //                // 4 byte uncompressed size
        //                // 1 byte flags
        //                // 4 byte data offset
        //                bodyReader.ReadBytes( 17 );
        //            }
        //        }
        //    }
        //    return fileNames;
        //}
    }
}
