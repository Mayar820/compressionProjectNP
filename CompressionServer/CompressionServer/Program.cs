using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;
using System.Net.Sockets;
using System.IO;

namespace CompressionServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Socket welcomingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 5090);
            welcomingSocket.Bind(ipep);
            welcomingSocket.Listen(10);
            Console.WriteLine("Server Started , waiting for client...\n");

            while (true) { 

                Socket handlingSocket = await welcomingSocket.AcceptAsync();
                IPEndPoint clientipep = (IPEndPoint)handlingSocket.RemoteEndPoint;
                Console.WriteLine("Client conected: " + clientipep.ToString());

                _ = Task.Run(() => HandleClient(handlingSocket));
            
            }
        }

        static async Task HandleClient(Socket handlingSocket)
        {
            try
            {
                byte []bufSize = new byte[8];
                await ReceiveExactly(handlingSocket, bufSize, 8);
                long fileSize = BitConverter.ToInt64(bufSize, 0);
                Console.WriteLine("Receiving file of size: " + fileSize + " bytes ");

                byte[] fileBytes = new byte[fileSize];
                await ReceiveExactly(handlingSocket, fileBytes, (int)fileSize);

                byte[] compressed = Compress(fileBytes);
                Console.WriteLine("Compressed size: " + compressed.Length + " bytes ");

                byte[] compSize = BitConverter.GetBytes((long)compressed.Length);
                await handlingSocket.SendAsync(new ArraySegment<byte>(compSize), SocketFlags.None);

                await handlingSocket.SendAsync(new ArraySegment<byte>(compressed), SocketFlags.None);
                Console.WriteLine("Compressed file send to client");


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally { 
                handlingSocket.Close();
            }
        }
        static async Task ReceiveExactly(Socket socket, byte[] buffer, int count) {

            int totalReceived = 0;
            while (totalReceived < count) { 

                int rec = await socket.ReceiveAsync(new ArraySegment<byte>(buffer,totalReceived,count - totalReceived), SocketFlags.None);
                if (rec == 0) throw new Exception("Connection closed before all data recieved .");
                totalReceived+= rec;
            
            }
        }

        static byte[] Compress(byte[] data) {

            using (MemoryStream ms = new MemoryStream()) {

                using (GZipStream gz = new GZipStream(ms,CompressionMode.Compress)) {

                    gz.Write(data, 0, data.Length);

                }
                return ms.ToArray();
            }
        }

    }
}
