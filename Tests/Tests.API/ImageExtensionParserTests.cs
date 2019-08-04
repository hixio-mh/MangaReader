﻿using MangaReader.Core.Services;
using NUnit.Framework;

namespace Tests.API
{
  public class ImageExtensionParserTests
  {
    private const string DefaultValue = "none";

    [Test]
    [TestCase(new byte[] { 0x42, 0x4d, 0xb6, 0x51, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00 }, ImageExtensionParser.Bmp)]
    [TestCase(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x0a, 0x03, 0x90, 0x01, 0xf7, 0xff, 0x00, 0xe8 }, ImageExtensionParser.Gif)]
    [TestCase(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48 }, ImageExtensionParser.Png)]
    [TestCase(new byte[] { 0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x02 }, ImageExtensionParser.Jpeg)]
    [TestCase(new byte[] { 0x49, 0x49, 0x2a, 0x00, 0xc8, 0x46, 0x00, 0x00, 0xff, 0xff, 0x3f, 0x2d, 0x0d, 0x07 }, ImageExtensionParser.Tiff)]
    [TestCase(new byte[] { 0x4d, 0x4d, 0x00, 0x2a, 0x00, 0x00, 0x00, 0x08, 0x00, 0x10, 0x00, 0xfe, 0x00, 0x04 }, ImageExtensionParser.Tiff)]
    [TestCase(new byte[] { 0x44 }, DefaultValue)]
    [TestCase(new byte[] { 0x42, 0x49, 0x4e, 0xe0, 0xc8 }, DefaultValue)]
    public void Parse(byte[] body, string extension)
    {
      var parsed = ImageExtensionParser.Parse(body, DefaultValue);
      Assert.AreEqual(extension, parsed);
    }
  }
}
