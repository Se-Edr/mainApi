using Application.Services;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace Benchmarks_Application
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ResizerBenchmarks>();
        }
    }


    [MemoryDiagnoser] 
    [ThreadingDiagnoser] 
    [RPlotExporter]
    public class ResizerBenchmarks
    {
        private readonly PhotoResizerService _service = new();
        private MemoryStream _inputStream;
        private MemoryStream _outputStream;

        [GlobalSetup]
        public void Setup()
        {
            Image<Rgba32> largeImage = new Image<Rgba32>(30000, 30000);
            _inputStream = new MemoryStream();
            largeImage.Save(_inputStream, new JpegEncoder { Quality = 100 });
            _inputStream.Position = 0;
            _outputStream = new MemoryStream();
        }

        [Benchmark]
        public void ResizeImage_Benchmark()
        {
            _inputStream.Position = 0;
            _outputStream.SetLength(0);
            _outputStream.Position = 0;
            _service.ResizeImage(_inputStream, _outputStream);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _inputStream.Dispose();
            _outputStream.Dispose();
        }

    }
}
