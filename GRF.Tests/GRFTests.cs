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

        public static LoadingMode[] LoadingModes() => new LoadingMode[]
        {
            LoadingMode.Immediate,
            LoadingMode.Deferred
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
        public void EntryNames_ReturnsAllFilesFromTestGrf_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
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
            grf.Load( inputFile, mode );

            var actual = grf.EntryNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Entries_ContainGrfEntriesWithPaths_AfterLoadingAFile( 
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
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
            grf.Load( inputFile, mode );

            foreach( var path in expectedPaths )
            {
                var entryFound = grf.Find( path, out GrfEntry entry );
                Assert.IsTrue( entryFound );
                Assert.AreEqual( path, entry.Path );
            }
        }

        [Test]
        public void Files_ContainGrfEntriesWithNames_AfterLoadingAFile( 
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
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
            grf.Load( inputFile, mode );

            foreach( var (path, name) in expectedPathAndName )
            {
                var entryFound = grf.Find( path, out GrfEntry entry );
                Assert.IsTrue( entryFound );
                Assert.AreEqual( name, entry.Name );
            }
        }

        [Test]
        public void Files_ContainGrfEntriesWithTypes_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
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
            grf.Load( inputFile, mode );

            foreach( var (path, type) in expectedPathAndTypes )
            {
                var entryFound = grf.Find( path, out GrfEntry entry );
                Assert.IsTrue( entryFound );
                Assert.AreEqual( type, entry.Type);
            }
        }

        [Test]
        public void UncompressedSize_ReturnsSameSizeAsExtractedData_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            grf.Load( inputFile, mode );

            Assert.IsTrue( grf.Count != 0 );
            foreach(var path in grf.EntryNames )
            {
                var entryFound = grf.Find( path, out GrfEntry entry );
                Assert.IsTrue( entryFound );
                Assert.AreEqual( entry.Size, entry.GetUncompressedData().Length );
            }
        }

        [Test]
        public void GetUncompressedData_DoesntChangeOriginalDataOnUncompressing_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            grf.Load( inputFile, mode );

            Assert.IsTrue( grf.Count != 0 );
            foreach( var path in grf.EntryNames )
            {
                var entryFound = grf.Find( path, out GrfEntry entry );
                Assert.IsTrue( entryFound );
                Assert.AreEqual( entry.Size, entry.GetUncompressedData().Length );
                Assert.AreEqual( entry.Size, entry.GetUncompressedData().Length );
            }
        }

        [Test]
        public void EntryNames_ReturnsEmptyList_AfterUnloadingAPreviouslyLoadedFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            var expected = new List<string>();
            grf.Load( inputFile, mode );
            grf.Unload();

            var actual = grf.EntryNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void EntryCount_ReturnsZero_BeforeLoadingAFile()
        {
            var grf = new Grf();
            var expected = 0;

            var actual = grf.Count;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void EntryCount_ReturnsNine_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            var expected = 9;
            grf.Load( inputFile, mode );

            var actual = grf.Count;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void EntryCount_ReturnsZero_AfterUnloadingAPreviouslyLoadedFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            var expected = 0;
            grf.Load( inputFile, mode );
            grf.Unload();

            var actual = grf.Count;

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
        public void IsLoaded_ReturnsTrue_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            var expected = true;
            grf.Load( inputFile, mode );

            var actual = grf.IsLoaded;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void IsLoaded_ReturnsFalse_AfterUnloadingAPreviouslyLoadedFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            var expected = false;
            grf.Load( inputFile, mode );
            grf.Unload();

            var actual = grf.IsLoaded;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Load_ThrowsDirectoryNotFound_WhenPassingInvalidPath()
        {
            var grf = new Grf();

            void throwingMethod() { grf.Load( "some/path/file.grf" ); }

            Assert.Throws<DirectoryNotFoundException>( throwingMethod );
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
        public void Signature_ReturnsMasterOfMagic_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            var expected = "Master of Magic";
            grf.Load( inputFile, mode );

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Signature_ReturnsEmptyString_AfterUnloadingAPreviouslyLoadedFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var grf = new Grf();
            var expected = string.Empty;
            grf.Load( inputFile, mode );
            grf.Unload();

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }
    }
}
