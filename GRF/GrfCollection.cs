using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRF
{
    public class GrfCollection
    {
        private List<Grf> _grfs = new List<Grf>();

        public GrfCollection() { }
        public GrfCollection( string iniFilePath ) => Load( iniFilePath );

        public void Load( string iniFilePath, string sectionName = "Data" )
        {
            _grfs.Clear();

            var dataIni = new GrfIni( iniFilePath );
            var directory = Path.GetDirectoryName( iniFilePath );
            var grfFiles = dataIni.Values( sectionName );

            foreach( var grfFile in grfFiles )
            {
                var filePath = Path.Combine( directory, grfFile );
                _grfs.Add( new Grf( filePath ) );
            }
        }

        public void Unload()
        {
            foreach( var grf in _grfs )
            {
                grf.Unload();
            }

            _grfs.Clear();
        }

        public bool FindEntry( string entryName, out GrfEntry entry )
        {
            entry = null;
            foreach( var grf in _grfs )
            {
                if( grf.FindEntry( entryName, out entry ) )
                    break;
            }

            return !( entry is null );
        }

        public List<string> AllFileNames()
        {
            var fileNames = new List<string>();
            foreach( var grf in _grfs )
            {
                fileNames.AddRange( grf.EntryNames );
            }

            return fileNames.Distinct().ToList();
        }
    }
}
