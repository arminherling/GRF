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
            byte[] newData = new byte[_data.Length];
            _data.CopyTo( newData, 0 );

            if( Flags.HasFlag( FileFlag.Mixed ) )
            {
                DecodeFull( ref newData, CompressedSize );
            }
            else if( Flags.HasFlag( FileFlag.DES ) )
            {
                DecodeHeader( ref newData, CompressedSize );
            }
            return ZlibStream.UncompressBuffer( newData );
        }

        private static void DecodeHeader( ref byte[] data, int uncompressedSize )
        {
            int blocks = data.Length / 8;
            // first 20 blocks are DES-encrypted
            for( int i = 0; i < 20 && i < blocks; ++i )
                DataEncryptionStandard.DecryptBlock( ref data, i * 8 );
        }

        private static void DecodeFull( ref byte[] data, int uncompressedSize )
        {
            DecodeHeader( ref data, uncompressedSize );

            int blocks = data.Length / 8;
            int digits = uncompressedSize.ToString().Length;

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
                    DataEncryptionStandard.DecryptBlock( ref data, i * 8 );
                    continue;
                }

                ++j;
                if( ( j % gapBetweenShuffledBlocks ) == 0 && j != 0 )
                {
                    DeshuffleBlock( ref data, i * 8 );
                    continue;
                }
            }
        }

        private static void DeshuffleBlock( ref byte[] src, int start )
        {
            byte[] tmp = new byte[8];

            tmp[0] = src[start + 3];
            tmp[1] = src[start + 4];
            tmp[2] = src[start + 6];
            tmp[3] = src[start + 0];
            tmp[4] = src[start + 1];
            tmp[5] = src[start + 2];
            tmp[6] = src[start + 5];
            tmp[7] = Substitute( src[start + 7] );

            Array.Copy( tmp, 0, src, start, 8 );
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
