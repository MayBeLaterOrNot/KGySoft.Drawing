﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IWritableBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a writable <see cref="IBitmapData"/> instance.
    /// To create an instance use the <see cref="BitmapDataFactory"/> class or the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetWritableBitmapData.htm" target="_blank">BitmapExtensions.GetWritableBitmapData</a> extension method.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a> method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    /// <seealso cref="BitmapDataFactory"/>
    public interface IWritableBitmapData : IBitmapData
    {
        #region Properties and Indexers

        #region Properties

        /// <summary>
        /// Gets an <see cref="IWritableBitmapDataRow"/> instance representing the first row of the current <see cref="IWritableBitmapData"/>.
        /// Subsequent rows can be accessed by calling the <see cref="IBitmapDataRow.MoveNextRow">MoveNextRow</see> method on the returned instance
        /// while it returns <see langword="true"/>. Alternatively, you can use the <see cref="this">indexer</see> to obtain any row.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        IWritableBitmapDataRow FirstRow { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets an <see cref="IWritableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IWritableBitmapData"/>.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <param name="y">The y-coordinate of the row to obtain.</param>
        /// <returns>An <see cref="IWritableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IWritableBitmapData"/>.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        IWritableBitmapDataRow this[int y] { get; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color"/> structure that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>If multiple pixels need to be set process the bitmap line by line for better performance.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRow"/> members and then moving to the next line by the <see cref="IBitmapDataRow.MoveNextRow">MoveNextRow</see> property.</para>
        /// <para>The <paramref name="color"/> argument represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="KnownPixelFormat"/>. To access the actual <see cref="KnownPixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set is not supported by owner <see cref="IReadWriteBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        void SetPixel(int x, int y, Color color);

        #endregion
    }
}
