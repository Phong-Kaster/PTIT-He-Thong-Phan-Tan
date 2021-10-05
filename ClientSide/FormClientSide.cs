using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ClientSide
{
    public partial class FormClient : DevExpress.XtraEditors.XtraForm
    {
        /*khoi tao socket phia server*/
        private static Socket clientSide = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static string clientRequest = "";



        /***********************************************************************
         * Step 1: vong lap while gui ket noi lien tuc toi  
         * Step 2: ket noi toi server bang IP Loopback address toi port 100
         * Step 3: khi nao ma ket noi duoc thi xoa man hinh di ghi bang "connected"
         ***********************************************************************/
        private static void LoopConnect()
        {
            //int trial = 0;
            while (!clientSide.Connected)
            {
                //trial++;
                try
                {
                    clientSide.Connect(IPAddress.Loopback, 100);
                    Console.WriteLine("IP : " + IPAddress.Loopback);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LoopConnect error"); 
                    Console.WriteLine(ex.Message);
                }
            }
        }



        private static void SendData()
        {
            while (true)
            {
                
                //Thread.Sleep(500);
                byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
                clientSide.Send(dataSent);

                byte[] buffer = new byte[1024];
                int dataReceivedSize = clientSide.Receive(buffer);

                byte[] dataReceived = new byte[dataReceivedSize];
                Array.Copy(buffer, dataReceived, dataReceivedSize);

                String response = Encoding.ASCII.GetString(dataReceived);
                Console.WriteLine("<--------<<Response: " + response);
            }
        }



        public FormClient()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
                clientRequest = txtRequest.Text;

                //Thread.Sleep(500);
                byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
                clientSide.Send(dataSent);

                byte[] buffer = new byte[1024];
                int dataReceivedSize = clientSide.Receive(buffer);

                byte[] dataReceived = new byte[dataReceivedSize];
                Array.Copy(buffer, dataReceived, dataReceivedSize);

                string response = Encoding.ASCII.GetString(dataReceived);
                //response = response.Replace("|", "\n\n\n\n");
                Console.WriteLine(response);
                txtResponse.Text = response;

         
        }

        private void button4_Click(object sender, EventArgs e)
        {
            while (!clientSide.Connected)
            {
                //trial++;
                try
                {
                    clientSide.Connect(IPAddress.Loopback, 100);
                    Console.WriteLine("IP : " + IPAddress.Loopback);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LoopConnect error");
                    Console.WriteLine(ex.Message);
                }
            }
            txtStatus.Text = "Connected";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtRequest.Text = fbd.SelectedPath;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.txtRequest.Text = "";
            this.txtResponse.Text = "";
        }

        private void btnConnect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            while (!clientSide.Connected)
            {
                //trial++;
                try
                {
                    clientSide.Connect(IPAddress.Loopback, 100);
                    Console.WriteLine("IP : " + IPAddress.Loopback);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LoopConnect error");
                    Console.WriteLine(ex.Message);
                }
            }
            txtStatus.Text = "Connected";
        }

        private void btnSend_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            clientRequest = txtRequest.Text;

            //Thread.Sleep(500);
            byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
            clientSide.Send(dataSent);

            byte[] buffer = new byte[1024];
            int dataReceivedSize = clientSide.Receive(buffer);

            byte[] dataReceived = new byte[dataReceivedSize];
            Array.Copy(buffer, dataReceived, dataReceivedSize);

            string response = Encoding.ASCII.GetString(dataReceived);
            //response = response.Replace("|", "\n\n\n\n");
            Console.WriteLine(response);
            txtResponse.Text = response;
        }

        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.txtRequest.Text = "";
            this.txtResponse.Text = "";
        }

        private void btnExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Dispose();
        }
    }
}
