using Application.Services;
using Org.BouncyCastle.Utilities.Zlib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Drawing;
using Xunit.Abstractions;

namespace Tests_Application
{
    public class ResizerService_Tests(ITestOutputHelper _printer)
    {
        [Fact]
        public void ResizeImage_HugeFile_ReturnsSmall()
        {
            PhotoResizerService resizer = new PhotoResizerService();

            using Image<Rgba32> largeImage = new Image<Rgba32>(30000,30000);
            using MemoryStream inputstream = new MemoryStream();
            using MemoryStream outputstream = new MemoryStream();
            largeImage.Save(inputstream, new JpegEncoder() { Quality = 100 });
            double origWeight = inputstream.Length / 1024;
            inputstream.Position= 0;

            Stopwatch stopWatch = Stopwatch.StartNew();
            long memoryBefore = GC.GetTotalMemory(true);

            resizer.ResizeImage(inputstream, outputstream);
            stopWatch.Stop();
            long memoryafter= GC.GetTotalMemory(false);
            double elapsedMilSec=stopWatch.Elapsed.TotalMilliseconds;
            double usedMb = (memoryafter - memoryBefore) / 1024d / 1024d;

            Assert.True(outputstream.Length <= 1000000);

            _printer.WriteLine($"Time: {elapsedMilSec:N2}s");
            _printer.WriteLine($"Memory diff: {usedMb:N2} MB");
            _printer.WriteLine($"Original size: {origWeight} KB");
            _printer.WriteLine($"Output size: {outputstream.Length / 1024} KB");
            
        }

        [Fact]
        public void ResizeImadeOld_HugeFile_ReturnsSmall()
        {
            var resizer = new PhotoResizerService();

            using Image<Rgba32> largeImage = new Image<Rgba32>(30000, 30000);
            using MemoryStream inputstream = new MemoryStream();
            largeImage.Save(inputstream, new JpegEncoder() { Quality = 100 });
            double origWeight = inputstream.Length / 1024;
            inputstream.Position = 0;

            Stopwatch stopWatch = Stopwatch.StartNew();
            long memoryBefore = GC.GetTotalMemory(true);

            using Stream resizedStream = resizer.ResizeImage_Old(inputstream);

            stopWatch.Stop();
            long memoryafter = GC.GetTotalMemory(false);
            double elapsedMilSec = stopWatch.Elapsed.TotalMilliseconds;
            double usedMb = (memoryafter - memoryBefore) / 1024d / 1024d;

            Assert.True(resizedStream.Length <= 1000000);

            _printer.WriteLine($"Time: {elapsedMilSec:N2}s");
            _printer.WriteLine($"Memory diff: {usedMb:N2} MB");
            _printer.WriteLine($"Original size: {origWeight} KB");
            _printer.WriteLine($"Output size: {resizedStream.Length / 1024} KB");

        }
    }
}
