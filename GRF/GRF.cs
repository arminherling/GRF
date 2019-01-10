using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;
using Microsoft.Win32.SafeHandles;

namespace GRF
{
    public class Grf
    {
        static readonly uint GrfHeaderSize = 46;

        public bool IsLoaded { get; private set; }
        public string Signature { get; private set; } = string.Empty;
        public List<GrfEntry> Entries { get; set; } = new List<GrfEntry>();
        public int EntryCount => Entries.Count;
        public List<string> EntryNames => Entries.ConvertAll<string>(f => f.Path);
        public string FilePath { get; private set; }

        public GrfFileHeader header { get; private set; }

        public Grf() { }
        public Grf( string grfFilePath ) => Load( grfFilePath );

        public void Load( string grfFilePath )
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            FileInfo fiGrf = new FileInfo(Path.Combine(baseDirectory, grfFilePath));
            FilePath = fiGrf.FullName;
            if(!fiGrf.Exists)
                throw new FileNotFoundException(grfFilePath);

            this.header = new GrfFileHeader();

            using (FileStream fsGrf = fiGrf.OpenRead())
            using (BinaryReader brGrf = new BinaryReader(fsGrf))
            {
                this.header.signature = brGrf.ReadBytes(15);
                Signature = Encoding.ASCII.GetString(this.header.signature);
                brGrf.ReadByte(); // string null terminator

                this.header.encryptKey = brGrf.ReadBytes(14);
                this.header.fileOffset = brGrf.ReadUInt32();
                this.header.seed = brGrf.ReadUInt32();
                this.header.fileCount = brGrf.ReadUInt32();
                this.header.version = (GrfFormat)brGrf.ReadUInt32();

                brGrf.BaseStream.Seek(this.header.fileOffset, SeekOrigin.Current);

                this.header.fileCount -= 7;

                if(this.header.version == GrfFormat.Version102 || this.header.version == GrfFormat.Version103) {
                    this.header.fileCount -= this.header.seed;
                    LoadVersion1xx(
                        brGrf,
                        this.header.fileCount);
                } else if(this.header.version == GrfFormat.Version200) {
                    LoadVersion2xx(
                        brGrf,
                        this.header.fileCount);
                } else {
                    throw new NotImplementedException( $"Version {this.header.version} of GRF files is currently not supported." );
                }
                IsLoaded = true;

                brGrf.Close();
                brGrf.Dispose();
            }
        }

        public void Unload()
        {
            Entries.Clear();
            FilePath = string.Empty;
            Signature = string.Empty;
            IsLoaded = false;
        }

        private void LoadVersion1xx(BinaryReader streamReader, uint fileCount)
        {
            uint bodySize = (uint)(streamReader.BaseStream.Length - streamReader.BaseStream.Position);
            byte[] bodyData = streamReader.ReadBytes((int)bodySize);
            MemoryStream bodyStream = new MemoryStream(bodyData);
            BinaryReader bodyReader = new BinaryReader(bodyStream);

            for( int i = 0, fileEntryHeader = 0; i < fileCount; i++ )
            {
                bodyReader.BaseStream.Seek(fileEntryHeader, SeekOrigin.Begin);
                int nameLength = bodyReader.PeekChar() - 6;
                int fileEntryData = fileEntryHeader + bodyReader.ReadInt32() + 4;

                bodyReader.BaseStream.Seek(fileEntryHeader + 6, SeekOrigin.Begin);
                var encodedName = bodyReader.ReadBytes(nameLength);
                var fileName = DecodeFileName(encodedName.AsSpan());

                bodyReader.BaseStream.Seek(fileEntryData, SeekOrigin.Begin);
                uint compressedFileSizeBase = bodyReader.ReadUInt32();
                uint compressedFileSizeAligned = bodyReader.ReadUInt32() - 37579;
                uint uncompressedFileSize = bodyReader.ReadUInt32();
                uint compressedFileSize = compressedFileSizeBase - uncompressedFileSize - 715;
                FileFlag fileFlags = (FileFlag)bodyReader.ReadByte();
                fileFlags |= IsFullEncrypted( fileName )
                    ? FileFlag.Mixed
                    : FileFlag.DES;
                uint fileDataOffset = bodyReader.ReadUInt32() + GrfHeaderSize;

                // skip directories and files with zero size
                if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                    continue;

                streamReader.BaseStream.Seek(fileDataOffset, SeekOrigin.Begin);

                Entries.Add(
                    new GrfEntry(
                        fileName,
                        fileDataOffset,
                        compressedFileSize,
                        compressedFileSizeAligned,
                        uncompressedFileSize,
                        fileFlags,
                        this));

                fileEntryHeader = fileEntryData + 17;
            }
            bodyReader.Close();
        }

        private void LoadVersion2xx(BinaryReader streamReader, uint fileCount)
        {
            uint compressedBodySize = streamReader.ReadUInt32();
            uint bodySize = streamReader.ReadUInt32();

            byte[] compressedBody = streamReader.ReadBytes((int)compressedBodySize);
            byte[] bodyData = ZlibStream.UncompressBuffer(compressedBody);

            MemoryStream bodyStream = new MemoryStream(bodyData);
            BinaryReader bodyReader = new BinaryReader(bodyStream);

            for( int i = 0; i < fileCount; i++ )
            {
                string fileName = string.Empty;
                char currentChar;
                while( ( currentChar = (char)bodyReader.ReadByte() ) != 0 )
                {
                    fileName += currentChar;
                }

                uint compressedFileSize = bodyReader.ReadUInt32();
                uint compressedFileSizeAligned = bodyReader.ReadUInt32();
                uint uncompressedFileSize = bodyReader.ReadUInt32();
                FileFlag fileFlags = (FileFlag)bodyReader.ReadByte();
                uint fileDataOffset = bodyReader.ReadUInt32();

                // skip directories and files with zero size
                if( !fileFlags.HasFlag( FileFlag.File ) || uncompressedFileSize == 0 )
                    continue;


                Entries.Add(
                    new GrfEntry(
                        fileName,
                        GrfHeaderSize + fileDataOffset,
                        compressedFileSize,
                        compressedFileSizeAligned,
                        uncompressedFileSize,
                        fileFlags,
                        this));
            }

            bodyReader.Close();
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

            string fileName = string.Empty;
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
            string[] extensions = { ".gnd", ".gat", ".act", ".str" };
            foreach( var extension in extensions )
            {
                if( fileName.EndsWith( extension ) )
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get compressed bytes.
        /// </summary>
        /// <param name="offset">Position to fetch</param>
        /// <param name="len">Size to fetch</param>
        /// <returns>All bytes inside the range</returns>
        public byte[] GetCompressedBytes(uint offset, uint len)
        {
            byte[] data = new byte[len];

            using (FileStream fsGrf = new FileStream(FilePath, FileMode.Open))
            using (BinaryReader brGrf = new BinaryReader(fsGrf))
            {
                brGrf.BaseStream.Seek(offset, SeekOrigin.Begin);
                data = brGrf.ReadBytes((int)len);
                brGrf.Close();
                brGrf.Dispose();
            }

            return data;
        }

        /// <summary>
        /// Searches a file inside entries based on hashCode
        /// </summary>
        /// <param name="fileName">Full path to search in</param>
        /// <returns>If found, GrfEntry object is returned.</returns>
        public GrfEntry SearchEntry(string fileName)
        {
            int hashCode = fileName.GetHashCode();
            return this.Entries.FirstOrDefault(entry => entry.GetHashCode().Equals(hashCode));
        }
    }
}
