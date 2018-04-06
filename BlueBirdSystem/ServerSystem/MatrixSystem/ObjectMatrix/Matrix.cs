using System;
using System.Collections.Generic;

namespace MatrixSystem
{

    public class Matrix
    {
        private IMatrixItem[] items = new IMatrixItem[((int)MatrixTypes.MAX_VALUE)+1]; 
                          
        public T GetItem<T>(MatrixTypes type) where T : IMatrixItem
        {
            return (T)items[(int)type];
        }

        public void SetItem(IMatrixItem item)
        {
            items[(int)item.Type] = item;
        }

        internal Matrix Clone()
        {
            return new Matrix();
        }

        public List<byte[]> getBytes(uint itemMask)
        {
            List<byte[]> list = new List<byte[]>();
            for(int id = 0; id<items.Length; id++)
            {
                if (items[id] != null)
                {
                    if (((1 << id) & itemMask) != 0)
                    {
                        list.Add(new byte[] { (byte)id });
                        byte[] data = items[id].getBytes();
                        list.Add(data);
                    }
                }
            }
            list.Add(new byte[] { (byte)MatrixTypes.MAX_VALUE });
            return list;
        }

        public int setFrom(byte[] data, int startIndex)
        {
            while (data.Length > startIndex)
            {
                byte id = data[startIndex];
                if (id < (byte)MatrixTypes.MAX_VALUE)
                {
                    if (items[id] == null)
                        createEmptyItem(id);
                    startIndex = items[id].setFrom(data, startIndex + 1);
                }
                else
                    break;
            }
         
            return startIndex+1;
        }

        private void createEmptyItem(byte id)
        {
            throw new NotImplementedException();
        }
    }
}
