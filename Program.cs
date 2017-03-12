using System;
using System.Runtime.InteropServices;

namespace CapsToggler
{
    public unsafe class Program
    {
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(String DllName);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, String ProcName);

        [DllImport("kernel32")]
        private extern static bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool InitializeWinIoType();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetPortValType(UInt16 PortAddr, UInt32* pPortVal, UInt16 Size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SetPortValType(UInt16 PortAddr, UInt32 PortVal, UInt16 Size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ShutdownWinIoType();

        static IntPtr hMod;


        private static void load()
        {
            // Check if this is a 32 bit or 64 bit system
            if (IntPtr.Size == 4)
            {
                hMod = LoadLibrary("WinIo32.dll");

            }
            else if (IntPtr.Size == 8)
            {
                hMod = LoadLibrary("WinIo64.dll");
            }

            if (hMod == IntPtr.Zero)
            {
                Console.WriteLine("Can't find WinIo dll.\nMake sure the WinIo library files are located in the same directory as your executable file.");
            }

            IntPtr pFunc = GetProcAddress(hMod, "InitializeWinIo");

            if (pFunc != IntPtr.Zero)
            {
                InitializeWinIoType InitializeWinIo = (InitializeWinIoType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitializeWinIoType));
                bool Result = InitializeWinIo();

                if (!Result)
                {
                    Console.WriteLine("Error returned from InitializeWinIo.\nMake sure you are running with administrative privileges and that the WinIo library files are located in the same directory as your executable file.");
                    FreeLibrary(hMod);
                }
            }
            
        }

        private static void setValue(String portVal)
        {
            IntPtr pFunc = GetProcAddress(hMod, "SetPortVal");

            if (pFunc != IntPtr.Zero)
            {
                UInt16 PortAddr;
                UInt32 PortVal;

                PortAddr = UInt16.Parse("60", System.Globalization.NumberStyles.HexNumber);
                PortVal = UInt32.Parse(portVal, System.Globalization.NumberStyles.HexNumber);
                Console.WriteLine("Port: " + PortAddr + " " + "60" + " Val: " + PortVal + " " + portVal);

                SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(SetPortValType));

                // Call WinIo to set value
                bool Result = SetPortVal(PortAddr, PortVal, 1);

                if (!Result)
                {
                    Console.WriteLine("Error returned from SetPortVal");
                }
                Console.WriteLine("Success");
            }
        }

        static void interactive()
        {
            while (true)
            {
                Console.WriteLine("Type port value or q to quit:");
                var portVal = Console.ReadLine();
                if (portVal == "q")
                {
                    Console.WriteLine("Exiting...");
                    break;
                }
                setValue(portVal);
            }
        }

        static void loop()
        {
            String[] portVals = { "ed", "0", "ed", "4", "ed", "0" };
            for (var x = 1; x <= 5; x++)
            {

                Console.WriteLine("==============================");
                Console.WriteLine("Cycle " + x);
                for(var i = 0; i < 3; i++)
                {
                    for (var j = 0; j< 2; j++)
                    {
                        var port = portVals[i * 2 + j];
                        Console.WriteLine("Writing " + port + " at the adress 60h");
                        setValue(port);
                        System.Threading.Thread.Sleep(500);
                    }
                }
                Console.WriteLine("End Of Cycle " + x);
                Console.WriteLine("==============================");
                System.Threading.Thread.Sleep(1000);
            }
        }
        
        static void Main(string[] args)
        {
            load();
            loop();
        }
    }
}
