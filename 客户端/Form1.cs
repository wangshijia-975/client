// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Form1.cs" company="nd@231216">
//   
// </copyright>
// <summary>
//   Defines the Form1 type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace 客户端
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Windows.Forms;

    using CCWin;

    /// <inheritdoc />
    /// <summary>TODO The form 1.</summary>
    public partial class Form1 : Skin_Color
    {

        public Form1()
        {
            InitializeComponent();
        }

        ClientSjmyHelper sjmyHelper = new ClientSjmyHelper();


        // 客户端通信套接字-
        Socket sokMsg = null;

        // 客户端通信线程
        Thread thrMsg = null;

        private void Form1_Shown(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory($@"D:\developer");
            // 连接45.25
            ShowMsg(RemoteConnect.ConnectState(sjmyHelper.Site, sjmyHelper.Account, sjmyHelper.Password));

            // 获取在线设备
            foreach (var devices in sjmyHelper.GetDevices())
            {
                listb(devices);
            }

            const int port = 6000;
            const string ip = "192.168.64.105"; // 需要连接的ip地址
            try
            {
                // 创建连接套接字，使用ip4协议，流式传输，tcp链接
                sokMsg = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 获取要链接的服务端节点
                // 获取网络节点对象
                var address = IPAddress.Parse(ip);
                var endPoint = new IPEndPoint(address, port);

                // 向服务端发送链接请求。
                sokMsg.Connect(endPoint);
                //ShowMsg("连接服务器成功！");

                // 开启通信线程
                thrMsg = new Thread(ReceiveMsg);
                thrMsg.IsBackground = true;

                // win7， win8 需要设置客户端通信线程同步设置，才能在接收文件时打开文件选择框
                thrMsg.SetApartmentState(ApartmentState.STA);
                thrMsg.Start();
            }
            catch (Exception ex)
            {
                ShowMsg("连接服务器失败！" + ex.Message);
            }
        }

        #region  打印消息 + ShowMsg(string strmsg)
        public delegate void textbdelegate(string str);

        /// <summary>打印消息 跨线程添加textbox的值</summary>
        /// <param name="msg">The msg.</param>
        public void ShowMsg(string msg)
        {
            if (log.InvokeRequired)
            {
                textbdelegate dt = ShowMsg;
                log.Invoke(dt, msg + "\n");
            }
            else
            {
                log.AppendText(ClientSjmyHelper.GetTime() + msg + "\n");
            }
        }
        #endregion


        #region 接收服务端消息
        bool isReceive = true;
        void ReceiveMsg()
        {
            // 准备一个消息缓冲区域
            var arrMsg = new byte[1024 * 1024 * 1];
            try
            {
                while (isReceive)
                {
                    // 接收 服务器发来的数据，因为包含了一个标示符，所以内容的真实长度应该-1
                    var realLength = sokMsg.Receive(arrMsg);
                    GetMsg(arrMsg, realLength);
                }
            }
            catch (Exception)
            {
                sokMsg.Close();
                sokMsg = null;
                ShowMsg("远程服务器关闭！");
            }
        }
        #endregion

        #region  接收服务端文本消息 + GetMsg(byte[] arrContent, int realLength)
        /// <summary>
        /// 接收服务端文本消息
        /// </summary>
        /// <param name="arrContent"></param>
        /// <param name="realLength"></param>
        private void GetMsg(byte[] arrContent, int realLength)
        {

            try
            { // 获取接收的内容
                var strMsg = System.Text.Encoding.UTF8.GetString(arrContent, 0, realLength);
                ShowMsg("服务器说：" + strMsg);
                var strtemp = strMsg.Split('*');
                switch (strtemp[0])
                {
                    // 1 渠道名称 2文件夹路径（数据库名称） 3 Apk版本  4 MTID(任务id) 5 卸载标志  6快速登陆标志 7 全局卸载标志
                    case "搭建测试环境":
                        ShowMsg($@"获取资源路径:{strtemp[1]}\{strtemp[2]}");

                        sjmyHelper.ProjectSite = $@"D:\developer\{strtemp[1]}\";

                        ShowMsg(sjmyHelper.ResetDir());

                        // ShowMsg("重置客户端环境目录成功，开始下载客户端资源");                
                        ShowMsg(sjmyHelper.DownResource(strtemp[1], strtemp[2]));

                        ShowMsg("客户端资源下载完成，开始复制测试模块脚本文件");
                        ShowMsg(sjmyHelper.CopyScript(strtemp[1], strtemp[3]));
                        sjmyHelper.SetImage(1, strtemp[1]);
                        ShowMsg("已替换登录文件");

                        sjmyHelper.SetSqlConfig(
                            strtemp[1] == "myht" ? $"myht_autotest{strtemp[2]}" : $"sjmy_autotest{strtemp[2]}");

                        ShowMsg("已修改SqlConfig.xml");

                        ShowMsg($"客户端环境搭建成功！卸载标志为{strtemp[5]}");
                        if (strtemp[5] == "0")
                        {
                            // strtemp[7] 全局卸载标志 strtemp[8] 包名
                            if (strtemp[7] == "1")
                            {
                                sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\uninstall.py", "com.tq.my");
                                sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\uninstall.py", "com.westhouse.mysjb.xsj");
                                sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\uninstall.py", "com.nd.myht");
                            }
                            else
                            {
                                sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\uninstall.py", "com.nd.myht");
                                sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\uninstall.py", "com.westhouse.mysjb.xsj");
                                sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\uninstall.py", $"{strtemp[8]}");
                            }

                            ShowMsg("app卸载操作已完成");
                        }
                        else
                        {
                            sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\stopApp.py", "com.tq.my");
                            sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\stopApp.py", "com.westhouse.mysjb.xsj");
                            sjmyHelper.RunCmdPython($@"{sjmyHelper.ProjectSite}main\stopApp.py", "com.nd.myht");
                            ShowMsg("app进程已关闭");
                        }

                        if (strtemp[6] == "1")
                        {
                            ShowMsg("已开启只登陆不执行脚本功能");
                            sjmyHelper.DelectDir();
                        }

                        // 执行runcontroler
                        ShowMsg("执行runcontroler脚本");

                        var Pro = Process.Start($@"D:\developer\平台控制器\runcontroler.exe", $"{strtemp[4]} {strtemp[1]}");

                        if (Pro != null)
                        {
                            if (!Pro.WaitForExit(10800000))
                            {
                                Pro.Kill();
                            }
                        }

                        // runcontroler.py 结束后标志本机跑完
                        btnSendMsg(get64segmentIP.LocalIp + "：第一轮全部脚本执行完成");
                        break;

                    case "执行第二轮":

                        ShowMsg("---------第二轮----------------");
                        ShowMsg(sjmyHelper.ReReport());

                        sjmyHelper.SetImage(2, strtemp[1]);
                        ShowMsg("已替换登录文件");
                        sjmyHelper.SetSqlConfig(
                            strtemp[1] == "myht" ? $"myht_again{strtemp[2]}" : $"sjmy_again{strtemp[2]}");
                        ShowMsg("已修改SqlConfig.xml");

                        ShowMsg("执行runcontroler脚本（第二轮）");

                        var ProTwo = Process.Start($@"D:\developer\平台控制器\runcontroler.exe", $"{strtemp[4]} {strtemp[1]}");

                        if (ProTwo != null)
                        {
                            if (!ProTwo.WaitForExit(10800000))
                            {
                                ProTwo.Kill();
                            }
                        }

                        btnSendMsg(get64segmentIP.LocalIp + "：第二轮全部脚本执行完成");
                        break;

                    case "上传任务结果":

                        // 1未通过报告路径 2 渠道名称  3文件夹路径（数据库名称）
                        ShowMsg(sjmyHelper.Uploa(strtemp[1], strtemp[2], strtemp[3]));

                        btnSendMsg(get64segmentIP.LocalIp + "：任务上传完成");
                        break;
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
          
        }

        #endregion

        #region 客户端发送消息到服务端
        /// <summary>客户端发送消息到服务端</summary>
        /// <param name="strMsg">The str Msg.</param>
        private void btnSendMsg(string strMsg)
        {
            if (strMsg != string.Empty)
            {
                var arrMsg = System.Text.Encoding.UTF8.GetBytes(strMsg);
                try
                {
                    sokMsg.Send(arrMsg);
                }
                catch (Exception ex)
                {
                    ShowMsg("发送消息失败！" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("未输入任何信息！");
            }
        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            sjmyHelper.ProjectSite = $@"D:\developer\sjmy\";
            sjmyHelper.SetImage(2,"sjmy");

        }

        #region 跨线程访问listbox

        public delegate void listbdelegate(string str);

        /// <summary>
        /// 跨线程添加listbox的值
        /// </summary>
        /// <param name="msg"></param>
        public void listb(string msg)
        {
            if (listDevice.InvokeRequired)
            {
                listbdelegate dt = listb;
                listDevice.Invoke(dt, msg);
            }
            else
            {
                listDevice.Items.Add(msg);
            }
        }



        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            ClientSjmyHelper.AdbGuard();
        }
    }
}
