using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GRF
{
    public class GRF
    {
        private Stream _stream;

        public int FileCount { get; } = 0;
        public bool IsOpen { get; private set; } = false;
        public List<string> FileNames { get; } = new List<string>();
        public string Signature { get; private set; } = string.Empty;

        public void Open( string filePath )
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var absolutePath = Path.Combine( baseDirectory, filePath );
            if( !File.Exists( absolutePath ) )
                throw new FileNotFoundException( filePath );

            _stream = new MemoryStream( File.ReadAllBytes( absolutePath ) );
            var byteReader = new BinaryReader( _stream );

            //var signatureBytes = new byte[15];
            //byteReader.Read( signatureBytes, 0, 15 );
            var signatureBytes = byteReader.ReadBytes( 15 );
            Signature = Encoding.ASCII.GetString( signatureBytes );







            IsOpen = true;
        }

        public void Close()
        {
            if( IsOpen )
            {
                _stream.Close();
                Signature = string.Empty;
            }

            IsOpen = false;
        }
    }
}
