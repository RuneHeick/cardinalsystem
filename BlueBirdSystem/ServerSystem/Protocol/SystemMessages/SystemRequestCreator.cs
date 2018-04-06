using Networking.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.SystemMessages
{
    public static class SystemRequestCreator
    {

        public static ComposedMessage CreateMergeRequest(ulong sector)
        {
            ComposedMessage composed = new ComposedMessage();
            byte[] packet = new byte[1+1+8];
            packet[0] = (byte)NetworkMessageType.SECTOR_CONTROLLER_MESSAGE;
            packet[1] = (byte)SystemMessageType.MERGING_REQUEST;
            Array.Copy(BitConverter.GetBytes(sector),0, packet, 2, 8);
            return composed;
        }

        public static ComposedMessage CreateMergeAccept(ulong sector)
        {
            ComposedMessage composed = new ComposedMessage();
            byte[] packet = new byte[1 + 1 + 8];
            packet[0] = (byte)NetworkMessageType.SECTOR_CONTROLLER_MESSAGE;
            packet[1] = (byte)SystemMessageType.MERGING_ACCEPTED;
            Array.Copy(BitConverter.GetBytes(sector), 0, packet, 2, 8);
            composed.Add(packet);
            return composed;
        }

        public static ComposedMessage CreateMergeDeny(ulong sector)
        {
            ComposedMessage composed = new ComposedMessage();
            byte[] packet = new byte[1 + 1 + 8];
            packet[0] = (byte)NetworkMessageType.SECTOR_CONTROLLER_MESSAGE;
            packet[1] = (byte)SystemMessageType.MERGING_DENIDE;
            Array.Copy(BitConverter.GetBytes(sector), 0, packet, 2, 8);
            composed.Add(packet);
            return composed;
        }

        public static ComposedMessage CreateMergeCompleteOK(ulong sector)
        {
            ComposedMessage composed = new ComposedMessage();
            byte[] packet = new byte[1 + 1 + 8];
            packet[0] = (byte)NetworkMessageType.SECTOR_CONTROLLER_MESSAGE;
            packet[1] = (byte)SystemMessageType.MERGING_COMPLETE_OK;
            Array.Copy(BitConverter.GetBytes(sector), 0, packet, 2, 8);
            composed.Add(packet);
            return composed;
        }

        public static ComposedMessage CreateMergeError(byte[] data)
        {
            ComposedMessage composed = new ComposedMessage();
            byte[] packet = new byte[1 + 1];
            packet[0] = (byte)NetworkMessageType.SECTOR_CONTROLLER_MESSAGE;
            packet[1] = (byte)SystemMessageType.MERGING_DATA;
            composed.Add(packet);
            composed.Add(data);
            return composed;
        }


    }
}
