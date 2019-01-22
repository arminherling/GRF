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
        public void EntryNames_ReturnsAllFilesFromTestGrf_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
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
            var grf = Grf.FromFile( inputFile, mode );

            var actual = grf.EntryNames;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Entries_ContainGrfEntriesWithPaths_AfterLoadingAFile( 
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
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
            var grf = Grf.FromFile( inputFile, mode );

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
            var grf = Grf.FromFile( inputFile, mode );

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
            var grf = Grf.FromFile( inputFile, mode );

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
            var grf = Grf.FromFile( inputFile, mode );

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
            var grf = Grf.FromFile( inputFile, mode );

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
        public void EntryCount_ReturnsNine_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var expected = 9;
            var grf = Grf.FromFile( inputFile, mode );

            var actual = grf.Count;

            Assert.AreEqual( expected, actual );
        }

        [Test]
        public void Load_ThrowsDirectoryNotFound_WhenPassingInvalidPath(
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            void throwingMethod() { Grf.FromFile( "some/path/file.grf", mode ); }

            Assert.Throws<DirectoryNotFoundException>( throwingMethod );
        }

        [Test]
        public void Signature_ReturnsMasterOfMagic_AfterLoadingAFile(
            [ValueSource( "InputFiles" )] string inputFile,
            [ValueSource( "LoadingModes" )] LoadingMode mode )
        {
            var expected = "Master of Magic";
            var grf = Grf.FromFile( inputFile, mode );

            var actual = grf.Signature;

            Assert.AreEqual( expected, actual );
        }
    }
}
