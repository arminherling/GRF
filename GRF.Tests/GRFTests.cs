using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace GRF.Tests
{
    [TestFixture]
    public class GRFTests
    {
        [Test]
        public void FileNames_ReturnsEmptyList_BeforeOpeningAFile()
        {
            var grf = new GRF();
            var expected = new List<string>();

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileNames_ReturnsAllFilesFromTestGrf_AfterOpeningAFile()
        {
            var grf = new GRF();
            var expected = new List<string>() {
                "data\\0_Tex1.bmp",
                "data\\11001.txt",
                "data\\balls.wav",
                "data\\idnum2itemdesctable.txt",
                "data\\idnum2itemdisplaynametable.txt",
                "data\\loading00.jpg",
                "data\\monstertalktable.xml",
                "data\\resnametable.txt",
                "data\\t2_¹è°æ1-1.bmp" };
            grf.Open( "Data/test.grf" );

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileNames_ReturnsEmptyList_AfterClosingAPreviouslyOpenedFile()
        {
            var grf = new GRF();
            var expected = new List<string>();
            grf.Open( "Data/test.grf" );
            grf.Close();

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileCount_ReturnsZero_BeforeOpeningAFile()
        {
            var grf = new GRF();
            var expected = 0;

            var actual = grf.FileCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileCount_ReturnsNine_AfterOpeningAFile()
        {
            var grf = new GRF();
            var expected = 9;
            grf.Open( "Data/test.grf" );

            var actual = grf.FileCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileCount_ReturnsZero_AfterClosingAPreviouslyOpenedFile()
        {
            var grf = new GRF();
            var expected = 0;
            grf.Open( "Data/test.grf" );
            grf.Close();

            var actual = grf.FileCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void IsOpen_ReturnsFalse_BeforeOpeningAFile()
        {
            var grf = new GRF();
            var expected = false;

            var actual = grf.IsOpen;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void IsOpen_ReturnsTrue_AfterOpeningAFile()
        {
            var grf = new GRF();
            var expected = true;
            grf.Open( "Data/test.grf" );

            var actual = grf.IsOpen;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void IsOpen_ReturnsFalse_AfterClosingAPreviouslyOpenedFile()
        {
            var grf = new GRF();
            var expected = false;
            grf.Open( "Data/test.grf" );
            grf.Close();

            var actual = grf.IsOpen;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Open_ThrowsFileNotFound_WhenPassingInvalidPath()
        {
            var grf = new GRF();

            void throwingMethod() { grf.Open( "some/path/file.grf" ); }

            Assert.Throws<FileNotFoundException>( throwingMethod );
        }

        [Test]
        public void Signature_ReturnsEmptyString_BeforeOpeningAFile()
        {
            var grf = new GRF();
            var expected = string.Empty;

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Signature_ReturnsMasterOfMagic_AfterOpeningAFile()
        {
            var grf = new GRF();
            var expected = "Master of Magic";
            grf.Open( "Data/test.grf" );

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Signature_ReturnsEmptyString_AfterClosingAPreviouslyOpenedFile()
        {
            var grf = new GRF();
            var expected = string.Empty;
            grf.Open( "Data/test.grf" );
            grf.Close();

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }
    }
}
