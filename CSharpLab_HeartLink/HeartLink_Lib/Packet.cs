using System;
using System.IO;

namespace HeartLink_Lib
{
    public class Packet
    {

        public Header Header { get; set; }
        public byte[] Data { get; set; }

        public Packet()
        {
            Header = new Header();
            Data = new byte[DataParser.PACKETDATALENGTH];
        }

        public Packet(byte[] byteStream)
        {
            byte[] headerBytes = new byte[13];
            Buffer.BlockCopy(byteStream, 0, headerBytes, 0, 13);
            Header = new Header(headerBytes);
            Data = new byte[byteStream.Length - 13];
            Buffer.BlockCopy(byteStream, 13, Data, 0, Data.Length);
        }

        public byte[] ToBytes()
        {
            byte[] headerBytes = Header.ToBytes();
            byte[] output = new byte[Data.Length + headerBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, output, 0, headerBytes.Length);
            Buffer.BlockCopy(Data, 0, output, headerBytes.Length, Data.Length);
            return output;
        }

    }

    public class Header
    {

        public byte[] Index { get; set; }
        public byte[] Command { get; set; }
        public byte[] DataLength { get; set; }
        public byte Flag { get; set; }	//低端表示法。[0]-命令包(1)、返回包(0)；[1]-单包(置1时2号位无用)、多包(置0时2号位启用)；[2]-最后包(1)、非最后包(0)

        public Header()
        {
            Index = new byte[4];
            Command = new byte[4];
            DataLength = new byte[4];
            Flag = 0x00;
        }

        public Header(byte[] byteStream)
        {
            Index = new byte[4];
            Command = new byte[4];
            DataLength = new byte[4];
            Flag = 0x00;
            Buffer.BlockCopy(byteStream, 0, DataLength, 0, 4);
            Buffer.BlockCopy(byteStream, 4, Index, 0, 4);
            Buffer.BlockCopy(byteStream, 8, Command, 0, 4);
            Flag = byteStream[12];
        }

        public byte[] ToBytes()
        {
            byte[] output = new byte[13];
            Buffer.BlockCopy(DataLength, 0, output, 0, DataLength.Length);
            Buffer.BlockCopy(Index, 0, output, DataLength.Length, Index.Length);
            Buffer.BlockCopy(Command, 0, output, DataLength.Length + Index.Length, Command.Length);
            output[output.Length - 1] = Flag;
            return output;
        }

    }

}

