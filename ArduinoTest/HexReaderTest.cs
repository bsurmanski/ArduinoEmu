using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ArduinoTest
{
    [TestClass]
    public class HexReaderTest
    {
        byte[] hexcoded;
        byte[] raw = { 0x94, 0x0C, 0x00, 0x5C, 0x94, 0x0C, 0x00, 0x79, 0x94, 0x0C, 0x00, 0x79, 0x94, 0x0C, 0x00, 0x79,
                       0x94, 0x0C, 0x00, 0x79, 0x94, 0x0C, 0x00, 0x79, 0x94, 0x0C, 0x00, 0x79, 0x94, 0x0C, 0x00, 0x79 };
        public HexReaderTest()
        {
            // .hex encoded version of 'raw'
            string testString = ":100000000C945C000C9479000C9479000C947900A9\n:100010000C9479000C9479000C9479000C9479007C";
            hexcoded = System.Text.Encoding.ASCII.GetBytes(testString);
        }

        [TestMethod]
        public void Decode()
        {
           
            HexReader hex = new HexReader(new MemoryStream(hexcoded));
            for (int i = 0; i < raw.Length; i++)
            {
                Assert.AreEqual(raw[i], hex.ReadByte());
            }
        }

        [TestMethod]
        public void Length()
        {
            HexReader hex = new HexReader(new MemoryStream(hexcoded));
            long position = hex.Position;
            Assert.AreEqual(32, hex.Length);
            Assert.AreEqual(position, hex.Position);
        }
    }
}
