using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;


namespace MultiThreadedApp
{
    public partial class MainWindow : Window
    {
        private bool isRunning = false;
        private Thread numberThread;
        private Thread letterThread;
        private Thread symbolThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartThreads_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;

                ThreadPriority priority = ThreadPriority.Normal;
                Dispatcher.Invoke(() =>
                {
                    switch (PriorityComboBox.SelectedIndex)
                    {
                        case 0: priority = ThreadPriority.Lowest; break;
                        case 1: priority = ThreadPriority.BelowNormal; break;
                        case 3: priority = ThreadPriority.AboveNormal; break;
                        case 4: priority = ThreadPriority.Highest; break;
                    }
                });

                numberThread = new Thread(() => GenerateNumbers(priority));
                letterThread = new Thread(() => GenerateLetters(priority));
                symbolThread = new Thread(() => GenerateSymbols(priority));

                numberThread.Start();
                letterThread.Start();
                symbolThread.Start();
            }
        }

        private void StopThreads_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            numberThread?.Abort();
            letterThread?.Abort();
            symbolThread?.Abort();
        }

        private void GenerateNumbers(ThreadPriority priority)
        {
            Random rand = new Random();

            while (isRunning)
            {
                Dispatcher.Invoke(() => OutputTextBox.AppendText(rand.Next(10).ToString() + " "), System.Windows.Threading.DispatcherPriority.Render);
                Thread.Sleep(1000);
            }
        }

        private void GenerateLetters(ThreadPriority priority)
        {
            Random rand = new Random();

            while (isRunning)
            {
                char letter = (char)('A' + rand.Next(26));
                Dispatcher.Invoke(() => OutputTextBox.AppendText(letter.ToString() + " "), System.Windows.Threading.DispatcherPriority.Render);
                Thread.Sleep(1000);
            }
        }

        private void GenerateSymbols(ThreadPriority priority)
        {
            Random rand = new Random();

            while (isRunning)
            {
                char symbol = (char)rand.Next(33, 127);
                Dispatcher.Invoke(() => OutputTextBox.AppendText(symbol.ToString() + " "), System.Windows.Threading.DispatcherPriority.Render);
                Thread.Sleep(1000);
            }
        }
    }
}
