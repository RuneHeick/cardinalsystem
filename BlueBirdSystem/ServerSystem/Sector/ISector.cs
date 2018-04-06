using System;
using System.Collections.Generic;
using System.Text;

namespace Sector
{
    public interface ISector
    {

        ulong Address { get; }

        event SectorStatusChanged OnSectorStatusChanged;


        #region SectorLoadSave

        /// <summary>
        /// Save all the data in the sector, used to move and store the sector. 
        /// </summary>
        /// <returns>ByteArray That can store the inner state of the sector</returns>
        byte[] Save();

        /// <summary>
        /// Can populate the sector with data from the save.
        /// </summary>
        /// <param name="data"></param>
        void Load(byte[] data);

        #endregion


        #region MessageHandling

        event SendMsgHandler OnSendMessage;

        void MessageRecieved(IMsg msg);

        void ClientOffline(string clientAddress);

        #endregion


        void Tick();

    }
}
