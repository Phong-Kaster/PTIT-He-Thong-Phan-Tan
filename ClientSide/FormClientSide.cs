using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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
            //kt người dùng có chọn option dowload ko
            if (!btnDownload.Checked)
            {
                /*Tuấn thêm vào*/
                if (!checkBoxAll.Checked || textBoxExtensions.Text.Trim() != "")
                {
                    sendRequestWithFilesTypesOptions();
                }
                /**/
                else
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
                
            }
            else
            {
                txtResponse.Text = "";
                clientRequest = "*"+txtRequest.Text;// thêm cờ vào rồi gửi lên cho server
                byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
                clientSide.Send(dataSent);

                txtResponse.Text += "It Works and looks for files\r\n";
                //Nhận dữ liệu từ server
                byte[] clientData = new byte[600000000];
                int receiveByteLen = clientSide.Receive(clientData);

                //phần tách dữ liệu theo như thứ tự đã được đặt trước ở server
                txtResponse.Text += "Receiving file ....\r\n";
                // lấy chiều dài của tên file
                int fNameLen = BitConverter.ToInt32(clientData, 0);

                //tiếp theo là lấy tên file
                string fName = Encoding.ASCII.GetString(clientData, 4, fNameLen);

                //tiến hành tạo tên file trên đường dẫn đích mà người dùng nhập vào
                BinaryWriter write = new BinaryWriter (File.Open (txtPathDest.Text + "/" + fName ,  FileMode.Append ));

                //Nạo dữ liệu vào file
                write .Write (clientData , 4+fNameLen , receiveByteLen -4 -fNameLen );

                txtResponse.Text += "Saving file....\r\n";
                write .Close ();
            }
        }

        /* ---------------------- Tuấn -----------------------------------
         */
        private void sendRequestWithFilesTypesOptions()
        {
            string requestWithFileTypeFilter = "";
            if (textBoxExtensions.Text != "")
            {
                requestWithFileTypeFilter = "exFiltered*" + txtRequest.Text + "*" + textBoxExtensions.Text.Trim();
            }
            else
            {
                requestWithFileTypeFilter = "filtered*" + txtRequest.Text;
                if (checkBoxAll.Checked)
                {
                    requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "all";
                }
                else
                {
                    if (checkBoxFolder.Checked)
                    {
                        requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "folder";

                    }

                    if (checkBoxSound.Checked)
                    {
                        requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "sound";
                    }
                    if (checkBoxVideo.Checked)
                    {
                        requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "video";
                    }
                    if (checkBoxText.Checked)
                    {
                        requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "text";
                    }
                    if (checkBoxImage.Checked)
                    {
                        requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "image";
                    }
                    if (checkBoxCompressed.Checked)
                    {
                        requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "compressed";
                    }
                }
            }

            /**/

            byte[] dataSent = Encoding.ASCII.GetBytes(requestWithFileTypeFilter);
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
        /**/
        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.txtRequest.Text = "";
            this.txtResponse.Text = "";
        }

        private void btnExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Dispose();
        }

        private void btnDownload_CheckedChanged(object sender, EventArgs e)
        {
            if (btnDownload.Checked)
            {
                txtPathDest.Visible = true;
            }
            else
            {
                txtPathDest.Visible = false;
            }
        }

        /* ------------------ Tuấn ----------------------*/
        private void checkBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAll.Checked)
            {
                checkBoxFolder.Checked = false;
                checkBoxText.Checked = false;
                checkBoxSound.Checked = false;
                checkBoxVideo.Checked = false;
                checkBoxImage.Checked = false;
                checkBoxCompressed.Checked = false;
            }
        }

        private void checkBoxSound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSound.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxVideo_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxVideo.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxText_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxText.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxFolder.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxImage_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxImage.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxCompressed_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCompressed.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }
    }
}
