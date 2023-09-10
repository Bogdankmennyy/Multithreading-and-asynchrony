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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectSourceFile_Click(object sender, RoutedEventArgs e)
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

     

private void SelectDestinationFolder_Click(object sender, RoutedEventArgs e)
    {
        var folderDialog = new OpenFileDialog
        {
            Title = "Select Destination Folder",
            CheckFileExists = false,
            FileName = "Folder Selection.",
            Filter = "Folder|no.files"
        };

        if (folderDialog.ShowDialog() == true)
        {
            destinationFolderPath = System.IO.Path.GetDirectoryName(folderDialog.FileName);
            DestinationFolderTextBox.Text = destinationFolderPath;
        }
    }




    private async void StartCopying_Click(object sender, RoutedEventArgs e)
        {
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

        private async Task CopyFileAsync(string sourceFilePath, string destinationFolderPath, int numberOfThreads)
        {
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                var tasks = new Task[numberOfThreads];
                long blockSize = totalFileSize / numberOfThreads;

                for (int i = 0; i < numberOfThreads; i++)
                {
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
    }
}

