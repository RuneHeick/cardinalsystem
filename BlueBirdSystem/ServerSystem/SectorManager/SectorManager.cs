using Sector;
using SectorController.Exceptions;
using SectorController.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SectorController
{
    public delegate void MsgToOtherSectorHandler(ISector sector, IMsg msg);
    public delegate void StatisticChangedHandler(SectorsStatistics statistics);

    public class SectorManager
    {
        private readonly ReaderWriterLockSlim _sectorLock = new ReaderWriterLockSlim();
        private Dictionary<ulong,SectorContainer> _sectors = new Dictionary<ulong, SectorContainer>();
        private SectorsStatistics _sectorsStatistics = new SectorsStatistics(); 

        public event MsgToOtherSectorHandler OnMsgToOther;
        public event StatisticChangedHandler OnStatisticChanged;
                
        public async void Add(ISector sector)
        {
            SectorContainer container = new SectorContainer(sector);
            await DiskCache.Save(container);
        }

        public async void Remove(ISector sector)
        {
            SectorContainer container = await GetSector(sector.Address);
            container.Sector.OnSectorStatusChanged -= Sector_OnSectorStatusChanged;
            container.Sector.OnSendMessage -= Sector_OnSendMessage;
            _sectors.Remove(container.Sector.Address);
            _sectorsStatistics.TotalSectors--;
            _sectorsStatistics.Decrement(container.Status);
        }

        public async void DoTick()
        {
            _sectorLock.EnterReadLock();
            try
            { 
                foreach (SectorContainer con in _sectors.Values)
                {
                    Task.Run(()=>con.Sector.Tick());
                }
            }
            finally
            {
                _sectorLock.ExitReadLock();
            }
        }

        public async void MessageFromOtherSector(IMsg msg)
        {
            SectorContainer container = await GetSector(msg.ToSector);
            container.Sector.MessageRecieved(msg);
        }

        private async void Sector_OnSectorStatusChanged(ISector sector, SectorStatus status)
        {
            SectorContainer container = await GetSector(sector.Address);
            _sectorsStatistics.Decrement(container.Status);
            container.Status = status;
            _sectorsStatistics.Incement(container.Status);
            Task.Run(() => OnStatisticChanged.Invoke(_sectorsStatistics));
        }

        private async void Sector_OnSendMessage(ISector sector, IMsg msg)
        {
            ISectorMsg secorMsg = msg as ISectorMsg;
            if (secorMsg != null)
            {
                    SectorContainer container = await GetSector(secorMsg.ToSector, true);
                    if(container != null)
                        Task.Run(() => container.Sector.MessageRecieved(msg));
                    else
                        Task.Run(() => OnMsgToOther?.Invoke(sector, msg));
            }
            else
            {
                Task.Run(() => OnMsgToOther?.Invoke(sector, msg));
            }
        }

        //-----------------------------------------------

        private async Task<SectorContainer> GetSector(ulong address, bool JustTry = false)
        {
            SectorContainer container;
            _sectorLock.EnterReadLock();
            try
            {
                if (_sectors.TryGetValue(address, out container))
                {
                    container.LastUsed = DateTime.Now;
                    return container;
                }
                else
                {
                    container = await TryLoadSector(address);
                    if (container != null)
                    {
                        container.LastUsed = DateTime.Now;
                        return container;
                    }

                    if (JustTry)
                        return null;
                    else
                        throw new SectorNotFoundException();
                }
            }
            finally
            {
                _sectorLock.ExitReadLock();
            }
        }

        private async Task StoreSector(SectorContainer container)
        {
            _sectorLock.EnterWriteLock();
            try
            {
                container.Sector.OnSectorStatusChanged -= Sector_OnSectorStatusChanged;
                container.Sector.OnSendMessage -= Sector_OnSendMessage;
                _sectors.Remove(container.Sector.Address);
                _sectorsStatistics.TotalSectors--;
                _sectorsStatistics.Decrement(container.Status);
                await DiskCache.Save(container);
            }
            finally
            {
                _sectorLock.ExitWriteLock();
            }
        }

        private async Task<SectorContainer> TryLoadSector(ulong address)
        {
            SectorContainer container = await DiskCache.Load(address);
            if (container != null)
            {
                _sectorLock.EnterWriteLock();
                try
                {
                    _sectors.Add(address, container);
                    container.Sector.OnSectorStatusChanged += Sector_OnSectorStatusChanged;
                    container.Sector.OnSendMessage += Sector_OnSendMessage;

                    _sectorsStatistics.TotalSectors++;
                    _sectorsStatistics.Incement(container.Status);
                    return container;
                }
                finally
                {
                    _sectorLock.ExitWriteLock();
                }
            }
            else
                return null;
        }

    }
}
