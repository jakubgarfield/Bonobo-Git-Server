using Bonobo.Git.Server.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Bonobo.Git.Server.Test.Unit
{
    [TestClass]
    public class PathEncoderTest
    {
        private static readonly Regex Rfc3986UnreservedCharacters = new Regex(@"^[A-Za-z0-9\-\._~]*$");
        private static readonly Regex Rfc3986UnreservedCharactersAndSlash = new Regex(@"^[A-Za-z0-9\-\._~/]*$");

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
        public void SlashEncodedByDefault()
        {
            Assert.AreEqual(0, PathEncoder.Encode("abc/def/ghi").Where(c => '/' == c).Count());
        }

        [TestMethod]
        public void AllowSlashTrue()
        {
            Assert.AreEqual(2, PathEncoder.Encode("abc/def/ghi", allowSlash: true).Where(c => '/' == c).Count());
        }

        [TestMethod]
        public void AllowSlashFalse()
        {
            Assert.AreEqual(0, PathEncoder.Encode("abc/def/ghi", allowSlash: false).Where(c => '/' == c).Count());
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
            AssertEncodeUsesOnlyIntendedCharacters(input);
            AssertUrlDecodeDoesNotChange(input);
        }

        private static void AssertDecodeOfEncodeMatches(string input)
        {
            var encodedInput = PathEncoder.Encode(input);
            Assert.AreEqual(input, PathEncoder.Decode(encodedInput));
            encodedInput = PathEncoder.Encode(input, allowSlash: true);
            Assert.AreEqual(input, PathEncoder.Decode(encodedInput));
            encodedInput = PathEncoder.Encode(input, allowSlash: false);
            Assert.AreEqual(input, PathEncoder.Decode(encodedInput));
        }

        private static void AssertEncodeUsesOnlyIntendedCharacters(string input)
        {
            var encodedInput = PathEncoder.Encode(input);
            Assert.IsTrue(Rfc3986UnreservedCharacters.IsMatch(encodedInput ?? ""));
            encodedInput = PathEncoder.Encode(input, allowSlash: true);
            Assert.IsTrue(Rfc3986UnreservedCharactersAndSlash.IsMatch(encodedInput ?? ""));
            encodedInput = PathEncoder.Encode(input, allowSlash: false);
            Assert.IsTrue(Rfc3986UnreservedCharacters.IsMatch(encodedInput ?? ""));
        }

        private static void AssertUrlDecodeDoesNotChange(string input)
        {
            var encodedInput = PathEncoder.Encode(input);
            var urlString = "http://localhost/" + encodedInput;
            Assert.AreEqual(urlString, HttpUtility.UrlDecode(urlString));
            encodedInput = PathEncoder.Encode(input, allowSlash: true);
            urlString = "http://localhost/" + encodedInput;
            Assert.AreEqual(urlString, HttpUtility.UrlDecode(urlString));
            encodedInput = PathEncoder.Encode(input, allowSlash: false);
            urlString = "http://localhost/" + encodedInput;
            Assert.AreEqual(urlString, HttpUtility.UrlDecode(urlString));
        }

        private static string GetStringCharacterRange(int min, int max)
        {
            return new string(Enumerable.Range(min, max - min + 1).Select(i => (char)i).ToArray());
        }
    }
}
