using Ionic.Zlib;
using System;
using System.IO;

namespace GRF
{
    public class GrfFile
    {
        private byte[] _data;

        public GrfFile( byte[] data, string filePath, int compressedSize, int uncompressedSize, FileFlag flags )
        {
            _data = data;
            FilePath = filePath;
            FileName = Path.GetFileName( filePath.Replace( "\\", "/" ) );
            FileType = Path.GetExtension( filePath ).TrimStart( '.' );
            CompressedSize = compressedSize;
            CompressedSizeAligned = _data.Length;
            UncompressedSize = uncompressedSize;
            Flags = flags;
        }

        public string FilePath { get; }
        public string FileName { get; }
        public string FileType { get; }
        public int CompressedSize { get; }
        public int CompressedSizeAligned { get; }
        public int UncompressedSize { get; }
        public FileFlag Flags { get; }

        public byte[] GetUncompressedData()
        {
            Span<byte> newData = stackalloc byte[_data.Length];
            _data.CopyTo( newData );

            if( Flags.HasFlag( FileFlag.Mixed ) )
            {
                DecodeFull( newData, CompressedSize );
            }
            else if( Flags.HasFlag( FileFlag.DES ) )
            {
                DecodeHeader( newData );
            }
            return ZlibStream.UncompressBuffer( newData.ToArray() );
        }

        private static void DecodeHeader( Span<byte> data )
        {
            int blocks = data.Length / DataEncryptionStandard.BlockSize;
            // first 20 blocks are DES-encrypted
            for( int i = 0; i < 20 && i < blocks; ++i )
                DataEncryptionStandard.DecryptBlock( data.Slice( i * DataEncryptionStandard.BlockSize, DataEncryptionStandard.BlockSize ) );
        }

        private static void DecodeFull( Span<byte> source, int compressedSize )
        {
            DecodeHeader( source );

            int blocks = source.Length / DataEncryptionStandard.BlockSize;
            int digits = compressedSize.ToString().Length;

            int gapBetweenEncryptedBlocks = ( digits < 3 ) ? 1
                   : ( digits < 5 ) ? digits + 1
                   : ( digits < 7 ) ? digits + 9
                   : digits + 15;
            int gapBetweenShuffledBlocks = 7;

            int j = -1;
            for( int i = 20; i < blocks; ++i )
            {
                if( ( i % gapBetweenEncryptedBlocks ) == 0 )
                {
                    // DES-encrypted
                    DataEncryptionStandard.DecryptBlock( source.Slice( i * DataEncryptionStandard.BlockSize, DataEncryptionStandard.BlockSize ) );
                    continue;
                }

                ++j;
                if( ( j % gapBetweenShuffledBlocks ) == 0 && j != 0 )
                {
                    DeshuffleBlock( source.Slice( i * DataEncryptionStandard.BlockSize, DataEncryptionStandard.BlockSize ) );
                    continue;
                }
            }
        }

        private static void DeshuffleBlock( Span<byte> source )
        {
            Span<byte> tempData = stackalloc byte[DataEncryptionStandard.BlockSize];

            tempData[0] = source[3];
            tempData[1] = source[4];
            tempData[2] = source[6];
            tempData[3] = source[0];
            tempData[4] = source[1];
            tempData[5] = source[2];
            tempData[6] = source[5];
            tempData[7] = Substitute( source[7] );

            tempData.CopyTo( source );
        }

        private static byte Substitute( byte input )
        {
            byte output;
            switch( input )
            {
                case 0x00: output = 0x2B; break;
                case 0x2B: output = 0x00; break;
                case 0x6C: output = 0x80; break;
                case 0x01: output = 0x68; break;
                case 0x68: output = 0x01; break;
                case 0x48: output = 0x77; break;
                case 0x60: output = 0xFF; break;
                case 0x77: output = 0x48; break;
                case 0xB9: output = 0xC0; break;
                case 0xC0: output = 0xB9; break;
                case 0xFE: output = 0xEB; break;
                case 0xEB: output = 0xFE; break;
                case 0x80: output = 0x6C; break;
                case 0xFF: output = 0x60; break;
                default: output = input; break;
            }

            return output;
        }
    }
}
