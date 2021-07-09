using System;
using System.Diagnostics;
using System.Linq;

namespace SearchPort
{
    class Program
    {
        static void Main(string[] args)
        {        
            while (true)
            {
                var res = GetPortInfo();
                if (res.Item1)
                {
                    ShowProtInfo(res.Item2);
                }
                Console.WriteLine("是否退出?(y/n)");
                var inputValue = Console.ReadKey();
                if (inputValue.Key == ConsoleKey.Y)
                {
                    return;
                }
            }
        }

        public static Tuple<bool, string[]> GetPortInfo()
        {

            Console.WriteLine("请输入端口号:(Enter键确认)");
            var inputPort = Console.ReadLine();
            if (string.IsNullOrEmpty(inputPort))
            {
                Console.WriteLine("端口号为空,请检查.");
                return new Tuple<bool, string[]>(false, new string[1]);
            }
            var cmdStr = "netstat -ano | findstr " + inputPort;

            var cmdRes = RunCmd(cmdStr);

            var arr = cmdRes.Split(" ");
            arr = arr
                 .Where(a => !string.IsNullOrWhiteSpace(a))
                 .Select(a =>
                 {
                     return a.Replace("\r\n", "");
                 }).ToArray();

            if (arr.Length == 0)
            {
                Console.WriteLine("该端口没有被程序占用.");
                return new Tuple<bool, string[]>(false, new string[1]);

            }


            return new Tuple<bool, string[]>(true, arr);

        }

        public static void ShowProtInfo(string[] arr)
        {
            for (int i = 0; i < arr.Length; i = i + 5)
            {
                
                // 防止数组越界 与 数据过多
                if (i + 4 > arr.Length || i + 3 > arr.Length || i + 2 > arr.Length || i + 1 > arr.Length || i > 80)
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

        public static string RunCmd(string cmd)
        {
            cmd = cmd.Trim().TrimEnd('&') + "&exit";
            Process process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.StandardInput.WriteLine(cmd);
            process.StandardInput.AutoFlush = true;

            string outStr = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();

            outStr = outStr.Substring(outStr.IndexOf(cmd) + cmd.Length + "\r\n".Length);
            return outStr;
        }
    }
}
