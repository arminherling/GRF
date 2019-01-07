using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRF
{
    public class GrfCollection
    {
        private List<Grf> _loadedGrfs = new List<Grf>();

        public GrfCollection() { }
        public GrfCollection( string iniFilePath ) => Load( iniFilePath );

        public void Load( string iniFilePath, string sectionName = "Data" )
        {
            _loadedGrfs.Clear();

            var dataIni = new GrfIni( iniFilePath );
            var directory = Path.GetDirectoryName( iniFilePath );
            var grfFiles = dataIni.Values( sectionName );

            foreach( var grfFile in grfFiles )
            {
                var filePath = Path.Combine( directory, grfFile );
                _loadedGrfs.Add( new Grf( filePath ) );
            }
        }

        public void Unload()
        {
            foreach( var grf in _loadedGrfs)
            {
                grf.Unload();
            }
            _loadedGrfs.Clear();
        }

        public bool FindEntry( string entryPath, out GrfEntry file )
        {
            foreach( var grf in _loadedGrfs )
            {
                if( grf.Entries.TryGetValue( entryPath, out file ) )
                    return true;
            }
            file = null;
            return false;
        }

        public List<string> AllFileNames()
        {
            var fileNames = new List<string>();
            foreach( var grf in _loadedGrfs )
            {
                fileNames.AddRange( grf.EntryNames );
            }
            return fileNames.Distinct().ToList();
        }
    }
}
