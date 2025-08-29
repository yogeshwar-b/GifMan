using FFMpegCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GifMan
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var hWnd = WindowNative.GetWindowHandle(this);

            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                appWindow.Resize(new SizeInt32(displayArea.WorkArea.Width / 2, displayArea.WorkArea.Height / 2));
            }


            if (System.Diagnostics.Debugger.IsAttached)
            {
                FilePathTextBox.Text = @"D:\Dev\Projects\GifMan\TestVideos\Big_Buck_Bunny_1080_10s_30MB.mkv";
            }


        }

        private async void FilePathSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.FileTypeFilter.Add(".mp4");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                FilePathTextBox.Text = file.Path;
            }
            else
            {
                FilePathTextBox.Text = "No file selected";
            }
        }

        private async void ProcessFileButton_Click(object sender, RoutedEventArgs e)
        {
            string outputPath = Path.GetDirectoryName(FilePathTextBox.Text) ?? String.Empty;

            await CreateGif_fromDisk(
                    FilePathTextBox.Text,
                    Path.Combine(Path.GetDirectoryName(FilePathTextBox.Text), "output.gif"),
                    LoadingTextBlock,
                    this.DispatcherQueue);


        }

        private async Task CreateGif_fromDisk(string InputFilePath, string OutPutGifFileName, TextBlock loadingTextBlock, DispatcherQueue dispatcherQueue)
        {
            string outputPath = Path.GetDirectoryName(InputFilePath) ?? String.Empty;
            if (!string.IsNullOrEmpty(outputPath))
            {
                loadingTextBlock.Text = "Processing....";
                var mediaInfo = await FFProbe.AnalyseAsync(InputFilePath);

                TimeSpan totalDuration = mediaInfo.Duration;
                double videoFrameRate = mediaInfo.PrimaryVideoStream?.AvgFrameRate ?? 0;
                await FFMpegArguments
                      .FromFileInput(InputFilePath)
                      .OutputToFile(outputPath + "/output/output_%04d.png", overwrite: true, options => options
                          .WithCustomArgument($"-vf fps={videoFrameRate}")
                          .ForceFormat("image2"))
                        .NotifyOnProgress(progress =>
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                loadingTextBlock.Text = ($"Progress: {progress}%");
                            });
                        }, totalDuration)
                                                    .ProcessAsynchronously();

                loadingTextBlock.Text = "Done.";



            }
        }
    }
}

