using NUnit.Framework;
using System.IO;

namespace GRF.Tests
{
    [TestFixture]
    public class GRFTests
    {
        [Test]
        public void Open_ThrowsFileNotFound_WhenPassingInvalidPath()
        {
            var grf = new GRF();

            TestDelegate throwingMethod = () => { grf.Open( "some/path/file.grf" ); };

            Assert.Throws<FileNotFoundException>( throwingMethod );
        }
    }
}
