using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.SharedFile.Files
{
    class SystemFileIndexFile:BaseFile
    {

        List<SystemFileInfo> systemFileInfosfileInfos = new List<SystemFileInfo>();


        public override byte[] Data
        {
            get { return ToByte(); }
            set { FromByte(value); }
        }

        private void FromByte(byte[] value)
        {
            throw new NotImplementedException();
        }

        private byte[] ToByte()
        {
            throw new NotImplementedException();
        }


        public class SystemFileInfo
        {
            string Name { get; set; }
            UInt32 Hash { get; set; }
        }
    }
}
