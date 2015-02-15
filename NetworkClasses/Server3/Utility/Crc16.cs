using System;

/// <summary>
/// Creates a checksum as a ushort / UInt16.
/// </summary>
public class Crc16
{
    const ushort polynomial = 0xA001;
    ushort[] table = new ushort[256];

    ushort crc = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Crc16"/> class.
    /// </summary>
    public Crc16()
    {
        ushort value;
        ushort temp;
        for (ushort i = 0; i < table.Length; ++i)
        {
            value = 0;
            temp = i;
            for (byte j = 0; j < 8; ++j)
            {
                if (((value ^ temp) & 0x0001) != 0)
                {
                    value = (ushort)((value >> 1) ^ polynomial);
                }
                else
                {
                    value >>= 1;
                }
                temp >>= 1;
            }
            table[i] = value;
        }
    }

    /// <summary>
    /// Computes the checksum.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns>The checkum.</returns>
    public ushort addBytes(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; ++i)
        {
            byte index = (byte)(crc ^ bytes[i]);
            crc = (ushort)((crc >> 8) ^ table[index]);
        }
        return crc;
    }

    public void Reset()
    { crc = 0;  }
}