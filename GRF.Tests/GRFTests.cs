using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace GRF.Tests
{
    [TestFixture]
    public class GrfTests
    {
        [Test]
        public void FileNames_ReturnsEmptyList_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = new List<string>();

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileNames_ReturnsAllFilesFromTestGrf_AfterLoadingAFile()
        {
            var grf = new Grf();
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
            grf.Load( "Data/test.grf" );

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }
        
        [Test]
        public void Files_ContainGrfFilesWithSameName_AfterLoadingAFile()
        {
            var grf = new Grf();
            var expectedNames = new List<string>() {
                "data\\0_Tex1.bmp",
                "data\\11001.txt",
                "data\\balls.wav",
                "data\\idnum2itemdesctable.txt",
                "data\\idnum2itemdisplaynametable.txt",
                "data\\loading00.jpg",
                "data\\monstertalktable.xml",
                "data\\resnametable.txt",
                "data\\t2_¹è°æ1-1.bmp" };
            grf.Load( "Data/test.grf" );

            var files = grf.Files;

            foreach(var name in expectedNames )
            {
                var file = files[name];
                Assert.AreEqual( name, file.Name );
            }
        }

        [Test]
        public void FileNames_ReturnsEmptyList_AfterUnloadingAPreviouslyLoadedFile()
        {
            var grf = new Grf();
            var expected = new List<string>();
            grf.Load( "Data/test.grf" );
            grf.Unload();

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileCount_ReturnsZero_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = 0;

            var actual = grf.FileCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileCount_ReturnsNine_AfterLoadingAFile()
        {
            var grf = new Grf();
            var expected = 9;
            grf.Load( "Data/test.grf" );

            var actual = grf.FileCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void FileCount_ReturnsZero_AfterUnloadingAPreviouslyLoadedFile()
        {
            var grf = new Grf();
            var expected = 0;
            grf.Load( "Data/test.grf" );
            grf.Unload();

            var actual = grf.FileCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void IsLoaded_ReturnsFalse_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = false;

            var actual = grf.IsLoaded;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void IsLoaded_ReturnsTrue_AfterLoadingAFile()
        {
            var grf = new Grf();
            var expected = true;
            grf.Load( "Data/test.grf" );

            var actual = grf.IsLoaded;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void IsLoaded_ReturnsFalse_AfterUnloadingAPreviouslyLoadedFile()
        {
            var grf = new Grf();
            var expected = false;
            grf.Load( "Data/test.grf" );
            grf.Unload();

            var actual = grf.IsLoaded;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Load_ThrowsFileNotFound_WhenPassingInvalidPath()
        {
            var grf = new Grf();

            void throwingMethod() { grf.Load( "some/path/file.grf" ); }

            Assert.Throws<FileNotFoundException>( throwingMethod );
        }

        [Test]
        public void Signature_ReturnsEmptyString_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = string.Empty;

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Signature_ReturnsMasterOfMagic_AfterLoadingAFile()
        {
            var grf = new Grf();
            var expected = "Master of Magic";
            grf.Load( "Data/test.grf" );

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Signature_ReturnsEmptyString_AfterUnloadingAPreviouslyLoadedFile()
        {
            var grf = new Grf();
            var expected = string.Empty;
            grf.Load( "Data/test.grf" );
            grf.Unload();

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }
    }
}
