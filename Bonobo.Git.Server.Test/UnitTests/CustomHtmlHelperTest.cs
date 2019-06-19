﻿using System.ComponentModel.DataAnnotations;
using Bonobo.Git.Server.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.UnitTests
{
    [TestClass]
    public class CustomHtmlHelperTest
    {
        private readonly IHtmlHelper Html;

        [TestMethod]
        public void EnumsWithDisplayAttributesAreFormatted()
        {
            Assert.AreEqual("NameA", Html.DisplayEnum(EnumWithAttributes.A).ToString());
            Assert.AreEqual("NameB", Html.DisplayEnum(EnumWithAttributes.B).ToString());
        }

        [TestMethod]
        public void EnumsWithoutDisplayAttributesAreFormattedByFramework()
        {
            Assert.AreEqual("[[A]]", Html.DisplayEnum(EnumWithoutAttributes.A).ToString());
            Assert.AreEqual("[[B]]", Html.DisplayEnum(EnumWithoutAttributes.B).ToString());
        }

        [TestMethod]
        public void InvalidEnumIsFormattedByFramework()
        {
            Assert.AreEqual("[[7]]", Html.DisplayEnum((EnumWithoutAttributes)7).ToString());
        }

        enum EnumWithAttributes
        {
            [Display(Name = "NameA")]
            A,
            [Display(Name = "NameB")]
            B
        }
        enum EnumWithoutAttributes
        {
            A,
            B
        }
    }
}