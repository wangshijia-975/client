// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientSjmyHelper.cs" company="nd@231216">
//   
// </copyright>
// <summary>
//   Defines the sjmyHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace 客户端
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    using SharpAdbClient;

    public class ClientSjmyHelper
    {
        public string Database = "QAGameAuto";
        public string Site = @"\\192.168.45.25";
        public string Account = "administrator";
        public string Password = "www.99.com.";
        public string ApkSite = @"\\192.168.45.25\upload\";

        public string ProjectSite;


        #region 下载客户端环境
        /// <summary>下载任务资源文件</summary>
        /// <param name="gnpy">渠道名称</param>
        /// <param name="AppSrc">时间戳-任务文件夹</param>
        /// <returns></returns>
        public string DownResource(string gnpy, string AppSrc)
        {
            var str = "获取到文件：\t\n";

            var path = $@"{ApkSite}{gnpy}\{AppSrc}";
            try
            {
                var root = new DirectoryInfo(path);
                foreach (var t in root.GetFiles())
                {
                    if (Path.GetExtension(t.Name) == ".apk")
                    {
                        DirFile.Copy(t.FullName, $@"{ProjectSite}data\app\moyu.apk");
                        str = str + t + "\t\n";
                    }
                    else if (Path.GetExtension(t.Name) == ".sql")
                    {
                        DirFile.Copy(t.FullName, $@"{ProjectSite}data\sqlfile\sjmy_autotest.sql");
                        str = str + t + "\t\n";
                    }
                    else if (Path.GetExtension(t.Name) == ".zip")
                    {
                        DirFile.Copy(t.FullName, $@"{ProjectSite}data\update\zipfile\sjmy.zip");
                        SharpZip.UnpackFiles(
                            $@"{ProjectSite}data\update\zipfile\sjmy.zip",
                            $@"{ProjectSite}data\update\filezip\");
                        str = str + t + "\t\n";
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            return str + "客户端环境下载完成";
        }

        #endregion

        #region 复制script 版本不存在则生成新版本文件夹并复制上一版本文件

        public string CopyScript(string gnpy, string apkVersion)
        {
            if (gnpy == "myht") return "脚本文件复制完成";

            var sArray = apkVersion.Split('.');
            var path = $@"{ProjectSite}data\script\AllVersion\{sArray[0]}.{sArray[1]}.X";
            if (!DirFile.IsExistDirectory(path))
            {
                var dir = new DirectoryInfo($@"{ProjectSite}data\script\AllVersion").GetDirectories("*.X");

                // 复制最大文件夹中的内容
                DirFile.CopyFolder($@"{ProjectSite}data\script\AllVersion\{DirFile.SortAsFileName(ref dir)}", path);
            }

            DirFile.CopyFolder(path, $@"{ProjectSite}data\script");
            DirFile.Copy($@"{ProjectSite}data\script\environinit\UserInfo.xml", $@"{ProjectSite}xmlconfig\UserInfo.xml");
            DirFile.Copy($@"{ProjectSite}data\script\environinit\accountid.xml", $@"{ProjectSite}xmlconfig\accountid.xml");

            return "脚本文件复制完成";
        }

        #endregion

        #region 清空任务所需文件夹
        /// <summary>
        /// 清空任务所需文件夹
        /// </summary>
        /// <returns></returns>
        public string ResetDir()
        {
            try
            {
                DirFile.DelectDir($@"{ProjectSite}data\app");
                DirFile.DelectDir($@"{ProjectSite}data\sqlfile");
                DirFile.DelectDir($@"{ProjectSite}data\update\zipfile");
                DirFile.DelectDir($@"{ProjectSite}data\update\filezip");
                DirFile.DelectDir($@"{ProjectSite}data\result\cpumemory");
                DirFile.DelectDir($@"{ProjectSite}data\result\debug");
                DirFile.DelectDir($@"{ProjectSite}data\result\excel");
                DirFile.DelectDir($@"{ProjectSite}data\result\fpsExcel");
                DirFile.DelectDir($@"{ProjectSite}data\result\image");
                DirFile.DelectDir($@"{ProjectSite}data\result\serverlog");
                DirFile.DelectDir($@"{ProjectSite}data\result\trafficExcel");
                DirFile.DelectDir($@"{ProjectSite}data\script\environinit\tempDate");
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            return "资源目录重置完成";
        }


        public void DelectDir()
        {
            DirFile.DelectDir($@"{ProjectSite}data\script\case");
        }

        #endregion

        #region cmd运行python脚本
        /// <summary>
        /// 运行python脚本
        /// </summary>
        /// <param name="path">脚本绝对路径</param>
        /// <param name="parameter1">脚本启动参数1</param>
        /// <param name="parameter2">脚本启动参数2</param>
        public void RunCmdPython(string path, string parameter1 = "", string parameter2 = "")
        {
            var runcmd = new RunCmd();
            runcmd.Exe($@"python {path} {parameter1} {parameter2}");
        }

        #endregion

        #region 执行adb

        public string RunCmdAdb(string cmd)
        {
            var runcmd = new RunCmd();
            return runcmd.adb("adb " + cmd);
        }

        #endregion

        #region 获取设备id
        /// <summary>
        /// 获取设备id
        /// </summary>
        /// <returns> device组成的数组</returns>
        public string[] GetDevices()
        {
            AdbGuard();

            var devices = AdbClient.Instance.GetDevices();
            var devlist = new string[devices.Count];
            var i = 0;
            foreach (var device in devices)
            {
                devlist[i] = $"{device.Model}: {device.Serial}: {device.State}";
                i = i + 1;
            }

            return devlist;

            // var reply = RunCmdAdb("devices");
            // var test = reply.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion

        #region 检查本机是否完成所有脚本

        /// <summary>
        /// 检查本机是否完成所有脚本
        /// </summary>
        /// <returns></returns>
        public bool carryOut()
        {
            var filePath = $@"{ProjectSite}result\excel\summary.xlsx";
            Console.WriteLine(DirFile.IsExistFile(filePath));
            return DirFile.IsExistFile(filePath);
        }

        #endregion

        #region 修改查库配置xml

        public void SetSqlConfig(string database)
        {
            var doc = new XmlDocument();
            doc.Load($@"{ProjectSite}\xmlconfig\SqlConfig.xml");
            var xn = doc.SelectSingleNode("root")?.SelectSingleNode("database");
            if (xn != null) xn.InnerText = database;
            doc.Save($@"{ProjectSite}\xmlconfig\SqlConfig.xml");

        }

        #endregion

        #region 删除失败报告

        public string ReReport()
        {
            try
            {
                DirFile.DelectDir($@"{ProjectSite}data\result\excel");
                DirFile.DelectDir($@"{ProjectSite}data\result\debug");
                DirFile.DelectDir($@"{ProjectSite}data\result\image");
                DirFile.DelectDir($@"{ProjectSite}data\script\environinit\tempDate");
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            return "已删除失败脚本相关文件" + $@"{ProjectSite}data\result\image";

        }

        #endregion

        #region 上传未通过
        /// <summary>
        /// 上传未通过
        /// </summary>
        /// <param name="stm">服务端发来未通过信息</param>
        /// <param name="gnpy">任务渠道</param>
        /// <param name="appSrc">时间戳文件夹</param>
        public string Uploa(string stm, string gnpy, string appSrc)
        {
            if (stm == "无未通过报告")
            {
                return "无未通过报告";
            }
            var tempStr = stm.Substring(0, stm.Length - 1).Split(',', '-', ':');

            try
            {
                for (var i = 0; i < tempStr.Length; i += 3)
                {

                    if (DirFile.IsExistFile($@"{ProjectSite}data\script\environinit\tempDate\{tempStr[i + 1]}.xlsx"))
                    {
                        var destFilePath = $@"{ApkSite}{gnpy}\{appSrc}\excel\{tempStr[i]}";

                        RunCmdPython($@"{ProjectSite}main\pngzip\pngzip.py", $@"{ProjectSite}data\result\image\{tempStr[i + 1]}");

                        DirFile.CopyFolder($@"{ProjectSite}data\result\image\{tempStr[i + 1]}", $@"{destFilePath}\操作步骤图片");
                        DirFile.Copy($@"{ProjectSite}data\result\excel\{tempStr[i + 2]}.xlsx", $@"{destFilePath}\{tempStr[i + 2]}.xlsx");
                    }
                    else if (DirFile.IsExistDirectory($@"{ProjectSite}data\result\image\{tempStr[i + 1]}"))
                    {
                        RunCmdPython($@"{ProjectSite}main\pngzip\pngzip.py", $@"{ProjectSite}data\result\image\{tempStr[i + 1]}");
                        DirFile.CopyFolder(
                            $@"{ProjectSite}data\result\image\{tempStr[i + 1]}",
                            $@"{ApkSite}{gnpy}\{appSrc}\failed\{tempStr[i]}");
                    }
                }

            }

            catch (Exception)
            {
                // ignored
            }

            DirFile.CopyFolder($@"{ProjectSite}data\script\environinit\tempDate", $@"{ApkSite}{gnpy}\{appSrc}\tempDate");
            return "上传完成";


        }
        #endregion

        #region 更换服务端图片

        /// <summary>
        /// 设置click_login图片
        /// </summary>
        /// <param name="ranking"></param>
        /// <param name="gnpy"></param>
        public void SetImage(int ranking, string gnpy)
        {
            string[] resolution = { "720x1280", "1080x1920", "1440x2560" };

            if (ranking == 1)
            {
                switch (gnpy)
                {
                    case "sjmy":

                        foreach (var variable in resolution)
                        {
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\template8.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\current.png");
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\select8.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\present.png");
                        }
                        break;

                    case "xsjmy":

                        foreach (var variable in resolution)
                        {
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\sjmy_xsj_first.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\3.png");
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\sjmy_xsj_firstsmall.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\5.png");
                        }
                        break;


                    case "myht":
                        foreach (var variable in resolution)
                        {
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\out10.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\current.png");
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\in10.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\present.png");
                        }
                        break;
                }
            }
            else
            {
                switch (gnpy)
                {
                    case "sjmy":

                        foreach (var variable in resolution)
                        {
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\template7.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\current.png");
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\select7.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\present.png");
                        }
                        break;
                    case "xsjmy":

                        foreach (var variable in resolution)
                        {
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\sjmy_xsj_second.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\3.png");
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\sjmy_xsj_secondsmall.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\5.png");
                        }
                        break;

                    case "myht":

                        foreach (var variable in resolution)
                        {
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\out11.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\current.png");
                            DirFile.Copy(
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\in11.png",
                                $@"{ProjectSite}data\script\common\Click_Login{variable}\present.png");
                        }
                        break;
                }

            }
        }

        #endregion


        /// <summary>
        /// 获取当前时间，格式"yyyy-MM-dd HH:mm:ss    "
        /// </summary>
        /// <returns></returns>
        public static string GetTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss    ");
        }

        public static void AdbGuard()
        {
            var startInfo =
                new ProcessStartInfo("adb.exe", "devices") { CreateNoWindow = true, UseShellExecute = false };
            var proce = Process.Start(startInfo);
            proce?.WaitForExit();

        }

    }


}
