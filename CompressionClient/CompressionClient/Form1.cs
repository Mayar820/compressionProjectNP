using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO.Compression;
using System.Net.Sockets;
using System.IO;

namespace CompressionClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Socket clientSocket;
        byte[] fileBytes;
        string fileName;

        private async void button1_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"),5090);
            await clientSocket.ConnectAsync(ipep);
            textBox1.Text += "Connected to server!" + Environment.NewLine;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) { 

                fileName = ofd.FileName;
                fileBytes=File.ReadAllBytes(fileName);
                textBox1.Text += "File selected: " + Path.GetFileName(fileName) + Environment.NewLine;
                textBox1.Text += "Original size: " + fileBytes.Length + " bytes" + Environment.NewLine;


            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) { MessageBox.Show("Please select a file first!"); return; }
            if (clientSocket == null || !clientSocket.Connected) { MessageBox.Show("Please connect to server first!"); return; }

            byte[] sizeBytes = BitConverter.GetBytes((long)fileBytes.Length);
            await clientSocket.SendAsync(new ArraySegment<byte>(sizeBytes), SocketFlags.None);

            await clientSocket.SendAsync(new ArraySegment<byte>(fileBytes), SocketFlags.None);
            textBox1.Text += "File sent, waiting for compressed file..." + Environment.NewLine;

            byte[] compSizeBuf = new byte[8];
            await ReceiveExactly(clientSocket, compSizeBuf, 8);
            long compSize = BitConverter.ToInt64(compSizeBuf, 0);
            textBox1.Text += "Compressed size incoming: " + compSize + " bytes" + Environment.NewLine;

            byte[] compData = new byte[compSize];
            await ReceiveExactly(clientSocket, compData, (int)compSize);

            string savePath = fileName + ".gz";
            File.WriteAllBytes(savePath, compData);

            double ratio = (1.0 - (double)compSize / fileBytes.Length) * 100;
            textBox1.Text += "Done! Saved: " + Path.GetFileName(savePath) + Environment.NewLine;
            textBox1.Text += "Compression ratio: " + ratio.ToString("F1") + "% smaller" + Environment.NewLine;

            MessageBox.Show("File compressed and saved!\n" + savePath);


        }

        static async Task ReceiveExactly(Socket socket, byte[] buffer, int count)
        {

            int totalReceived = 0;
            while (totalReceived < count)
            {

                int rec = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, totalReceived, count - totalReceived), SocketFlags.None);
                if (rec == 0) throw new Exception("Connection closed before all data recieved .");
                totalReceived += rec;

            }
        }
    }
}
