using System;
using System.Collections.Generic;
using System.IO;

namespace GRF
{
    public class GRF
    {
        public int FileCount { get; } = 0;
        public bool IsOpen { get; private set; } = false;
        public List<string> FileNames { get; } = new List<string>();

        public void Open( string path )
        {
            var directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            var combinedPath = Path.Combine( directoryPath, path );
            if( !File.Exists( combinedPath ) )
                throw new FileNotFoundException( path );

            IsOpen = true;
        }
    }
}
