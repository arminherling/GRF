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

        internal static Dictionary<string, GrfEntry> ReadEntries( string grfFilePath, GrfHeader header, LoadingMode loadingMode )
        {
            switch( header.Version )
            {
                case GrfFormat.Version102:
                case GrfFormat.Version103:
                    return Grf1xxFileReader.ReadEntries( grfFilePath, header, loadingMode );
                case GrfFormat.Version200:
                    return Grf2xxFileReader.ReadEntries( grfFilePath, header, loadingMode );
                default:
                    throw new NotImplementedException( $"Version {header.Version} of GRF files is currently not supported." );
            }
        }
    }
}
