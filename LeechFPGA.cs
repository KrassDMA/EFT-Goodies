using System;
using System.Threading.Tasks;
using Vmmsharp;

namespace EFT_Goodies
{
    internal class LeechFPGA
    {
        private MainWindow mw;
        private string processName;
        private string moduleName;
        private Vmm vmm = null!;
        private VmmProcess process = null!;
        private UInt64 moduleBaseAddress = 0;
        private readonly object mwLockToken = new object();

        internal LeechFPGA(MainWindow mainWindow, string processName, string moduleName)
        {
            this.mw = mainWindow;
            this.processName = processName;
            this.moduleName = moduleName;

            try
            {
                // Preload native libraries
                LeechCore.LoadNativeLibrary(System.AppDomain.CurrentDomain.BaseDirectory);
                Vmm.LoadNativeLibrary(System.AppDomain.CurrentDomain.BaseDirectory);
            }
            catch
            {
                mw.LogLine("Failed to load native libraries, Check for missing DLLs leechcore.dll and vmm.dll.");
                throw;
            }

            try
            {
                // Initialize a Vmm for FPGA card
                vmm = new Vmm("-device", "fpga");
                mw.LogLine("LeechFPGA initialized!");
            }
            catch
            {
                mw.LogLine("LeechFPGA initialization failed, check DMA card configuration and restart!");
                throw;
            }
        }

