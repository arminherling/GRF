using NUnit.Framework;
using System.IO;

namespace GRF.Tests
{
    [TestFixture]
    public class GRFTests
    {
        [Test]
        public void FileCount_ReturnsZero_BeforeOpeningAFile()
        {
            var grf = new GRF();
            var expected = 0;

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
        public void Open_ThrowsFileNotFound_WhenPassingInvalidPath()
        {
            var grf = new GRF();

            void throwingMethod() { grf.Open( "some/path/file.grf" ); }

            Assert.Throws<FileNotFoundException>( throwingMethod );
        }
    }
}
