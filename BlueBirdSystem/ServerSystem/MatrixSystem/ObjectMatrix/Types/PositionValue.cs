using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem.ObjectMatrix.Types
{
    public class PositionValue : IMatrixItem
    {
        public PublicSyncTypes Type => PublicSyncTypes.POSITION;

        public double X {
            get;
            set;
        }

        public double Y { get; set; }

        public byte[] getBytes()
        {
            byte[] xdata = BitConverter.GetBytes(X);
            byte[] ydata = BitConverter.GetBytes(Y);
            byte[] data = new byte[xdata.Length + ydata.Length];
            Array.Copy(xdata, 0, data, 0, xdata.Length);
            Array.Copy(ydata, 0, data, xdata.Length, ydata.Length);
            return data;
        }

        public int setFrom(byte[] data, int startIndex)
        {
            X = BitConverter.ToSingle(data, startIndex);
            Y = BitConverter.ToSingle(data, startIndex+4);
            return startIndex + 8; 
        }
    }
}
