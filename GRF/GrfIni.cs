using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRF
{
    internal class GrfIni
    {
        private Dictionary<string, Dictionary<string, string>> _sections = new Dictionary<string, Dictionary<string, string>>( StringComparer.InvariantCultureIgnoreCase );

        internal GrfIni() { }
        internal GrfIni( string path ) => Load( path );

        internal void Load( string path )
        {
            if( !File.Exists( path ) )
                throw new FileNotFoundException( path );

            var currentSection = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            _sections[string.Empty] = currentSection;

            foreach( var line in File.ReadAllLines( path ) )
            {
                var trimmedLine = line.Trim();
                if( trimmedLine.StartsWith( "[" ) && trimmedLine.EndsWith( "]" ) )
                {
                    var sectionName = trimmedLine.Substring( 1, trimmedLine.Length - 2 );
                    currentSection = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
                    _sections[sectionName] = currentSection;
                    continue;
                }

                if( trimmedLine.Contains( "=" ) )
                {
                    var split = trimmedLine.Split( '=' );
                    currentSection.Add( split[0], split[1] );
                }
            }
        }

        internal string[] Values( string section )
        {
            if( !_sections.ContainsKey( section ) )
                throw new KeyNotFoundException();

            return _sections[section].Values.ToArray();
        }
    }
}
