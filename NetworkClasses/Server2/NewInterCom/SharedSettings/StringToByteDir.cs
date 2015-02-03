using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.NewInterCom.SharedSettings
{
    public class StringToByteDir: ISetting
    {

        private List<int> Lengths = new List<int>(); 

        public int Count
        {
            get
            {
                return Lengths.Count; 
            }
        }

        public StringToByteDir()
        {
            OnDataChanged += StringToByteDir_OnDataChanged;
        }

        void StringToByteDir_OnDataChanged(ISetting obj)
        {
            Lengths.Clear();
            int StartIndex = 0; 
            for (int i = 0; i < Data.Length; i++ )
            {
                int endindex = Array.IndexOf<byte>(Data, 0, i);
                Lengths.Add(endindex - StartIndex);
                StartIndex = endindex + 1;
                i = endindex;
            }
        }

        private void setData(byte[] data)
        {
            OnDataChanged -= StringToByteDir_OnDataChanged;
            Data = data;
            OnDataChanged += StringToByteDir_OnDataChanged;
        }

        public byte this[string Name]
        {
            get
            {
                byte[] nameb = UTF8Encoding.UTF8.GetBytes(Name);
                int index = 0; 
                for(int i = 0; i<Lengths.Count; i++)
                {
                    if (Lengths[i] == nameb.Length)
                    {
                        if (isMatch(Data, index, nameb, 0, nameb.Length))
                            return (byte)i; 
                    }

                    index += Lengths[i]+1; 
                }
                return 255; 
            }
        }


        public string this[byte id]
        {
            get
            {
                int index = 0;
                for (int i = 0; i < Lengths.Count; i++)
                {
                    if (i == id)
                    {
                        return UTF8Encoding.UTF8.GetString(Data, index, Lengths[i]);
                    }

                    index += Lengths[i] + 1;
                }
                return "";
            }
        }

        private static bool isMatch(byte[] one, int oneStartIndex, byte[] two, int twoStartIndex, int count)
        {
            if (one.Length - oneStartIndex < count || two.Length - twoStartIndex < count)
                return false; 
            for(int i = 0; i<count; i++)
            {
                if (one[i + oneStartIndex] != two[i + twoStartIndex])
                    return false;
            }
            return true; 
        }

        public void Add(string Name)
        {

            if (this[Name]==255)
            {
                var data = new byte[Data.Length+Name.Length+1]; 
                Array.Copy(Data,data,Data.Length);
                UTF8Encoding.UTF8.GetBytes(Name, 0, Name.Length, data, Data.Length);
                data[data.Length - 1] = 0;
                Lengths.Add(Name.Length);
                setData(data); 
            }
        }


        private static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;


            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
                i = Array.IndexOf<byte>(arrayToSearchThrough, 0, i)+1;
            }
            return -1;
        }


    }
}
