using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRF
{
    internal class GrfIni
    {
        Dictionary<string, Dictionary<string, string>> _sections = new Dictionary<string, Dictionary<string, string>>( StringComparer.InvariantCultureIgnoreCase );

        internal GrfIni() { }
        internal GrfIni( string path ) => Load( path );

        internal void Load( string path )
        {
            if( !File.Exists( path ) )
                throw new FileNotFoundException( path );

            var currentSection = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            _sections[""] = currentSection;

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

        internal string Value( string section, string key, string defaultValue = "" )
        {
            if( !_sections.ContainsKey( section ) )
                return defaultValue;

            if( !_sections[section].ContainsKey( key ) )
                return defaultValue;

            return _sections[section][key];
        }

        internal string[] Keys( string section )
        {
            if( !_sections.ContainsKey( section ) )
                return new string[0];

            return _sections[section].Keys.ToArray();
        }

        internal string[] Values( string section )
        {
            if( !_sections.ContainsKey( section ) )
                return new string[] { };

            return _sections[section].Values.ToArray();
        }
    }
}
