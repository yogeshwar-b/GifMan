using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
            if (!string.IsNullOrEmpty(outputPath))
            {
                LoadingTextBlock.Text = "Processing....";
                var mediaInfo = await FFProbe.AnalyseAsync(FilePathTextBox.Text);
                TimeSpan totalDuration = mediaInfo.Duration;
                await FFMpegArguments
                      .FromFileInput(FilePathTextBox.Text)
                      .OutputToFile(outputPath + "/output_%04d.png", overwrite: true, options => options
                          .WithCustomArgument("-vf fps=30")   // apply fps filter
                          .ForceFormat("image2"))
                        .NotifyOnProgress(progress =>
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                LoadingTextBlock.Text = ($"Progress: {progress}%");
                            });
                        }, totalDuration)
                                                    .ProcessAsynchronously();

                LoadingTextBlock.Text = "Done.";



            }
        }
    }
}
