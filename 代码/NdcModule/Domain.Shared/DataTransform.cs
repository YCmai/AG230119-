namespace AciModule.Domain.Shared
{
    /// <summary>
    /// 数据转换类，包括了字节转int,int转字节
    /// </summary>
    static class DataTransform
    {
        public static int BytesToUInt8(byte[] buffer, int index)
        {
            if (index + 1 > buffer.Length) return 0;

            return buffer[index];
        }

        public static bool UInt8ToBytes(int data, byte[] buffer, int index)
        {
            if (index + 1 > buffer.Length) return false;

            buffer[index] = (byte)(data & 0xFF);
            return true;
        }
        public static bool UInt16ToReverseBytes(int data, byte[] buffer, int index)
        {
            if (index + 2 > buffer.Length) return false;

            buffer[index] = (byte)(data & 0xFF);
            buffer[index + 1] = (byte)((data >> 8) & 0xFF);
            return true;
        }

        public static int BytesToUInt16(byte[] buffer, int index)
        {
            if (index + 2 > buffer.Length) return 0;

            return (int)buffer[index] * 0x100 + (int)buffer[index + 1];
        }
        public static int BytesToReverseUInt16(byte[] buffer, int index)
        {
            if (index + 2 > buffer.Length) return 0;

            return (int)buffer[index + 1] * 0x100 + (int)buffer[index];
        }

        public static bool UInt16ToBytes(int data, byte[] buffer, int index)
        {
            if (index + 2 > buffer.Length) return false;

            buffer[index] = (byte)((data >> 8) & 0xFF);
            buffer[index + 1] = (byte)(data & 0xFF);
            return true;
        }

        public static uint BytesToUInt32(byte[] buffer, int index)
        {
            if (index + 4 > buffer.Length) return 0;

            return (uint)buffer[index] * 0x1000000 + (uint)buffer[index + 1] * 0x10000 + (uint)buffer[index + 2] * 0x100 + (uint)buffer[index + 3];
        }

        public static bool UInt32ToBytes(uint data, byte[] buffer, int index)
        {
            if (index + 4 > buffer.Length) return false;

            buffer[index] = (byte)((data >> 24) & 0xFF);
            buffer[index + 1] = (byte)((data >> 16) & 0xFF);
            buffer[index + 2] = (byte)((data >> 8) & 0xFF);
            buffer[index + 3] = (byte)(data & 0xFF);
            return true;
        }
    }
}
