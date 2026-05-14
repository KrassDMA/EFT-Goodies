using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Serilog;
using System;

namespace EFT_Goodies
{
    public sealed partial class MainWindow : Window
    {
        private DispatcherTimer gameLoopTimer = null!;
        LeechFPGA leech = null!;
        private const string processName = "Sam3.exe";
        private const string moduleName = "Sam3.exe";
        
        public MainWindow()
        {
            InitializeComponent();
            InitLogFile();
            InitMainLoop();
            this.Closed += MainWindow_Closed;
            LogLine("EFT Goodies application started...");

            leech = new LeechFPGA(this, processName, moduleName);
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // PUT YOUR CLEANUP CODE HERE
            Log.CloseAndFlush();
        }

        private void InitMainLoop()
        {
            gameLoopTimer = new DispatcherTimer();
            gameLoopTimer.Interval = TimeSpan.FromMilliseconds(10);
            gameLoopTimer.Tick += MainLoopTick;
            gameLoopTimer.Start();
        }   

        private void MainLoopTick(object? sender, object? e)
        {
            // PUT YOUR MAIN LOOP LOGIC HERE

            
        }

        private void InitLogFile()
        {
            // Rolls every day, Keeps only the last 7 days of logs
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File("logs/EFTG.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7).CreateLogger();
        }

        public void LogLine(string message)
        {
            UiConsoleText.Text += message + Environment.NewLine;
            Log.Information(message);
            ScrollTextBoxToBottom();
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

/*
        private UInt32[] offsets = { 0x50, 0x20, 0x30, 0x450 };
        private UInt32 healthAddr = 0;
        private UInt32 healthvalue = 0;
            healthAddr = leech.getDMAAddress32(0x00B92290, offsets);
            healthAddr -= 0x10;
            healthvalue = leech.getUInt32(healthAddr);
            LogLine(healthAddr.ToString("X"));
            LogLine(healthvalue.ToString());
 
*/
