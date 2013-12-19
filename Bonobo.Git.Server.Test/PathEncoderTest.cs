using Bonobo.Git.Server.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Test
{
    [TestClass]
    public class PathEncoderTest
    {
        [TestMethod]
        public void NullInput()
        {
            AssertAllRequirements(null);
        }

        [TestMethod]
        public void EmptyInput()
        {
            AssertAllRequirements("");
        }

        [TestMethod]
        public void SpaceInput()
        {
            AssertAllRequirements("     ");
        }

        [TestMethod]
        public void LetterNumberInput()
        {
            AssertAllRequirements("abc123");
        }

        [TestMethod]
        public void SymbolInput()
        {
            AssertAllRequirements("abc+def%ghi&jkl");
        }

        [TestMethod]
        public void TypicalPathInput()
        {
            AssertAllRequirements("/folder/another folder/file name.c++");
        }

        [TestMethod]
        public void AllByteCharacterInput()
        {
            AssertAllRequirements(GetStringCharacterRange(0, 255));
        }

        [TestMethod]
        public void DoubleEncodeDecode()
        {
            var input = GetStringCharacterRange(30, 126); // Printable characters
            Assert.AreEqual(input, PathEncoder.Decode(PathEncoder.Decode(PathEncoder.Encode(PathEncoder.Encode(input)))));
        }

        private static void AssertAllRequirements(string input)
        {
            AssertDecodeOfEncodeMatches(input);
            AssertUrlDecodeDoesNotChange(input);
        }

        private static void AssertDecodeOfEncodeMatches(string input)
        {
            var encodedInput = PathEncoder.Encode(input);
            Assert.AreEqual(input, PathEncoder.Decode(encodedInput));
        }

        private static void AssertUrlDecodeDoesNotChange(string input)
        {
            var encodedInput = PathEncoder.Encode(input);
            var urlString = "http://localhost/" + encodedInput;
            Assert.AreEqual(urlString, HttpUtility.UrlDecode(urlString));
        }

        private static string GetStringCharacterRange(int min, int max)
        {
            return new string(Enumerable.Range(min, max - min + 1).Select(i => (char)i).ToArray());
        }
    }
}
