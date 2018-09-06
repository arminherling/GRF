using NUnit.Framework;
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
            var expected = 0;

            var actual = grf.FileNames.Count;

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
