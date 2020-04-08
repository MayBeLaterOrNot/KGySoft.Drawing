﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IconExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class IconExtensionsTest : TestBase
    {
        #region Methods

        [Test]
        public void ToAlphaBitmapTest()
        {
            using var bmp = SystemIcons.Information.ToBitmap();
            using var abmp = SystemIcons.Information.ToAlphaBitmap();
            if (OSUtils.IsWindows)
                Assert.IsFalse(bmp.EqualsByContent(abmp));
            SaveImage(nameof(Icon.ToBitmap), bmp);
            SaveImage(nameof(IconExtensions.ToAlphaBitmap), abmp);
        }

        [Test]
        public void ToMultiResBitmapTest()
        {
            Assert.AreEqual(OSUtils.IsWindows ? 7 : 1, Icons.Information.ToMultiResBitmap().ExtractBitmaps().Length);

            // 128x128 PNG compressed icons are problematic even in Windows
            var reqSize = new Size(128, 128);
            Icon origIcon = Icons.Information;
            Icon combined = origIcon.Combine(origIcon.ExtractBitmap(new Size(256, 256)).Resize(reqSize));
            Bitmap multiRes = combined.ToMultiResBitmap();

            // ToMultiResBitmap does not use compressed icons anymore: it just caused problems and doesn't even matter as a Bitmap
            Assert.AreEqual(OSUtils.IsWindows ? 8 : 1, multiRes.ExtractBitmaps().Length);
        }

        [Test]
        public void GetImagesCountTest()
        {
            Assert.AreEqual(7, Icons.Information.GetImagesCount());
        }

        [Test]
        public void ExtractBitmapsTest()
        {
            Assert.AreEqual(7, Icons.Information.ExtractBitmaps().Length);
            Assert.AreEqual(1, Icons.Information.ExtractBitmaps(new Size(16, 16)).Length);
            Assert.AreEqual(0, Icons.Information.ExtractBitmaps(Size.Empty).Length);
            Assert.AreEqual(7, Icons.Information.ExtractBitmaps(PixelFormat.Format32bppArgb).Length);
        }

        [Test]
        public void ExtractBitmapTest()
        {
            Assert.IsNotNull(Icons.Information.ExtractBitmap());
            Assert.IsNotNull(Icons.Information.ExtractBitmap(new Size(16, 16)));
            Assert.IsNotNull(Icons.Information.ExtractBitmap(new Size(16, 16), PixelFormat.Format32bppArgb));
            Assert.IsNull(Icons.Information.ExtractBitmap(Size.Empty));
            Assert.IsNull(Icons.Information.ExtractBitmap(new Size(16, 16), PixelFormat.Format1bppIndexed));
            Assert.IsNotNull(Icons.Information.ExtractBitmap(0));
            Assert.IsNull(Icons.Information.ExtractBitmap(99));
        }

        [Test]
        public void ExtractNearestBitmapTest()
        {
            Assert.IsNotNull(Icons.Information.ExtractNearestBitmap(Size.Empty, PixelFormat.Format1bppIndexed));
            Assert.AreEqual(64, Icons.Information.ExtractNearestBitmap(new Size(64, 64), PixelFormat.Format1bppIndexed).Width);
        }

        [Test]
        public void ExtractIconsTest()
        {
            Assert.AreEqual(7, Icons.Information.ExtractIcons().Length);
            Assert.AreEqual(1, Icons.Information.ExtractIcons(new Size(16, 16)).Length);
            Assert.AreEqual(0, Icons.Information.ExtractIcons(Size.Empty).Length);
            Assert.AreEqual(7, Icons.Information.ExtractIcons(PixelFormat.Format32bppArgb).Length);
        }

        [Test]
        public void ExtractIconTest()
        {
            Assert.IsNotNull(Icons.Information.ExtractIcon(new Size(16, 16)));
            Assert.IsNotNull(Icons.Information.ExtractIcon(new Size(16, 16), PixelFormat.Format32bppArgb));
            Assert.IsNull(Icons.Information.ExtractIcon(Size.Empty));
            Assert.IsNull(Icons.Information.ExtractIcon(new Size(16, 16), PixelFormat.Format1bppIndexed));
            Assert.IsNotNull(Icons.Information.ExtractIcon(1));
            Assert.IsNull(Icons.Information.ExtractIcon(99));
        }

        [Test]
        public void ExtractNearestIconTest()
        {
            Assert.IsNotNull(Icons.Information.ExtractNearestIcon(Size.Empty, PixelFormat.Format1bppIndexed));
            Assert.AreEqual(OSUtils.IsWindows ? 256 : 64, Icons.Information.ExtractNearestIcon(new Size(256, 256), PixelFormat.Format1bppIndexed).Width);
        }

        [TestCase(512)]
        [TestCase(128)]
        [TestCase(30)]
        [TestCase(8)]
        public void ExtractIconBySizeTest(int newSize)
        {
            var reqSize = new Size(newSize, newSize);
            Icon origIcon = Icons.Information;
            Icon combined = origIcon.Combine(origIcon.ExtractBitmap(new Size(256, 256)).Resize(reqSize));
            SaveIcon($"Combined{newSize}", combined);

            Icon extracted = combined.ExtractIcon(reqSize);
            SaveIcon($"Extracted{newSize}", extracted);

            Assert.IsNotNull(extracted);
            Assert.AreEqual(reqSize, extracted.Size);
            Assert.AreEqual(extracted.IsCompressed(), newSize >= RawIcon.MinCompressedSize && OSUtils.IsVistaOrLater);
        }

        [Test]
        public void CombineTest()
        {
            Assert.AreEqual(2, SystemIcons.Information.Combine(SystemIcons.Application).GetImagesCount());
        }

        [Test]
        public void IsCompressedTest()
        {
            Assert.IsFalse(Icons.Information.ExtractIcon(new Size(16, 16)).IsCompressed());
            Assert.IsTrue(OSUtils.IsXpOrEarlier || Icons.Information.IsCompressed());
            Assert.IsFalse(Icons.Information.ToUncompressedIcon().IsCompressed());
            
            // On Linux extracting a standalone 256x256 icon may fail both in BMP and PNG format...
            Assert.IsTrue(OSUtils.IsXpOrEarlier || Icons.Information.ExtractIcon(new Size(256, 256))?.IsCompressed() == true || !OSUtils.IsWindows);
        }

        [Test]
        public void GetBitsPerPixelTest()
        {
            Assert.AreEqual(32, Icons.Information.GetBitsPerPixel());
        }

        [Test]
        public void ToCursorHandleTest()
        {
            AssertPlatformDependent(() => Assert.AreNotEqual(IntPtr.Zero, (IntPtr)Icons.Information.ToCursorHandle()), PlatformID.Win32NT);
        }

        #endregion
    }
}
