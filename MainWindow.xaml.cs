using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Serilog;
using System;

namespace EFT_Goodies
{
    public sealed partial class MainWindow : Window
    {
        internal bool isProcessFound = false;
        internal bool isModuleBassAddressFound = false;
        private bool isMainWindowFirstActivation = true;
        private readonly object logLockToken = new object();

        private const string processName = "EscapeFromTarkov.exe";
        private const string moduleName = "GameAssembly.dll";

        private DispatcherTimer gameLoopTimer = null!;
        private LeechFPGA gameAssemblyDllLeech = null!;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed; 
            this.Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (isMainWindowFirstActivation)
            {
                isMainWindowFirstActivation = false;

                try
                {
                    // initialize the leech for the GameAssembly.dll
                    gameAssemblyDllLeech = new LeechFPGA(this, processName, moduleName);
                }
                catch (Exception ex)
                {
                    LogLine(ex.ToString());
                    LogLine("Failed to initialize EFT Goodies application, Check DMA card configuration and restart!");
                    return;
                }

                // Initialize log file. Rolls every day, Keeps only the last 7 days of logs
                Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File("logs/EFTG.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7).CreateLogger();

                // Initialize and start the main loop timer
                gameLoopTimer = new DispatcherTimer();
                gameLoopTimer.Interval = TimeSpan.FromMilliseconds(10);
                gameLoopTimer.Tick += MainLoopTick;
                gameLoopTimer.Start();

                LogLine("EFT Goodies application started...");
                LogLine("Waiting for " + processName);
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // PUT YOUR CLEANUP CODE HERE
            Log.CloseAndFlush();
        } 

        private void MainLoopTick(object? sender, object? e)
        {
            // PUT YOUR MAIN LOOP LOGIC HERE

            // Waiting for the process and module to be found before doing anything
            if (!isProcessFound)
            {
                gameAssemblyDllLeech.getProcesss();
                return;
            }
            if (!isModuleBassAddressFound)
            {
                gameAssemblyDllLeech.getModuleBaseAddress();
                return;
            }
        }

        public void LogLine(string message)
        {
            lock (logLockToken)
            {
                UiConsoleText.Text += message + Environment.NewLine;
                Log.Information(message);
                ScrollTextBoxToBottom();
            }
        }

        private void ScrollTextBoxToBottom()
        {
            var scrollViewer = GetScrollViewer(UiConsoleText);
            if (scrollViewer != null)
            {
                // Scroll to the very bottom (ScrollableHeight)
                scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
            }
        }

        private ScrollViewer? GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer scrollViewer) return scrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}

/* test code for Serious Sam 3
 
 private const string processName = "Sam3.exe";
 private const string moduleName = "Sam3.exe";
 private UInt32[] offsets = { 0x50, 0x20, 0x30, 0x450 };
 private UInt32 healthAddr = 0;
 private UInt32 healthvalue = 0;
 
 // in main loop
 healthAddr = leech.getDMAAddress32(0x00B92290, offsets);
 healthAddr -= 0x10;
 healthvalue = leech.getUInt32(healthAddr);
 LogLine(healthAddr.ToString("X"));
 LogLine(healthvalue.ToString());
*/
