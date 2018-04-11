using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem
{
    public interface IMatrixItem
    {
        PublicSyncTypes Type { get; }

        byte[] getBytes();

        /// <summary>
        /// Set data from input bytes.
        /// </summary>
        /// <param name="data">data stream</param>
        /// <param name="startIndex">start index</param>
        /// <returns>The new index in the stream</returns>
        int setFrom(byte[] data, int startIndex);

    }
}
