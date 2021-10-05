using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerSide
{
    public partial class FormServerSide : Form
    {
        /*buffer de luu du lieu nhan duoc tu client*/
        private static byte[] buffer = new byte[1024];

        /*Khoi tao list chua cac socket cua client*/
        private static List<Socket> clientSide = new List<Socket>();

        /*khoi tao socket phia server*/
        private static Socket serverSide = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /*khoi tao regex de kiem tra neu request la 1 duong dan*/
        private static string pattern = @"^(?:[a - zA - Z]\:|\\\\[\w\.] +\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$";
        private static Regex folderRegex = new Regex(pattern);



        /**
         * 
         */
        private string retrieveFileNDirectory(string path)
        {
            string result = "";
            string fileResult = "";
            string directoryResult = "";


            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);


            foreach(string element in files)
            {
                fileResult = fileResult + "\r\n" + Path.GetFileName(element);
                Console.WriteLine(element);
            }

            foreach(string element in directories)
            {
                directoryResult = directoryResult + "\r\n" + Path.GetFileName(element);
                Console.WriteLine(element);
            }

            result = fileResult + "\r\n" + directoryResult;
            return result;
        }



        /**************************************************************
         * Ham nay nhan dien yeu cau(request) va tra ve phan hoi(response) tuong
         * ung voi yeu cau
         * 
         * VD:
         * time -> gio hien tai
         * today -> ngay thang hom nay
         * left 4 dead 2 -> mo game va quay thoi
         * photo -> mo thu muc hinh anh
         * duong dan thu muc -> danh sach ten tin va thu muc cua duong dan do
         **************************************************************/
        private string returnResponse(string request)
        {
            string response = "Not Available";
            request = request.ToLower();

            if (request == "time") response = "It is " + DateTime.Now.ToLongTimeString();



            if( request == "date")
            {
                DateTime dateTime = DateTime.UtcNow.Date;
                response = "Today is " + dateTime.ToString("dd-MM-yyyy");
            } 

            if( request == "photo")
            {
                /*Mo Thu Muc Template trong o D*/
                Process.Start(@"C:\Users\Dell\Pictures");
                response = "Opening Photo folder";
            }    

            if( request == "music")
            {
                /*"C:\Program Files (x86)\Dopamine\Dopamine.exe"*/
                Process.Start(@"C:\Program Files (x86)\Dopamine\Dopamine.exe");
                response = "Opening Dopamine";
            }

            if ( request == "chrome")
            {
                /*C:\Users\Dell\AppData\Local\Google\Chrome\Application\chrome.exe*/
                Process.Start(@"C:\Users\Dell\AppData\Local\Google\Chrome\Application\chrome.exe");
                response = "Opening Google Chrome";
            }

            if ( request == "left 4 dead 2")
            {
                /*Bat game left 4 dead 2*/
                Process.Start(@"D:\Downloads\Games\Left_4_Dead_2 pass thichvn.com\[thichvn.com]_4D2TLS\Left 4 Dead 2 The Last Stand\left4dead2.exe");
                response = "Left 4 dead 2 is running. Wait a minute !";
            }    

            if (Regex.IsMatch(request, @"^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$"))
            {
                response = retrieveFileNDirectory(request);
            }

            return response;
        }



        /******************************************************
         *Step 1: lang nghe ket noi tu moi IP voi cong 100
         *Step 2: dat xem co bao nhieu IP co the cung ket noi toi server. So 5 nghia 
         *la co 6 ket noi co the dong thoi ket noi toi server
         *Step 3: lang nghe cac ket noi
         ******************************************************/
        private void setUp()
        {
            Console.WriteLine("Server is running");

            /*Step 1*/
            serverSide.Bind(new IPEndPoint(IPAddress.Any, 100));

            /*Step 2*/
            serverSide.Listen(1);

            /*Step 3*/
            serverSide.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }



        /*************************************************************************
         * ham nay de tao ra socket lang nghe client 
         * Step 1: serverSide tao mot socket de co the gui du lieu ve cho client
         * Step 2: luu socket nay vao clientSide
         * Step 3: nhan du lieu tu client gui ve
         * Step 4: dung ki thuat truy hoi - dong lenh nay cho phep serverSide tao duoc 
         * nhieu socket de lang nghe cung luc nhieu client hon
         *************************************************************************/
        private void AcceptCallBack(IAsyncResult IAR)
        {
            /*Step 1*/
            Socket socket = serverSide.EndAccept(IAR);
            /*Step 2*/
            clientSide.Add(socket);
            
            Console.WriteLine("Client have connected !");
            /*Step 3*/
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);

            serverSide.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }



        /*************************************************************************
         * ham nay de xu ly du lieu nhan duoc tu client
         * Step 1: socket nay chinh la socket cua AcceptCallBack
         * Step 2: xac dinh xem data nhan duoc lon co nao 
         * Step 2.1: tao 1 mang byte de co 1 noi chua du lieu phu hop voi du lieu 
         * nhan duoc
         * Step 2.2: copy du lieu tu buffer vao data da tao voi kich co dataSize 
         * chinh la kich co cua goi tin nhan duoc
         * Step 3: ma hoa du lieu nhan duoc va in ra man hinh
         * Step 4: lay ket qua tra ve phu hop voi yeu cau cua client
         * Step 5: dong goi du lieu tra ve client
         * Step 6: bat dau 1 vong lop moi nghe du lieu va nhan du lieu tu client tiep
         *************************************************************************/
        private void ReceiveCallBack(IAsyncResult IAR)
        {
            try
            {
                /*Step 1*/
                Socket socket = (Socket)IAR.AsyncState;


                /*Step 2*/
                int dataSize = socket.EndReceive(IAR);//Exception
                /*Step 2.1*/
                byte[] dataReceived = new byte[dataSize];
                /*Step 2.2*/
                Array.Copy(buffer, dataReceived, dataSize);


                /*Step 3*/
                string request = Encoding.ASCII.GetString(dataReceived);
                Console.WriteLine("\n\nRequest : " + request);


                /*Step 4*/
                string response = returnResponse(request);
                Console.WriteLine("Response : " + response);


                /*Step 5*/
                byte[] dataSent = Encoding.ASCII.GetBytes(response);
                socket.BeginSend(dataSent, 0, dataSent.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);


                /*Step 6*/
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
                serverSide.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
            catch(Exception ex)
            {
                //throw ex;
                Console.WriteLine(ex);
            }
            
        }



        /***********************************************************
         * SendCallBack de ket thuc phien lam viec
         ***********************************************************/
        private static void SendCallBack(IAsyncResult IAR)
        {
            Socket socket = (Socket)IAR.AsyncState;
            socket.EndSend(IAR);
        }



        public FormServerSide()
        {
            InitializeComponent();
            //txtOperation.Text = diary;
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //btnEXIT
            this.Dispose();
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // btnACTIVATE
            txtStatus.Text = "Active";
            setUp();
            this.barButtonItem2.Enabled = false;
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            
        }

        private void txtStatus_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
