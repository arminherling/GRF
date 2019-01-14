using System;
using Ionic.Zlib;

namespace GRF
{
    public class GrfEntry
    {
        private readonly Grf _owner;
        private byte[] _data;

        private GrfEntryHeader Header { get; set; }

        public string Path { get; }
        public string Name => System.IO.Path.GetFileName( Path.Replace( "\\", "/" ) );
        public string Type => System.IO.Path.GetExtension( Path ).TrimStart( '.' );
        public uint Size => Header.UncompressedSize;

        public GrfEntry( string path, uint fileOffset, uint compressedSize, uint compressedFileSizeAligned, uint uncompressedSize, FileFlag flags, Grf owner )
        {
            Path = path;
            Header = new GrfEntryHeader
            {
                FileOffset = fileOffset,
                CompressedSize = compressedSize,
                CompressedSizeAligned = compressedFileSizeAligned,
                UncompressedSize = uncompressedSize,
                Flags = flags
            };
            _owner = owner;
        }

        public byte[] GetUncompressedData()
        {
            if( _data != null )
                return _data;

            Span<byte> newData = stackalloc byte[(int)Header.CompressedSizeAligned];
            _owner.GetCompressedBytes( Header.FileOffset, Header.CompressedSizeAligned ).CopyTo( newData );

            if( Header.Flags.HasFlag( FileFlag.Mixed ) )
            {
                DecodeFull( newData, Header.CompressedSize );
            }
            else if( Header.Flags.HasFlag( FileFlag.DES ) )
            {
                DecodeHeader( newData );
            }

            _data = ZlibStream.UncompressBuffer( newData.ToArray() );
            return _data;
        }

        public override int GetHashCode() => Path.GetHashCode();

        private static void DecodeHeader( Span<byte> data )
        {
            int blocks = data.Length / DataEncryptionStandard.BlockSize;

            // first 20 blocks are DES-encrypted
            for( int i = 0; i < 20 && i < blocks; ++i )
                DataEncryptionStandard.DecryptBlock( data.Slice( i * DataEncryptionStandard.BlockSize, DataEncryptionStandard.BlockSize ) );
        }

        private static void DecodeFull( Span<byte> source, uint compressedSize )
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
                    DataEncryptionStandard.DecryptBlock(
                        source.Slice(
                            i * DataEncryptionStandard.BlockSize,
                            DataEncryptionStandard.BlockSize ) );

                    continue;
                }

                ++j;
                if( ( j % gapBetweenShuffledBlocks ) == 0 && j != 0 )
                {
                    DeshuffleBlock(
                        source.Slice(
                            i * DataEncryptionStandard.BlockSize,
                            DataEncryptionStandard.BlockSize ) );

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
            if( input == 0x00 )
                return 0x2B;
            else if( input == 0x2B )
                return 0x00;
            else if( input == 0x6C )
                return 0x80;
            else if( input == 0x01 )
                return 0x68;
            else if( input == 0x68 )
                return 0x01;
            else if( input == 0x48 )
                return 0x77;
            else if( input == 0x60 )
                return 0xFF;
            else if( input == 0x77 )
                return 0x48;
            else if( input == 0xB9 )
                return 0xC0;
            else if( input == 0xC0 )
                return 0xB9;
            else if( input == 0xFE )
                return 0xEB;
            else if( input == 0xEB )
                return 0xFE;
            else if( input == 0x80 )
                return 0x6C;
            else if( input == 0xFF )
                return 0x60;
            else
                return input;
        }
    }
}
