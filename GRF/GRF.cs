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
        public string Signature => Header.Signature;
        public List<string> EntryNames { get; private set; }
        public int Count => Entries.Count;
        private Dictionary<string, GrfEntry> Entries { get; set; }

        private string _filePath;

        public static Grf FromFile( string grfFilePath, LoadingMode loadingMode = LoadingMode.Deferred )
        {
            var grf = new Grf();
            grf.Load( grfFilePath, loadingMode );
            return grf;
        }

        private Grf() { }

        private void Load( string grfFilePath, LoadingMode loadingMode )
        {
            _filePath = grfFilePath;
            Header = GrfFileReader.ReadHeader( grfFilePath );
            Entries = GrfFileReader.ReadEntries( grfFilePath, Header, loadingMode );
            EntryNames = Entries.Keys.ToList();
        }

        public bool Find( string entryName, out GrfEntry entry )
        {
            entry = Entries.ContainsKey( entryName )
                ? Entries[entryName]
                : null;

            return !(entry is null);
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
