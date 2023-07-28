using FFMediaToolkit.Decoding;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PoliceWebScraping
{

    public static class VideoScreenshot
    {
        public static void CaptureScreenshots(string inputFilePath, string ffmpegPath = ".\\dll\\ffmpeg.exe", double cropRatio = 0.1, int fps = 8)
        {
            var fileInfo = new FileInfo(inputFilePath);
            string outputDirectory = Path.Combine(fileInfo.Directory.FullName, "NewImage");
            Directory.CreateDirectory(outputDirectory);

            string outputPattern = $"output_{fileInfo.Name}_%d.png";
            string outputFilePath = Path.Combine(outputDirectory, outputPattern);

            string firstFramePath = Path.Combine(outputDirectory, "first_frame.png");
            string firstFrameArguments = $"-i \"{inputFilePath}\" -vf \"select='eq(n,0)'\" -vframes 1 \"{firstFramePath}\"";
            RunFFmpegProcess(ffmpegPath, firstFrameArguments);

            int imageHeight = GetImageHeight(firstFramePath);

            File.Delete(firstFramePath);

            int deleteHeight = (int)(imageHeight * cropRatio);

            string cropFilter = $"crop=iw:{imageHeight - deleteHeight}:0:{deleteHeight}";
            string arguments = $"-i \"{inputFilePath}\" -vf \"{cropFilter}\" -r {fps} \"{outputFilePath}\"";
            RunFFmpegProcess(ffmpegPath, arguments);
        }


        private static void RunFFmpegProcess(string ffmpegPath, string arguments)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // 輸出 FFmpeg 的執行結果
                Console.WriteLine(output);
            }
            catch(Exception ex) { }
        }


        private static int GetImageHeight(string imageFilePath)
        {
            using (var image = new Bitmap(imageFilePath))
            {
                return image.Height;
            }
        }


    }

}
