using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol
{
    enum PacketType { SimpleMessage = 1, ClientList = 2, Login = 3 }

    // класс для упаковки массива строк в массив байт - для отправки 
    // и для обратной распаковки массива байт в массив строк - для получения
    // см. protocol.txt
    class Packet
    {
        private PacketType pt;

        // длины строк
        private int[] anLen;
        // строки
        private string[] asData;

        public Packet(PacketType _pt = PacketType.SimpleMessage, int nLen = 2)
        {
            pt = _pt;
            switch (pt)
            {
                case PacketType.SimpleMessage: nLen = 2; break;
                case PacketType.Login: nLen = 1; break;
            }
            if (nLen > 0) asData = new string[nLen];
        }

        // property = pair of functions { get(), set() }
        public PacketType Type
        {
            get { return pt; }
            set { pt = value; }
        }
        // пара функций {get, set} в стиле C++ - аналог property 
        public PacketType getType() { return pt; }
        public void setType(PacketType _value) { pt = _value; }


        public int ItemCount
        {
            get
            {
                // null - пустой указатель
                //if (asData != null) return asData.Length;
                //else return 0;
                return (asData != null) ? asData.Length : 0;
            }
            set
            {
                if (value <= 0) return;
                if (ItemCount == value) return;
                asData = new string[value];
            }
        }

        private static int CalcHeaderSize(int nItemCount)
        {
            int nSize = 0;
            // тип пакета
            nSize += 1;
            // кол-во строк
            nSize += 2;
            int N = nItemCount;
            // длины строк
            nSize += 2 * N;
            return nSize;
        }

        public int HeaderSize
        {
            get { return CalcHeaderSize(ItemCount); }
        }

        //public int Size
        //{
        //	get
        //	{
        //		int nSize = HeaderSize;
        //		if (nSize == 0) return 0;

        //		int N = ItemCount;
        //		// длины строк (для ASCII)
        //		for (int i = 0; i < N; i++)
        //			nSize += asData[i].Length;
        //		return nSize;
        //	}
        //}

        public int LoadHeader(byte[] abBuffer, int nShift = 0)
        {
            if (abBuffer.Length < nShift + 3) return 0;

            // тип пакета
            pt = (PacketType)abBuffer[nShift + 0];
            // кол-во строк
            int N = BitConverter.ToUInt16(abBuffer, nShift + 1);

            if (abBuffer.Length < nShift + 3 + 2 * N) return 0;

            asData = new string[N];
            anLen = new int[N];
            int nSize = HeaderSize;
            for (int i = 0; i < N; i++)
            {
                anLen[i] = BitConverter.ToUInt16(abBuffer, nShift + 3 + 2 * i);
                nSize += anLen[i];
            }
            return nSize;
        }

        public byte[] ToBytes()
        {
            // размер пакета (байты)
            int nSize = HeaderSize;
            // кол-во строк
            UInt16 N = (ushort)ItemCount;
            anLen = new int[N];

            // массивы байт для каждой строки
            byte[][] abStr = new byte[N][];
            for (int i = 0; i < N; i++)
            {
                // байты i-строки
                abStr[i] = Str2Bytes(asData[i]);
                // длина i-строки (в байтах)
                anLen[i] = abStr[i].Length;
                nSize += anLen[i];
            }

            // итоговый буфер
            byte[] abBuffer = new byte[nSize];

            byte[] ab;
            int iPos = 0;

            // наполняем буфер данными

            // тип пакета
            abBuffer[iPos] = (byte)pt;
            iPos += 1;

            // кол-во строк
            ab = BitConverter.GetBytes(N);
            ab.CopyTo(abBuffer, iPos);
            iPos += ab.Length;

            // длины строк
            for (int i = 0; i < N; i++)
            {
                ab = BitConverter.GetBytes((ushort)anLen[i]);
                ab.CopyTo(abBuffer, iPos);
                iPos += ab.Length;
            }
            // строки
            for (int i = 0; i < N; i++)
            {
                abStr[i].CopyTo(abBuffer, iPos);
                iPos += anLen[i];
            }

            return abBuffer;
        }

        public void FromBytes(byte[] abIn, int nShift = 0, bool bLoadHeader = false)
        {
            if (bLoadHeader && LoadHeader(abIn, nShift) == 0) throw new Exception("Insufficient data to load a header");
            int N = ItemCount;
            int iPos = nShift + HeaderSize;
            for (int i = 0; i < N; i++)
            {
                asData[i] = Bytes2Str(abIn, iPos, anLen[i]);
                iPos += anLen[i];
            }
        }
        // строим новый пакет из массива байт
        public static Packet ParseBytes(byte[] bytes, int nShift = 0)
        {
            Packet packet = new Packet();
            packet.FromBytes(bytes, nShift, true);
            return packet;
        }
        public string[] Items
        {
            get { return asData; }
            set { asData = value; }
        }

        public string GetItem(int i)
        {
            if (i < 0 || i >= ItemCount) return "";
            return asData[i];
        }

        public void SetItem(int i, string sValue)
        {
            if (i < 0 || i >= ItemCount) return;
            asData[i] = sValue;
        }

        public void FromItemList(List<string> listItems)
        {
            if (ItemCount != listItems.Count)
                asData = new string[listItems.Count];
            int N = ItemCount;
            for (int i = 0; i < N; i++)
                asData[i] = listItems[i];
        }

        private static byte[] Str2Bytes(string s)
        {
            // ASCII
            //return Encoding.GetEncoding(1251).GetBytes(s);
            // Unicode
            return Encoding.Unicode.GetBytes(s);
        }
        private static string Bytes2Str(byte[] bytes, int nIndex, int nCount)
        {
            // ASCII
            //return Encoding.GetEncoding(1251).GetString(bytes, nIndex, nCount);
            // Unicode
            return Encoding.Unicode.GetString(bytes, nIndex, nCount);
        }
    }

}