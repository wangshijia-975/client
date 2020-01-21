// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunCmd.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the RunCmd type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace 客户端
{
    using System.Diagnostics;

    public class RunCmd
    {
        private readonly Process proc = null;

        public RunCmd()
        {
            proc = new Process();
        }

        public void Exe(string cmd)
        {
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            //proc.OutputDataReceived += sortProcess_OutputDataReceived;
            proc.Start();
            var cmdWriter = proc.StandardInput;
            proc.BeginOutputReadLine();

            if (!string.IsNullOrEmpty(cmd))
            {
                cmdWriter.WriteLine(cmd + "&exit");
            }

            cmdWriter.Close();
            proc.WaitForExit();
            proc.Close();
                       
        }

        public static void AddlistDevices(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
            }
        }

        public string adb(string cmd)
        {
            var AdbResult = string.Empty;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        AdbResult = AdbResult + "\n" + e.Data;
                    }
                };
            proc.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        AdbResult = AdbResult + "\n" + e.Data;
                    }
                };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            if (!string.IsNullOrEmpty(cmd))
            {
                proc.StandardInput.WriteLine(cmd + "&exit");
            }
            proc.StandardInput.Close();
            proc.WaitForExit();
            proc.Close();
            return AdbResult;
        }


    }
}
