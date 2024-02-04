using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MegaMariPrac
{
    class ProcessMemory
    {
        #region process & address
        //function imports
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        //access values
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        //process handle
        static Process p = Process.GetProcessesByName("megamari")[0];
        IntPtr processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, p.Id);
        public static int FirstProcessModuleMemorySize;

        //amount of bytes written/read
        private int bytesWritten = 0;
        private int bytesRead = 0;
        #endregion

        public ProcessMemory()
        {
            new Thread(ProcessRun) { IsBackground = true }.Start(); //starts thread with method ProcessRun
            ProcessModule pm = p.Modules[0];
            FirstProcessModuleMemorySize = pm.ModuleMemorySize;
            Console.WriteLine("ModuleMemorySize: " + pm.ModuleMemorySize);
        }

        void ProcessRun() //checks if program is running
        {
            while (true)
            {
                if (p.HasExited)
                    Environment.Exit(1);
                Thread.Sleep(50);
            }
        }

        public void WriteStatic(int address, byte[] buffer)
        {
            WriteProcessMemory((int)processHandle, (int)p.Modules[0].BaseAddress + address, buffer, buffer.Length, ref bytesWritten);
        }

        public byte[] ReadStatic(int address, byte[] buffer)
        {
            ReadProcessMemory((int)processHandle, (int)p.Modules[0].BaseAddress + address, buffer, buffer.Length, ref bytesRead);
            return buffer;
        }

        public void Write(int first_off, int last_off, byte[] value)
        {
            byte[] buffer = new byte[4];

            //read address pointed by the game + first initial offset -> equivalent to 'Game.exe+first offset' in cheat engine
            ReadProcessMemory((int)processHandle, (int)p.Modules[0].BaseAddress + first_off, buffer, buffer.Length, ref bytesRead);
            IntPtr curAdd = (IntPtr)BitConverter.ToInt32(buffer, 0);

            //offsetting new pointer
            curAdd += last_off;

            //writing value from the new pointer address
            WriteProcessMemory((int)processHandle, (int)curAdd, value, value.Length, ref bytesWritten);
        }

        public byte[] Read(int first_off, int last_off)
        {
            byte[] buffer = new byte[4];

            //read address pointed by the game + first initial offset -> equivalent to 'Game.exe+first offset' in cheat engine
            ReadProcessMemory((int)processHandle, (int)p.Modules[0].BaseAddress + first_off, buffer, buffer.Length, ref bytesRead);
            IntPtr curAdd = (IntPtr)BitConverter.ToInt32(buffer, 0);

            //offsetting new pointer
            curAdd += last_off;

            //reading value from the new pointer address
            ReadProcessMemory((int)processHandle, (int)curAdd, buffer, buffer.Length, ref bytesRead);

            //return read value
            return buffer;
        }
    }
}
