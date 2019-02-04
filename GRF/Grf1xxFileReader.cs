using System;
using System.Collections.Generic;
using System.IO;

namespace GRF
{
    internal class Grf1xxFileReader
    {
        internal static Dictionary<string, GrfEntry> ReadEntries( string grfFilePath, GrfHeader header, LoadingMode loadingMode )
        {
            var entries = new Dictionary<string, GrfEntry>();

            using( var fileStream = File.OpenRead( grfFilePath ) )
            using( var binaryReader = new BinaryReader( fileStream ) )
            {
                binaryReader.BaseStream.Seek( header.FileTablePosition, SeekOrigin.Begin );

                int currentEntryOffset = 0;
                for( int i = 0; i < header.FileCount; i++ )
                {
                    binaryReader.BaseStream.Seek( header.FileTablePosition + currentEntryOffset, SeekOrigin.Begin );
                    int nameLength = binaryReader.PeekChar() - 6;
                    int entryOffset = currentEntryOffset + binaryReader.ReadInt32() + 4;

                    binaryReader.ReadBytes( 2 );
                    var encodedName = binaryReader.ReadBytes( nameLength );
                    var fileName = DecodeFileName( encodedName.AsSpan() );

                    binaryReader.BaseStream.Seek( header.FileTablePosition + entryOffset, SeekOrigin.Begin );
                    uint compressedFileSizeBase = binaryReader.ReadUInt32();
                    uint compressedFileSizeAligned = binaryReader.ReadUInt32() - 37579;
                    uint uncompressedFileSize = binaryReader.ReadUInt32();
                    uint compressedFileSize = compressedFileSizeBase - uncompressedFileSize - 715;
                    var fileFlags = (FileFlag)binaryReader.ReadByte();
                    fileFlags |= IsFullEncrypted( fileName )
                        ? FileFlag.Mixed
                        : FileFlag.DES;
                    uint fileDataOffset = binaryReader.ReadUInt32() + (uint)header.Size;

                    // skip directories and files with zero size
                    if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                        continue;

                    entries.Add(
                        fileName,
                        new GrfEntry(
                            fileName,
                            fileDataOffset,
                            compressedFileSize,
                            compressedFileSizeAligned,
                            uncompressedFileSize,
                            fileFlags,
                            null ) );

                    currentEntryOffset = entryOffset + 17;
                }
            }

            return entries;
        }

        //private static Dictionary<string, GrfEntry> ReadVersion1xxFileNames( string grfFilePath, GrfHeader header )
        //{
        //    var fileNames = new Dictionary<string, GrfEntry>();

        //    using( var fileStream = File.OpenRead( grfFilePath ) )
        //    using( var binaryReader = new BinaryReader( fileStream ) )
        //    {
        //        for( int i = 0, fileEntryHeader = 0; i < header.FileCount; i++ )
        //        {
        //            fileStream.Seek( header.FileTablePosition + fileEntryHeader, SeekOrigin.Begin );
        //            int nameLength = binaryReader.PeekChar() - 6;
        //            int fileEntryData = fileEntryHeader + binaryReader.ReadInt32() + 4;

        //            binaryReader.ReadBytes( 2 );
        //            var encodedName = binaryReader.ReadBytes( nameLength );
        //            var fileName = DecodeFileName( encodedName.AsSpan() );
        //            fileNames.Add( fileName, null );

        //            fileEntryHeader = fileEntryData + 17;
        //        }
        //    }

        //    return fileNames;
        //}

        internal static bool IsFullEncrypted( string fileName )
        {
            var extensions = new string[] { ".gnd", ".gat", ".act", ".str" };
            foreach( var extension in extensions )
            {
                if( fileName.EndsWith( extension ) )
                    return false;
            }

            return true;
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
