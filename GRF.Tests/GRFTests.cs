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
        public void EntryNames_ReturnsEmptyList_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = new List<string>();

            var actual = grf.EntryNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void EntryNames_ReturnsAllFilesFromTestGrf_AfterLoadingAFile( string inputFile )
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

            var actual = grf.EntryNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void Entries_ContainGrfEntriesWithPaths_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expectedPaths = new List<string>() {
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

            foreach( var path in expectedPaths )
            {
                int index = grf.EntryNames.IndexOf(path);
                Assert.AreEqual( path, grf.Entries[index].Path );
            }
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void Files_ContainGrfEntriesWithNames_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expectedPathAndName = new List<(string, string)>() {
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

            foreach( var (path, name) in expectedPathAndName )
            {
                int index = grf.EntryNames.IndexOf(path);
                Assert.AreEqual(name, grf.Entries[index].Name);
            }
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void Files_ContainGrfEntriesWithTypes_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expectedPathAndTypes = new List<(string, string)>() {
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

            foreach( var (path, type) in expectedPathAndTypes )
            {
                int index = grf.EntryNames.IndexOf(path);
                Assert.AreEqual(type, grf.Entries[index].Type);
            }
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void UncompressedSize_ReturnsSameSizeAsExtractedData_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            grf.Load( inputFile );

            var entries = grf.Entries;

            Assert.IsNotEmpty( entries );
            entries.ForEach(entry => Assert.AreEqual(entry.header.uncompressedSize, entry.GetUncompressedData().Length));
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void GetUncompressedData_DoesntChangeOriginalDataOnUncompressing_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            grf.Load( inputFile );

            var entries = grf.Entries;

            Assert.IsNotEmpty( entries );
            entries.ForEach(entry => Assert.AreEqual(entry.header.uncompressedSize, entry.GetUncompressedData().Length));
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void EntryNames_ReturnsEmptyList_AfterUnloadingAPreviouslyLoadedFile( string inputFile )
        {
            var grf = new Grf();
            var expected = new List<string>();
            grf.Load( inputFile );
            grf.Unload();

            var actual = grf.EntryNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void EntryCount_ReturnsZero_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = 0;

            var actual = grf.EntryCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void EntryCount_ReturnsNine_AfterLoadingAFile( string inputFile )
        {
            var grf = new Grf();
            var expected = 9;
            grf.Load( inputFile );

            var actual = grf.EntryCount;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        [TestCaseSource( "InputFiles" )]
        public void EntryCount_ReturnsZero_AfterUnloadingAPreviouslyLoadedFile( string inputFile )
        {
            var grf = new Grf();
            var expected = 0;
            grf.Load( inputFile );
            grf.Unload();

            var actual = grf.EntryCount;

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
