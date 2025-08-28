using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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
                FilePathTextBox.Text = @"C:\Dev\Projects\GifMan\Project\GifMan\TestVideos\salaar_axe.mp4";
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

            await CreateGif(
                    FilePathTextBox.Text,
                    Path.Combine(Path.GetDirectoryName(FilePathTextBox.Text), "output.gif"),
                    LoadingTextBlock,
                    this.DispatcherQueue);


        }

        public static async Task CreateGif(string inputFile, string outputGif, TextBlock loadingBlock, DispatcherQueue dispatcher)
        {
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile);
            TimeSpan totalDuration = mediaInfo.Duration;

            int width = mediaInfo.PrimaryVideoStream.Width;
            int height = mediaInfo.PrimaryVideoStream.Height;
            int frameSize = width * height * 4;

            var settings = new GifskiNative.GifskiSettings
            {
                quality = 90,
                fast = false,
                repeat = 0
            };
            IntPtr encoder = GifskiNative.gifski_new(ref settings);
            GifskiNative.gifski_set_file_output(encoder, outputGif);

            var ms = new MemoryStream();
            var sink = new StreamPipeSink(ms);

            await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToPipe(sink, options => options
                    .WithCustomArgument("-vf fps=30")
                    .WithCustomArgument("-f rawvideo")
                    .WithCustomArgument("-pix_fmt rgba"))
                .NotifyOnProgress(progress =>
                {
                    dispatcher?.TryEnqueue(() =>
                    {
                        loadingBlock.Text = $"Progress: {progress}%";
                    });
                }, totalDuration)
                .ProcessAsynchronously();

            ms.Position = 0;
            byte[] buffer = new byte[frameSize];
            uint frameNumber = 0;

            while (ms.Read(buffer, 0, frameSize) == frameSize)
            {
                double pts = frameNumber / 30.0;
                GifskiNative.gifski_add_frame_rgba(
                    encoder,
                    frameNumber,
                    (uint)width,
                    (uint)height,
                    buffer,
                    pts
                );
                frameNumber++;
            }

            GifskiNative.gifski_finish(encoder);

            dispatcher?.TryEnqueue(() =>
            {
                loadingBlock.Text = "Done.";
            });
        }
    }
}
