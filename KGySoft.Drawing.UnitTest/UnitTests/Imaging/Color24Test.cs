﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color24Test.cs
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

using System.Drawing;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Color24Test
    {
        #region Methods

        [Test]
        public void ConversionTest()
        {
            Color32 c = Color32.FromArgb(0x11223344);

            Color24 c24 = new Color24(c);
            Assert.AreEqual(Color32.FromArgb(255, c), c24.ToColor32());
        }

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(3, sizeof(Color24));

            Color24* p = stackalloc Color24[2];
            Assert.AreEqual(3, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}