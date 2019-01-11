using Ionic.Zlib;
using System;
using System.IO;

namespace GRF
{
    public class GrfEntry
    {
        private byte[] _data;
        private Grf _owner;
        public GrfEntryHeader header { get; internal set; }

        private int hashCode { get; set; }

        public GrfEntry(string path, uint fileOffset, uint compressedSize, uint compressedFileSizeAligned, uint uncompressedSize, FileFlag flags, Grf owner)
        {
            Path = path;
            hashCode = Path.GetHashCode();
            this.header = new GrfEntryHeader();
            this.header.fileOffset = fileOffset;
            this.header.compressedSize = compressedSize;
            this.header.compressedSizeAligned = compressedFileSizeAligned;
            this.header.uncompressedSize = uncompressedSize;
            this.header.flags = flags;
            _owner = owner;
            this._data = null;
        }

        public string Path { get; }
        public string Name => System.IO.Path.GetFileName( Path.Replace( "\\", "/" ) );
        public string Type => System.IO.Path.GetExtension( Path ).TrimStart( '.' );

        public byte[] GetUncompressedData()
        {
            if (this._data != null)
                return this._data;
        
            Span<byte> newData = stackalloc byte[(int)this.header.compressedSizeAligned];
            this._owner.GetCompressedBytes(this.header.fileOffset, this.header.compressedSizeAligned).CopyTo(newData);

            if(this.header.flags.HasFlag(FileFlag.Mixed)) {
                DecodeFull(newData, (int)this.header.compressedSize);
            } else if(this.header.flags.HasFlag(FileFlag.DES)) {
                DecodeHeader(newData);
            }

            this._data = ZlibStream.UncompressBuffer(newData.ToArray());
            return this.GetUncompressedData();
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

        public override int GetHashCode()
        {
            return this.hashCode;
        }
    }
}
