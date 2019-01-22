using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRF
{
    public class GrfCollection
    {
        private List<Grf> _grfs = new List<Grf>();

        public static GrfCollection FromFile( string iniFilePath, string sectionName = "Data", LoadingMode loadingMode = LoadingMode.Deferred )
        {
            var grfCollection = new GrfCollection();
            grfCollection.Load( iniFilePath, sectionName, loadingMode );
            return grfCollection;
        }

        private GrfCollection() { }

        private void Load( string iniFilePath, string sectionName, LoadingMode loadingMode )
        {
            var dataIni = new GrfIni( iniFilePath );
            var directory = Path.GetDirectoryName( iniFilePath );
            var grfFiles = dataIni.Values( sectionName );

            foreach( var grfFile in grfFiles )
            {
                var filePath = Path.Combine( directory, grfFile );
                _grfs.Add( Grf.FromFile( filePath, loadingMode ) );
            }
        }

        public bool Find( string entryName, out GrfEntry entry )
        {
            entry = null;
            foreach( var grf in _grfs )
            {
                if( grf.Find( entryName, out entry ) )
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
