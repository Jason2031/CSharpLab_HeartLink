using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Foundation;
using HeartLink_Lib;

namespace HeartLink_Client
{
    class StaticObj
    {
        public static Person user = null;
        public static DataReader streamReader;
        public static DataWriter streamWriter;
        public static StreamSocket s = null;
        public static HostName hostName = new HostName("127.0.0.1");

        public async static void SendPackets(Packet[] packets)
        {
            foreach (Packet p in packets)
            {
                StaticObj.streamWriter.WriteBytes(p.ToBytes());
            }
            await streamWriter.StoreAsync();
        }

        public static async Task<Packet[]> ReceivePackets()
        {
            IAsyncOperation<uint> taskLoad = streamReader.LoadAsync(sizeof(int));
            taskLoad.AsTask().Wait();
            //await streamReader.LoadAsync(sizeof(int));
            byte[] temp = new byte[sizeof(int)];
            streamReader.ReadBytes(temp);
            int firstPacketSize = DataParser.Bytes2Int(temp);

            int leftBytes = firstPacketSize + 9;
            await streamReader.LoadAsync((uint)leftBytes);
            byte[] incompleteByteArray = new byte[leftBytes];
            streamReader.ReadBytes(incompleteByteArray);
            byte[] completeByteArray = new byte[firstPacketSize + 13];
            System.Buffer.BlockCopy(DataParser.Int2Bytes(firstPacketSize), 0, completeByteArray, 0, sizeof(int));
            System.Buffer.BlockCopy(incompleteByteArray, 0, completeByteArray, sizeof(int), incompleteByteArray.Length);

            List<Packet> output = new List<Packet>();

            while (true)
            {
                Packet p = new Packet(completeByteArray);
                output.Add(p);
                if (DataParser.IsPacketLast(p))
                {
                    break;
                }
                else
                {
                    await streamReader.LoadAsync(sizeof(int));
                    temp = new byte[sizeof(int)];
                    streamReader.ReadBytes(temp);
                    int nextPacketSize = DataParser.Bytes2Int(temp);

                    leftBytes = nextPacketSize + 9;
                    await streamReader.LoadAsync((uint)leftBytes);
                    incompleteByteArray = new byte[leftBytes];
                    streamReader.ReadBytes(incompleteByteArray);
                    completeByteArray = new byte[nextPacketSize + 13];
                    System.Buffer.BlockCopy(DataParser.Int2Bytes(nextPacketSize), 0, completeByteArray, 0, sizeof(int));
                    System.Buffer.BlockCopy(incompleteByteArray, 0, completeByteArray, sizeof(int), incompleteByteArray.Length);
                }
            }
            return output.ToArray();
        }

    }
}
