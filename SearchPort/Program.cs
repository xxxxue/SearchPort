using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SearchPort
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                var (result, portInfoArr) = GetPortInfo();
                if (result)
                {
                    ShowPortInfo(portInfoArr);
                }

                Console.WriteLine("是否退出?(y/n)");
                var inputValue = Console.ReadKey();
                if (inputValue.Key == ConsoleKey.Y)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 获取端口信息
        /// </summary>
        /// <returns></returns>
        private static Tuple<bool, string[]> GetPortInfo()
        {
            Console.WriteLine("请输入端口号:(Enter键确认)");
            // 读取用户输入的端口号
            var inputPort = Console.ReadLine();
            if (string.IsNullOrEmpty(inputPort))
            {
                Console.WriteLine("端口号为空,请检查.");
                return new Tuple<bool, string[]>(false, null);
            }

            // 拼接命令
            var cmdStr = "netstat -ano | findstr " + inputPort;

            // 执行命令,获取返回值
            var cmdRes = RunCmd(cmdStr);

            // 格式化
            var arr = cmdRes.Split(" ");
            
            arr = arr
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Replace("\r\n", "")).ToArray();

            if (arr.Length == 0)
            {
                Console.WriteLine("该端口没有被程序占用.");
                return new Tuple<bool, string[]>(false, null);
            }

            return new Tuple<bool, string[]>(true, arr);
        }

        /// <summary>
        /// 显示端口信息
        /// </summary>
        /// <param name="arr"></param>
        private static void ShowPortInfo(IReadOnlyList<string> arr)
        {
            for (var i = 0; i < arr.Count; i += 5)
            {
                // 防止数组越界 与 数据过多
                if (i + 4 > arr.Count || i + 3 > arr.Count || i + 2 > arr.Count || i + 1 > arr.Count || i > 80)
                {
                    break;
                }

                Console.WriteLine("-----------------");
                Console.WriteLine("PID:  " + arr[i + 4]);
                Console.WriteLine("\t类型:" + arr[i]);
                Console.WriteLine("\t内部地址:" + arr[i + 1]);
                Console.WriteLine("\t外部地址:" + arr[i + 2]);
                Console.WriteLine("\t状态:" + arr[i + 3]);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string RunCmd(string cmd)
        {
            cmd = cmd.Trim().TrimEnd('&') + "&exit";
            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.StandardInput.WriteLine(cmd);
            process.StandardInput.AutoFlush = true;

            var outStr = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();

            var startStrLength = outStr.IndexOf(cmd) + cmd.Length + "\r\n".Length;
            // 截取
            outStr = outStr[startStrLength..];
            return outStr;
        }

    }
}