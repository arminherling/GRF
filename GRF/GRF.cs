using System;
using System.IO;

namespace GRF
{
    public class GRF
    {
        public void Open( string path )
        {
            if( !File.Exists( path ) )
                throw new FileNotFoundException( path );
        }
    }
}
