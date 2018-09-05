using System;
using System.Collections.Generic;
using System.IO;

namespace GRF
{
    public class GRF
    {
        public int FileCount { get; } = 0;
        public bool IsOpen { get; } = false;
        public List<string> FileNames { get; } = new List<string>();

        public void Open( string path )
        {
            if( !File.Exists( path ) )
                throw new FileNotFoundException( path );
        }
    }
}
