using System;
using System.Text;

namespace HeartLink_Lib
{
    public class DataParser
    {

        public const int PACKETDATALENGTH = 512 * 1024; // 512KBytes

        /// <summary>
        /// 组装数据包
        /// </summary>
        /// <param name="command">命令码</param>
        /// <param name="data">数据字节流</param>
        /// <returns>数据包数组，长度通常为1，若1个数据包放不下所有数据，则将拆成更多个数据包</returns>
        public static Packet[] Bytes2Packets(int command, byte[] data)
        {
            byte[] commandByte = Int2Bytes(command);
            byte[] dataLength = null;
            int count = data.Length / PACKETDATALENGTH + 1;
            Packet[] packets = new Packet[count];
            byte flag = 0;
            if (count == 1)
            {
                flag = 0x07; // 00000111 命令、单包、最后
            }
            else
            {
                flag = 0x01; // 00000001 命令、多包、非最后
            }
            for (int i = 0; i < count; ++i)
            {
                byte[] packetData = null;
                if (i != count - 1)
                {
                    packetData = new byte[PACKETDATALENGTH];
                    Buffer.BlockCopy(data, i * PACKETDATALENGTH, packetData, 0, PACKETDATALENGTH);
                    dataLength = Int2Bytes(PACKETDATALENGTH);
                }
                else
                {
                    packetData = new byte[data.Length % PACKETDATALENGTH]; // 去掉不必要的冗余
                    Buffer.BlockCopy(data, i * PACKETDATALENGTH, packetData, 0, data.Length % PACKETDATALENGTH);
                    dataLength = Int2Bytes(data.Length % PACKETDATALENGTH);
                    flag = (byte)(flag | 0x05); // 00000101 命令、多包、最后
                }
                Header header = new Header();
                header.Index = Int2Bytes(i * PACKETDATALENGTH);
                header.Command = commandByte;
                header.DataLength = dataLength;
                header.Flag = flag;

                Packet p = new Packet();
                p.Header = header;
                p.Data = packetData;

                packets[i] = p;
            }
            return packets;
        }

        /// <summary>
        /// 组装数据包
        /// </summary>
        /// <param name="command">命令码</param>
        /// <param name="jsonStr">数据字符串</param>
        /// <returns>数据包数组，长度通常为1，若1个数据包放不下所有数据，则将拆成更多个数据包</returns>
        public static Packet[] Str2Packets(int command, String Str)
        {
            return Bytes2Packets(command, Encoding.UTF8.GetBytes(Str));
        }

        /// <summary>
        /// 从字节流提取数据包中的字节流数据
        /// </summary>
        /// <param name="packets">数据包数组</param>
        /// <returns>数据包数据字节流</returns>
        public static byte[] Packets2Bytes(Packet[] packets)
        {
            int packetCount = packets.Length;
            int bytesLength = (packetCount - 1) * PACKETDATALENGTH
                + Bytes2Int(packets[packetCount - 1].Header.DataLength);
            byte[] output = new byte[bytesLength];
            for (int i = 0; i < packetCount; ++i)
            {
                Buffer.BlockCopy(packets[i].Data, 0, output, i * PACKETDATALENGTH,
                    Bytes2Int(packets[i].Header.DataLength));
            }
            return output;
        }

        /// <summary>
        /// 从字节流提取数据包中的Json数据字符串
        /// </summary>
        /// <param name="packets">数据包数组</param>
        /// <returns>数据包数据字符串</returns>
        public static String Packets2Str(Packet[] packets)
        {
            byte[] incoming = Packets2Bytes(packets);
            return Encoding.UTF8.GetString(incoming, 0, incoming.Length);
        }

        /// <summary>
        /// 得到数据包命令码
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>命令码</returns>
        public static int GetPacketCommandCode(Packet packet)
        {
            return Bytes2Int(packet.Header.Command);
        }

        /// <summary>
        /// 判断该包是否为最后一个包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static bool IsPacketLast(Packet packet)
        {
            if (((int)packet.Header.Flag & 0x04) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 将一个整型数以低端表示方式转变为一个字节数组
        /// </summary>
        /// <param name="input">整型数</param>
        /// <returns></returns>
        public static byte[] Int2Bytes(int input)
        {
            byte[] output = new byte[4];
            for (int i = 0; i < 4; ++i)
            {
                output[i] = (byte)(input & 0xff);
                input = input >> 8;
            }
            return output;
        }

        /// <summary>
        /// 将一个字节数组以低端表示方式解析为一个整型数
        /// </summary>
        /// <param name="input">长度为4的字节数组</param>
        /// <returns></returns>
        public static int Bytes2Int(byte[] input)
        {
            int output = 0;
            for (int i = 3; i >= 0; --i)
            {
                output = output << 8;
                output += input[i];
            }
            return output;
        }

    }
}

