using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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

        private string retrieveFileNDirectory(string path)
        {
            string result = "";
            string fileResult = "";
            string directoryResult = "";


            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);


            foreach (string element in files)
            {
                fileResult = fileResult + "\r\n" + Path.GetFileName(element);
                Console.WriteLine(element);
            }

            foreach (string element in directories)
            {
                directoryResult = directoryResult + "\r\n" + Path.GetFileName(element);
                Console.WriteLine(element);
            }

            result = fileResult + "\r\n" + directoryResult;
            return result;
        }

        
        /*******************************PHẦN CỦA QUỐC TUẤN********************************/

        /**********************************************************************************
         * Step 1: Khởi tạo các list string chứa extension của loại file
         * Step 2: Hàm isEndWith để kiểm tra đường dẫn file có đuôi cần tìm hay không
         * Step 3: Hàm retrieveFiltered_FileNDirectory kết hợp retrieveFileNDirectory và isEndWith để cho kết quả đã lọc
         * 
         **********************************************************************************/

        /*Step 1*/
        private static List<string> soundExtensions = new List<string>(new string[] { "mp3", "m4p", "m4a", "flac" });
        private static List<string> videoExtensions = new List<string>(new string[] { "mp4", "mkv", "webm", "flv" });
        private static List<string> textExtensions = new List<string>(new string[] { "txt", "doc", "docx" });
        private static List<string> imageExtensions = new List<string>(new string[] { "jpg", "jpeg", "png", "bmp" });
        private static List<string> compressedExtensions = new List<string>(new string[] { "7z", "rar", "zip" });

        /*Step 2*/
        public bool isEndWith(string fileType, string path)
        {
            switch (fileType)
            {
                case "sound":
                    if (soundExtensions.Contains(path.Substring(path.LastIndexOf(".") + 1).ToLower()))
                    {
                        return true;
                    }
                    break;
                case "video":
                    if (videoExtensions.Contains(path.Substring(path.LastIndexOf(".") + 1).ToLower()))
                    {
                        return true;
                    }
                    break;
                case "text":
                    if (textExtensions.Contains(path.Substring(path.LastIndexOf(".") + 1).ToLower()))
                    {
                        return true;
                    }
                    break;
                case "image":
                    if (imageExtensions.Contains(path.Substring(path.LastIndexOf(".") + 1).ToLower()))
                    {
                        return true;
                    }
                    break;
                case "compressed":
                    if (compressedExtensions.Contains(path.Substring(path.LastIndexOf(".") + 1).ToLower()))
                    {
                        return true;
                    }
                    break;
                case "all":
                    return true;
                    break;
            }
            return false;
        }


        /*******************************PHẦN CỦA QUỐC TUẤN********************************/
        /**********************************************************************************
         * @path = đường dẫn thư mục gửi từ client
         * result = chứa kết quả trả về cuối cùng
         * fileResult = chứa tên các tệp tin
         * directoryResult = chứa tên các thư mục
         * files = mảng chứa tên tệp tin lấy từ hệ thống
         * directories = mảng chứa tên thư mục lấy từ hệ thống
         * 
         * Step 1: khai bao bien
         * Step 2: duyệt vòng lặp lấy tên các tệp tin bỏ vào fileReusult
         *  + mảng pathAndTypes chứa sẵn thứ tự các đuôi. Ví dụ [ 'jpg', 'mp3', 'txt',....]
         *  + nếu isEndWith trả về true tức là loại file đó nằm tron danh sách được hỗ trợ, những đuôi  
         * file lạ kiểu 7zip, exez, .. sẽ không được lấy vào
         * Step 3: nếu yêu cầu là folder hoặc all thì lấy hết
         * Step 4: trả kết quả về
         **********************************************************************************/
        private string retrieveFiltered_FileNDirectory(string path, string[] pathAndTypes)
        {
            /*Step 1*/
            Console.WriteLine("retrieve filtered is running");
            string result = "";
            string fileResult = "";
            string directoryResult = "";
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);



            /*Step 2*/
            foreach (string element in files)
            {
                for (int j = 2; j < pathAndTypes.Length; j++)
                {
                    if (isEndWith(pathAndTypes[j], Path.GetFileName(element)))
                    {
                        fileResult = fileResult + "\r\n" + Path.GetFileName(element);

                    }
                }
                Console.WriteLine(element);
            }



            /*Step 3*/
            if (pathAndTypes[2] == "folder" || pathAndTypes[2] == "all")
            {
                foreach (string element in directories)
                {
                    directoryResult = directoryResult + "\r\n" + Path.GetFileName(element);
                    Console.WriteLine(element);
                }
            }


            /*Step 4*/
            result = fileResult + "\r\n" + directoryResult;
            return result;
        }


        /**********************************************************************************
         * tương tự như trên nhưng duyệt cả LIST để lấy hết file được hỗ trợ 
         * trong list phía trên
         **********************************************************************************/
        public bool isEndWith(List<string> extensions, string path)
        {
            if (extensions.Contains(path.Substring(path.LastIndexOf(".") + 1).ToLower()))
            {
                return true;
            }
            return false;
        }
        private string retrieveFiltered_FileNDirectory(string path, List<string> extensions)
        {
            Console.WriteLine("retrieve exfiltered is running");
            string result = "";
            string fileResult = "";
            string directoryResult = "";
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);


            foreach (string element in files)
            {
                if (isEndWith(extensions, Path.GetFileName(element)))
                {
                    fileResult = fileResult + "\r\n" + Path.GetFileName(element);

                }
                Console.WriteLine(element);
            }
            /* foreach (string element in directories)
             {
                 directoryResult = directoryResult + "\r\n" + Path.GetFileName(element);
                 Console.WriteLine(element);
             }*/
            result = fileResult + "\r\n" + directoryResult;
            return result;
        }
        /*------------------------------------------------------------------------------*/

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
         * operating || system || os -> lay thong tin he dieu hanh
         * processor -> lay thong tin phan cung CPU
         **************************************************************/
        private string returnResponse(string request)
        {
            string response = "Not Available";

            /*-----------------Tuấn thêm vào-------------
             * phân tách request từ client
             */
            string[] pathAndTypes = request.Split('*');
            /*-------------------------------------------*/

            request = request.ToLower();

            if (request == "time") response = "It is " + DateTime.Now.ToLongTimeString();



            if (request == "date")
            {
                DateTime dateTime = DateTime.UtcNow.Date;
                response = "Today is " + dateTime.ToString("dd-MM-yyyy");
            }

            if (request == "photo")
            {
                /*Mo Thu Muc Template trong o D*/
                Process.Start(@"C:\Users\Dell\Pictures");
                response = "Opening Photo folder";
            }

            if (request == "music")
            {
                /*"C:\Program Files (x86)\Dopamine\Dopamine.exe"*/
                Process.Start(@"C:\Program Files (x86)\Dopamine\Dopamine.exe");
                response = "Opening Dopamine";
            }

            if (request == "chrome")
            {
                /*C:\Users\Dell\AppData\Local\Google\Chrome\Application\chrome.exe*/
                Process.Start(@"C:\Users\Dell\AppData\Local\Google\Chrome\Application\chrome.exe");
                response = "Opening Google Chrome";
            }

            if (request == "left 4 dead 2")
            {
                /*Bat game left 4 dead 2*/
                Process.Start(@"D:\Downloads\Games\Left_4_Dead_2 pass thichvn.com\[thichvn.com]_4D2TLS\Left 4 Dead 2 The Last Stand\left4dead2.exe");
                response = "Left 4 dead 2 is running. Wait a minute !";
            }

            if (Regex.IsMatch(request, @"^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$"))
            {
                response = retrieveFileNDirectory(request);
            }

            /*TRƯỜNG BỔ SUNG*/
            //lấy thông tin hệ điều hành
            else if (request.Contains("operating") || request.Contains("system") || request.Contains("os") )
            {
                response = getOperatingSystemInfo();
            }

            //lấy thông tin CPU
            else if (request.Contains("processor"))
            {
                response = getProcessorInfo();
            }

            /*TUÁN BỔ SUNG*/
            //kiểm tra lọc theo loại hay theo đuôi file
            
            else if (pathAndTypes[0] == "filtered")
            {
                response = retrieveFiltered_FileNDirectory(pathAndTypes[1], pathAndTypes);
            }
            else if (pathAndTypes[0] == "exFiltered")
            {
                response = retrieveFiltered_FileNDirectory(pathAndTypes[1], new List<string>(pathAndTypes[2].Split(';')));
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
            try
            {
                /*Step 1*/
                serverSide.Bind(new IPEndPoint(IPAddress.Any, 100));

                /*Step 2*/
                serverSide.Listen(1);

                /*Step 3*/
                serverSide.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
            catch(Exception ex)
            {

            }
            
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
         * Step 3: kiem tra yeu cau gom 2 loai
         *      1.Yeu cau thuc thi truyen thong: photo, music, chrome,...
         *      2.Yeu cau tai tep tin tu thu muc goc toi thu muc dich
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

                //kiểm tra có phải yêu cầu download không
                if (request[0].Equals('*') && Regex.IsMatch(request.Substring(1), @"^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$"))
                {
                    socket.BeginSend(sendDownload(request), 0, sendDownload(request).Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
                }
                else
                {
                    /*Step 4*/
                    string response = returnResponse(request);
                    Console.WriteLine("Response : " + response);


                    /*Step 5*/
                    byte[] dataSent = Encoding.ASCII.GetBytes(response);
                    socket.BeginSend(dataSent, 0, dataSent.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
                }


                /*Step 6*/
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
                serverSide.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
            catch (Exception ex)
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



        /*******************************PHẦN CỦA VĨNH TRƯỜNG********************************/

        /*********************************************************************************
         * Step 1: khoi tao tieu de
         * Step 2: tao doi tuong ManagementObjectSearcher va truyen cau truy van vao nhu doi so
         * Step 3: chay vong lap lay cac thong tin can thiet cua he dieu hanh
         *********************************************************************************/
        public String getOperatingSystemInfo()
        {
            /*Step 1*/
            String result = "";
            result += "****OPERATING SYSTEM INFOR****\r\n";
            

            /*Step 2*/
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");


            /*Step 3*/
            foreach (ManagementObject managementObject in mos.Get())
            {
                //Lay ten he dieu hanh
                if (managementObject["Caption"] != null)
                {
                    result += "Operating System Name  :  " + managementObject["Caption"].ToString() + "\r\n";
                }
                //Kien truc 64 bit hay 32 bit
                if (managementObject["OSArchitecture"] != null)
                {
                    result += "Operating System Architecture  :  " + managementObject["OSArchitecture"].ToString() + "\r\n";
                }
                //Goi dich vu di kem
                if (managementObject["CSDVersion"] != null)
                {
                    result += "Operating System Service Pack   :  " + managementObject["CSDVersion"].ToString() + "\r\n";
                }
            }
            return result;
        }



        /*******************************PHẦN CỦA VĨNH TRƯỜNG********************************/

        /*********************************************************************************
         * Step 1: khoi tao tieu de
         * Step 2: tao doi tuong lay de truy cap vao thong tin phan cung
         * Step 3: chay vong lap lay cac thong tin can thiet cua bo vi xu ly
         *********************************************************************************/
        public String getProcessorInfo()
        {
            /*Step 1*/
            String result = "";
            result += "****PROCESSOR NAME****\r\n";

            /*Step 2*/
            RegistryKey processor_name = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);

            /*Step 3*/
            if (processor_name != null)
            {
                if (processor_name.GetValue("ProcessorNameString") != null)
                {
                    result += processor_name.GetValue("ProcessorNameString") + "\r\n";
                }
            }
            return result;
        }



        /*********************************************************************************
         * Hàm xử lý file khi có yêu cầu tải từ thư mục đích tới thư mục gốc
         * 
         * file: tệp tin được yêu cầu
         * fName: tên tệp tin
         * path: đường dẫn tệp tin
         * 
         * Step 1: lay ten tep tin tu vi tri so 1 và thay the ki tu "/" bang "\\"
         * Step 2: duyet ten tep tin va lay path va fName
         * Step 3: nap ten tep tin vao mang byte
         * Step 4: chuyen kich thuoc tep tin tu byte sang bit
         * Step 5: ghep path va fName -> duong dan tuyet doi cua file -> nap duong dan vao mang byte
         * Step 6: gui tep tin da duoc dong goi ve client
         *********************************************************************************/
        private Byte[] sendDownload(String request)
        {
            /*Step 1*/
            string file = request.Substring(1);
            string fName = file.Replace("\\", "/");
            string path = "";

            /*Step 2*/
            while (fName.IndexOf("/") > -1)
            {
                path += fName.Substring(0, fName.IndexOf("/") + 1);
                fName = fName.Substring(fName.IndexOf("/") + 1);
            }
            

            /*Step 3*/
            byte[] fNameByte = Encoding.ASCII.GetBytes(fName);

            Console.WriteLine("Buffering...");

            /*Step 4*/
            byte[] fNameLen = BitConverter.GetBytes(fNameByte.Length);

            /*Step 5*/
            byte[] fileData = File.ReadAllBytes(path + fName);

            //khởi tạo mảng byte có kích thước phù hợp để chứa dữ liệu gửi đi:
            //(gồm: - 4ptu đầu là độ dài của tên file( là 1 số nguyên đc biến đổi về mảng byte[4]),
            //      - tiếp theo là ptu dùng để lưu trữ tên file được biến đổi về mảng byte,
            //      - cuối cùng là các ptu dùng để lưu trữ dữ liệu file đã đc chuyển về mảng byte)
            // === NOTE: Sẽ áp dụng thứ tự này để tiến hành ghi dữ liệu vào mảng và client cũng sẽ phải đọc theo thứ tự ntn lun
            byte[] clientData = new byte[4 + fNameByte.Length + fileData.Length];

            /*Step 6*/
            fNameLen.CopyTo(clientData, 0);
            fNameByte.CopyTo(clientData, 4);
            fileData.CopyTo(clientData, 4 + fNameByte.Length);

            return clientData;
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