        internal async Task getProcesss()
        {
            try
            {
                // Get process
                process = vmm.Process(processName);
                if (process != null)
                {
                    mw.LogLine("Found " + processName);
                    lock (mwLockToken)
                    {
                        mw.isProcessFound = true;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                mw.LogLine(ex.ToString());
                return;
            }
        }

        internal async Task getModuleBaseAddress()
        {
            try
            {
                // Get module base address
                moduleBaseAddress = process.GetModuleBase(moduleName);
                if(moduleBaseAddress == 0)
                {
                    return;
                }
                else
                {
                    mw.LogLine(moduleName + " base address = " + moduleBaseAddress.ToString("X16"));
                    lock (mwLockToken)
                    {
                        mw.isModuleBassAddressFound = true;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                mw.LogLine(ex.ToString());
                return;
            }
        }

        internal UInt64 getDMAAddress64(UInt64 relativeOffset, UInt64[] offsets)
        {
            UInt64 address = (UInt64)moduleBaseAddress + relativeOffset;
            byte[] memReadData;

            try
            {
                for (UInt32 i = 0; i < offsets.Length; i++)
                {
                    memReadData = process.MemRead(address, 8);
                    address = BitConverter.ToUInt64(memReadData, 0);
                    address += offsets[i];
                }
                return address;
            }
            catch (Exception ex)
            {
                mw.LogLine("Failed LeechFPGA.getDMAAddress64()");
                mw.LogLine(ex.ToString());
                return 0;
            }
        }

        internal UInt64 getUInt64(UInt64 address)
        {
            try
            {
                byte[] memReadData = process.MemRead(address, 8);
                return BitConverter.ToUInt64(memReadData, 0);
            }
            catch (Exception ex)
            {
                mw.LogLine("Failed LeechFPGA.getUInt64()");
                mw.LogLine(ex.ToString());
                return 0;
            }
        }

        internal UInt32 getDMAAddress32(UInt32 relativeOffset, UInt32[] offsets)
        {
            UInt32 address = (UInt32)moduleBaseAddress + relativeOffset;
            byte[] memReadData;

            try
            {
                for (UInt32 i = 0; i < offsets.Length; i++)
                {
                    memReadData = process.MemRead(address, 4);
                    address = BitConverter.ToUInt32(memReadData, 0);
                    address += offsets[i];
                }
                return address;
            }
            catch (Exception ex)
            {
                mw.LogLine("Failed LeechFPGA.getDMAAddress32()");
                mw.LogLine(ex.ToString());
                return 0;
            }   
        }

        internal UInt32 getUInt32(UInt32 address)
        { 
            try
            {
                byte[] memReadData = process.MemRead(address, 4);
                return BitConverter.ToUInt32(memReadData, 0);
            }
            catch (Exception ex)
            {
                mw.LogLine("Failed LeechFPGA.getUInt32()");
                mw.LogLine(ex.ToString());
                return 0;
            }
        }

    }
}


/*
 * uint32_t LeechFPGA::getDMAAddress(uint32_t relativeOffset, std::vector<unsigned int> offsets) {
    uint32_t addr = moduleBaseAddr + relativeOffset;
    for (unsigned int i = 0; i < offsets.size(); ++i) {
        result = VMMDLL_MemRead(hVMM, dwPID, addr, pbPage1, sizeof(addr));
        if (result) {
            std::memcpy(&addr, pbPage1, sizeof(addr));
            addr += offsets[i];
        } else {
            std::cout << std::uppercase << std::hex << "FAIL: VMMDLL_MemRead() - PID:" << dwPID << std::endl;
        }
    }
    return addr;
}
 * 
 * 
 *  // Example: vmmprocess.GetModuleBase():
            // Retrieve the base address of a module.
            Console.WriteLine("====================================");
            Console.WriteLine("VmmProcess.GetModuleBase():");
            ulong vaModuleBaseAddress = explorerProcess.GetModuleBase("kernel32.dll");
            Console.WriteLine("Base address of kernel32.dll: {0:X}", vaModuleBaseAddress);

  // Example: vmmprocess.MemRead():
            // Read 0x100 bytes from beginning of explorer.exe!kernel32.dll with vmm flags.
            Console.WriteLine("====================================");
            Console.WriteLine("VmmProcess.MemRead() [flags]:");
            byte[] memReadDataFlags = explorerProcess.MemRead(kernel32.vaBase, 0x100, Vmm.FLAG_NOCACHE | Vmm.FLAG_ZEROPAD_ON_FAIL);
            Console.WriteLine("Read from explorer.exe!kernel32.dll: \n{0}", Vmm.UtilFillHexAscii(memReadDataFlags));






// Example: vmmprocess.Search() #1: - asynchronous.
            // Search process virtual memory efficiently.
            // Search whole address space in asynchronous non-blocking mode and update.
            // Search max 0x10000 hits (max allowed).
            Console.WriteLine("====================================");
            Console.WriteLine("VmmProcess.Search() #1 [async]:");
            byte[] SEARCH1_TERM1 = { 0x4D, 0x5A }; // MZ
            VmmSearch search1 = explorerProcess.Search();
            VmmSearch.SearchResult search1Result;
            uint search1TermId = search1.AddSearch(SEARCH1_TERM1, null, 0x1000);
            Console.WriteLine("search term with id={0} added.", search1TermId);
            search1.Start();
            while (true)
            {
                search1Result = search1.Poll();
                Console.WriteLine("search poll status: completed={0} va_current={1:x} read_bytes={2:x} results={3}", search1Result.isCompleted, search1Result.addrCurrent, search1Result.totalReadBytes, search1Result.result.Count);
                if (search1Result.isCompleted)
                {
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
            search1Result = search1.Result();
            Console.WriteLine("search result: completed={0} success={4} va_current={1:x} read_bytes={2:x} results={3}", search1Result.isCompleted, search1Result.addrCurrent, search1Result.totalReadBytes, search1Result.result.Count, search1Result.isCompletedSuccess);
            foreach (VmmSearch.SearchResultEntry search1ResultEntry in search1Result.result)
            {
                Console.Write("{0:X}({1})  ", search1ResultEntry.address, search1ResultEntry.search_term_id);
            }
            Console.WriteLine();


            // Example: vmmprocess.Search() #2: - synchronous.
            // Search process virtual memory efficiently.
            // Search whole address space in synchronous blocking mode.
            // Search max 0x10000 hits (max allowed).
            Console.WriteLine("====================================");
            Console.WriteLine("VmmProcess.Search() #2 [sync]:");
            byte[] SEARCH2_TERM1 = { 0x4D, 0x5A }; // MZ
            VmmSearch search2 = explorerProcess.Search(0, ulong.MaxValue, 0x10000, Vmm.FLAG_NOCACHE);
            uint search2TermID = search2.AddSearch(SEARCH2_TERM1, null, 0x1000);
            Console.WriteLine("search term with id={0} added.", search2TermID);
            VmmSearch.SearchResult search2Result = search2.Result();
            Console.WriteLine("search result: completed={0} success={4} va_current={1:x} read_bytes={2:x} results={3}", search2Result.isCompleted, search2Result.addrCurrent, search2Result.totalReadBytes, search2Result.result.Count, search2Result.isCompletedSuccess);
            foreach (VmmSearch.SearchResultEntry search2ResultEntry in search2Result.result)
            {
                Console.Write("{0:X}({1})  ", search2ResultEntry.address, search2ResultEntry.search_term_id);
            }
            Console.WriteLine();


            // Example: vmmprocess.SearchYara() #1: - asynchronous.
            // Search process virtual memory efficiently using a yara signature.
            // Search whole address space in asynchronous non-blocking mode and update.
            // Search max 0x10000 hits (max allowed).
            // In this example a simple yara signature is used to find the string
            // "MZ" at the start of a page.
            // it's also possible to load yara rules from files - in that case
            // specify the full file path instead of the yara rule string.
            Console.WriteLine("====================================");
            Console.WriteLine("VmmProcess.SearchYara() #1 [async]:");
            string YARA_RULE1 = "rule MZ { strings: $mz = \"MZ\" condition: $mz at 0 }";
            VmmYara yara1 = explorerProcess.SearchYara(YARA_RULE1);
            yara1.Start();
            while (true)
            {
                VmmYara.YaraResult yara1Result = yara1.Poll();
                Console.WriteLine("yara poll status: completed={0} va_current={1:x} read_bytes={2:x} results={3}", yara1Result.isCompleted, yara1Result.addrCurrent, yara1Result.totalReadBytes, yara1Result.result.Count);
                if (yara1Result.isCompleted)
                {
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
            VmmYara.YaraResult yara1ResultFinal = yara1.Result();
            Console.WriteLine("yara result: completed={0} success={4} va_current={1:x} read_bytes={2:x} results={3}", yara1ResultFinal.isCompleted, yara1ResultFinal.addrCurrent, yara1ResultFinal.totalReadBytes, yara1ResultFinal.result.Count, yara1ResultFinal.isCompletedSuccess);
            foreach (VmmYara.YaraMatch yara1Match in yara1ResultFinal.result)
            {
                Console.Write("rule={0} :: ", yara1Match.sRuleIdentifier);
                foreach (VmmYara.YaraMatchString yaraMatchString in yara1Match.strings)
                {
                    Console.Write("{0}:", yaraMatchString.sString);
                    foreach (ulong yaraMatchAddress in yaraMatchString.addresses)
                    {
                        Console.Write("{0:X},", yaraMatchAddress);
                    }
                }
                Console.WriteLine("");
            }
            #endregion // Process Search and YARA functionality

*/