using Bonobo.Git.Server.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            AssertAllRequirements("abc+def%ghi&jkl/mno\\pqr#stu~vwx-yz");
        }

        [TestMethod]
        public void UnicodeInput()
        {
            // Sample characters from http://en.wikipedia.org/wiki/Unicode_and_HTML
            AssertAllRequirements("\u0041\u00df\u00fe\u0394\u017d\u0419\u05e7\u0645\u0e57\u1250\u3042\u53f6\u8449\ub5ab\u16a0\u0d37");
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

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void NonByteCharacterInput()
        {
            PathEncoder.Decode("\u0394");
        }

        [TestMethod]
        public void MissingNoNibbles()
        {
            Assert.AreEqual("\0", PathEncoder.Decode("~00"));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingOneNibble()
        {
            PathEncoder.Decode("~0");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingBothNibbles()
        {
            PathEncoder.Decode("~");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void BadNibbleValues()
        {
            PathEncoder.Decode("~no");
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
