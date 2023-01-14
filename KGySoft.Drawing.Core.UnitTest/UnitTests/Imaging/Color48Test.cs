﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color48Test.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Color48Test
    {
        #region Methods

        [Test]
        public void ConversionTest()
        {
            Color32 c = Color32.FromArgb(0x11223344);

            Color48 c48 = new Color48(c);
            Assert.AreEqual(c.ToOpaque(), c48.ToColor32());
        }

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(6, sizeof(Color48));

            Color48* p = stackalloc Color48[2];
            Assert.AreEqual(6, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}