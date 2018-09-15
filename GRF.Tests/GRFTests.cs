using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace GRF.Tests
{
    [TestFixture]
    public class GrfTests
    {
        public static string[] InputFiles() => new string[]
        {
            "Data/test102.grf",
            "Data/test103.grf",
            "Data/test200.grf"
        };

        [Test]
        public void FileNames_ReturnsEmptyList_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = new List<string>();

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void FileNames_ReturnsAllFilesFromTestGrf_AfterLoadingAFile( string inputFile )
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
            grf.Load( inputFile );

            var actual = grf.FileNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void Files_ContainGrfFilesWithFilePaths_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expectedFilePaths = new List<string>() {
                "data\\0_Tex1.bmp",
                "data\\11001.txt",
                "data\\balls.wav",
                "data\\idnum2itemdesctable.txt",
                "data\\idnum2itemdisplaynametable.txt",
                "data\\loading00.jpg",
                "data\\monstertalktable.xml",
                "data\\resnametable.txt",
                "data\\t2_¹è°æ1-1.bmp" };
            grf.Load( inputFile );

            var files = grf.Files;

            foreach( var filePath in expectedFilePaths )
            {
                var file = files[filePath];
                Assert.AreEqual( filePath, file.FilePath );
            }
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void Files_ContainGrfFilesWithFileNames_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expectedFilePathAndFileName = new List<(string, string)>() {
                ("data\\0_Tex1.bmp", "0_Tex1.bmp"),
                ("data\\11001.txt", "11001.txt"),
                ("data\\balls.wav", "balls.wav"),
                ("data\\idnum2itemdesctable.txt", "idnum2itemdesctable.txt"),
                ("data\\idnum2itemdisplaynametable.txt", "idnum2itemdisplaynametable.txt"),
                ("data\\loading00.jpg", "loading00.jpg"),
                ("data\\monstertalktable.xml", "monstertalktable.xml"),
                ("data\\resnametable.txt", "resnametable.txt"),
                ("data\\t2_¹è°æ1-1.bmp", "t2_¹è°æ1-1.bmp") };
            grf.Load( inputFile );

            var files = grf.Files;

            foreach( var (filePath, extension) in expectedFilePathAndFileName )
            {
                var file = files[filePath];
                Assert.AreEqual( extension, file.FileName );
            }
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void Files_ContainGrfFilesWithFileTypes_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expectedFilePathAndType = new List<(string, string)>() {
                ("data\\0_Tex1.bmp", "bmp"),
                ("data\\11001.txt", "txt"),
                ("data\\balls.wav", "wav"),
                ("data\\idnum2itemdesctable.txt", "txt"),
                ("data\\idnum2itemdisplaynametable.txt", "txt"),
                ("data\\loading00.jpg", "jpg"),
                ("data\\monstertalktable.xml", "xml"),
                ("data\\resnametable.txt", "txt"),
                ("data\\t2_¹è°æ1-1.bmp", "bmp") };
            grf.Load( inputFile );

            var files = grf.Files;

            foreach( var (filePath, extension) in expectedFilePathAndType )
            {
                var file = files[filePath];
                Assert.AreEqual( extension, file.FileType );
            }
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void UncompressedSize_ReturnsSameSizeAsExtractedData_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            grf.Load( inputFile );

            var files = grf.Files;

            Assert.IsNotEmpty( files );
            foreach( var file in files.Values )
            {
                Assert.AreEqual( file.UncompressedSize, file.GetUncompressedData().Length );
            }
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void FileNames_ReturnsEmptyList_AfterUnloadingAPreviouslyLoadedFile( string inputFile )
        {
            var grf = new Grf();
            var expected = new List<string>();
            grf.Load( inputFile );
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
        [TestCaseSource( "InputFiles" )]
        public void FileCount_ReturnsNine_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expected = 9;
            grf.Load( inputFile );

            var actual = grf.FileCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void FileCount_ReturnsZero_AfterUnloadingAPreviouslyLoadedFile( string inputFile )
        {
            var grf = new Grf();
            var expected = 0;
            grf.Load( inputFile );
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
        [TestCaseSource( "InputFiles" )]
        public void IsLoaded_ReturnsTrue_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expected = true;
            grf.Load( inputFile );

            var actual = grf.IsLoaded;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void IsLoaded_ReturnsFalse_AfterUnloadingAPreviouslyLoadedFile( string inputFile )
        {
            var grf = new Grf();
            var expected = false;
            grf.Load( inputFile );
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
        [TestCaseSource( "InputFiles" )]
        public void Signature_ReturnsMasterOfMagic_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expected = "Master of Magic";
            grf.Load( inputFile );

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void Signature_ReturnsEmptyString_AfterUnloadingAPreviouslyLoadedFile( string inputFile )
        {
            var grf = new Grf();
            var expected = string.Empty;
            grf.Load( inputFile );
            grf.Unload();

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }
    }
}
