using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace FileCopyApp
{
    public partial class MainWindow : Window
    {
        private string sourceFilePath;
        private string destinationFolderPath;
        private int numberOfThreads = 1;
        private long totalFileSize;
        private long copiedFileSize;
        private bool isCopyingPaused = false;
        private bool isCopyingStopped = false;

        public MainWindow()
        {
            InitializeComponent();
            // Обработчик события закрытия окна для завершения копирования приложения.
            this.Closing += MainWindow_Closing;
        }

        private async void SelectSourceFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                sourceFilePath = openFileDialog.FileName;
                SourceFileTextBox.Text = sourceFilePath;
                FileInfo fileInfo = new FileInfo(sourceFilePath);
                totalFileSize = fileInfo.Length;
            }
        }

        private async void SelectDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    destinationFolderPath = folderDialog.SelectedPath;
                    DestinationFolderTextBox.Text = destinationFolderPath;
                }
            }
        }

        private async void StartCopying_Click(object sender, RoutedEventArgs e)
        {
            isCopyingPaused = false;
            isCopyingStopped = false;

            if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationFolderPath))
            {
                MessageBox.Show("Please select source file and destination folder.");
                return;
            }

            if (!File.Exists(sourceFilePath))
            {
                MessageBox.Show("Source file does not exist.");
                return;
            }

            numberOfThreads = (int)ThreadSlider.Value;
            ProgressBar.Maximum = totalFileSize;
            copiedFileSize = 0;

            ProgressBar.Visibility = Visibility.Visible;
            await CopyFileAsync(sourceFilePath, destinationFolderPath, numberOfThreads);
            ProgressBar.Visibility = Visibility.Hidden;

            MessageBox.Show("File copied successfully!");
        }

        private void PauseCopying_Click(object sender, RoutedEventArgs e)
        {
            isCopyingPaused = !isCopyingPaused;
            // Обновляем текст на кнопке "Пауза" в зависимости от состояния.
            PauseCopyingButton.Content = isCopyingPaused ? "Продолжить" : "Пауза";
        }

        private void StopCopying_Click(object sender, RoutedEventArgs e)
        {
            isCopyingStopped = true;
        }

        private async Task CopyFileAsync(string sourceFilePath, string destinationFolderPath, int numberOfThreads)
        {
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                var tasks = new Task[numberOfThreads];
                long blockSize = totalFileSize / numberOfThreads;

                for (int i = 0; i < numberOfThreads; i++)
                {
                    if (isCopyingStopped)
                    {
                        return;
                    }

                    while (isCopyingPaused)
                    {
                        await Task.Delay(500);
                    }

                    long startOffset = i * blockSize;
                    long endOffset = (i == numberOfThreads - 1) ? totalFileSize : (i + 1) * blockSize;
                    tasks[i] = CopyBlockAsync(sourceStream, Path.Combine(destinationFolderPath, System.IO.Path.GetFileName(sourceFilePath)), startOffset, endOffset);
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task CopyBlockAsync(FileStream sourceStream, string destinationPath, long startOffset, long endOffset)
        {
            int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];

            using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                sourceStream.Seek(startOffset, SeekOrigin.Begin);

                while (startOffset < endOffset)
                {
                    if (isCopyingStopped)
                    {
                        return;
                    }

                    while (isCopyingPaused)
                    {
                        await Task.Delay(500);
                    }

                    int bytesRead = await sourceStream.ReadAsync(buffer, 0, bufferSize);
                    if (bytesRead == 0)
                        break;

                    await destinationStream.WriteAsync(buffer, 0, bytesRead);
                    startOffset += bytesRead;
                    copiedFileSize += bytesRead;

                    Dispatcher.Invoke(() => ProgressBar.Value = copiedFileSize, System.Windows.Threading.DispatcherPriority.Render);
                }
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Завершаем копирование перед закрытием окна.
            isCopyingStopped = true;
        }
    }
}

