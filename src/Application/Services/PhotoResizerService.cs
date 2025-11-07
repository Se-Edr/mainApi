using Org.BouncyCastle.Crypto.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

namespace Application.Services
{
    public class PhotoResizerService
    {
        long maxSize = 1000000;

        public void ResizeImageWithTwoStreams(Stream input, Stream output)
        {
            if (output == null || input == null)
            {
                throw new ArgumentNullException("some of streams is empty");
            }

            using var image = Image.Load(input);
            long startPos=output.Position;
            image.Save(output,new JpegEncoder { Quality=90});
            long savedSize = output.Position - startPos;
            if(savedSize < maxSize)
            {
                return;
            }


        }
       
        public  void ResizeImage(Stream origStream, Stream outputStream)
        {
            if (origStream == null || outputStream == null)
            {
                throw new ArgumentNullException("some stream  is empty");
            }

            double scaler = 1;
            
            Image currentImage= Image.Load(origStream);

            outputStream.Position = 0;
            outputStream.SetLength(0);
            
            currentImage.Save(outputStream, new JpegEncoder());


            for (int i = 0; i < 10; i++)
            {
                if(outputStream.Length < maxSize)
                {
                    break;
                }

                double div = (double)maxSize / outputStream.Length;
                scaler =Math.Sqrt(div);

                var newImage = UsingScale(scaler, currentImage);

                currentImage.Dispose();

                outputStream.SetLength(0);
                outputStream.Position = 0;

                currentImage = newImage;
                currentImage.Save(outputStream, new JpegEncoder());

            }
            outputStream.Position = 0;
        }

        public Stream ResizeImage_Old(Stream origStream)
        {
            
            double scaler = 1;

            Image? origImg = Image.Load(origStream);

            Image currentImage = origImg.Clone(x => x.Resize(origImg.Width, origImg.Height));
            long currentSize = origStream.Length;

            Stream mainStream = origStream;

            try
            {
                while (true)
                {
                    if (mainStream.Length < maxSize)
                    {
                        return mainStream;
                    }
                    //currentImage.Save(mainStream, new JpegEncoder());
                    double div = (double)maxSize / mainStream.Length;
                    scaler = Math.Sqrt(div);

                    Image newImage = UsingScale(scaler, currentImage);
                    mainStream.SetLength(0);
                    mainStream.Position = 0;
                    newImage.Save(mainStream, new JpegEncoder());
                    currentImage = currentImage.Clone(x => x.Resize(newImage.Width, newImage.Height));
                }
            }
            finally
            {
            }
        }


        public  async Task<MemoryStream> ResizeImageForMinio(MemoryStream originalStream)
        {
            long maxSize = 1000000;
            double scaler = 1;

            var origImg = Image.Load(originalStream);
            Image currentImage = origImg.Clone(x => x.Resize(origImg.Width, origImg.Height));

            MemoryStream secondaryStream = new MemoryStream();

            while (true)
            {
                secondaryStream.SetLength(0);
                currentImage.Save(secondaryStream, new JpegEncoder());
                if (secondaryStream.Length < maxSize)
                {
                    secondaryStream.Position = 0;
                    return secondaryStream;
                }
                double div = (double)maxSize / secondaryStream.Length;
                scaler = Math.Sqrt(div);
                Image newImage = UsingScale(scaler, currentImage);
                currentImage.Dispose();
                currentImage = newImage;

            }
        }


        private Image UsingScale(double scaler, Image img)
        {
            int newWidth = (int)(img.Width * scaler);
            int newHeight = (int)(img.Height * scaler);
            Image newImg = img.Clone(x => x.Resize(newWidth, newHeight));
            return newImg;
        }




        //public async Task<MemoryStream> ResizeImage(MemoryStream origStream)
        //{
        //    long maxSize = 1000000;
        //    double scale = 1;
        //    double currentLength = origStream.Length;

        //    MemoryStream mainStream = new MemoryStream(origStream.ToArray(), true);


        //    try
        //    {
        //        while (true)
        //        {
        //            if (mainStream.Length < maxSize)
        //            {
        //                return new MemoryStream(mainStream.ToArray());
        //            }

        //            double div = (double)maxSize / currentLength;
        //            scale = Math.Sqrt(div);
        //            mainStream.Position = 0;
        //            using SKBitmap origBitmap = SKBitmap.Decode(mainStream);
        //            using SKBitmap resized = new SKBitmap((int)(origBitmap.Width * scale), (int)(origBitmap.Height * scale));

        //            using (var canvas = new SKCanvas(resized))
        //            {
        //                canvas.DrawBitmap(origBitmap, new SKRect(0, 0, (int)(origBitmap.Width * scale), (int)(origBitmap.Height * scale)));
        //            }

        //            using var image = SKImage.FromBitmap(resized);
        //            using var newImg = image.Encode(SKEncodedImageFormat.Jpeg, 80);

        //            if (!mainStream.CanWrite)
        //                throw new Exception("mainStream is not writable.");

        //            mainStream.SetLength(0);
        //            mainStream.Position = 0;


        //            newImg.SaveTo(mainStream);
        //            currentLength = mainStream.Length;
        //        }
        //    }
        //    finally
        //    {
        //        // secondaryStream?.Dispose();
        //        mainStream.SetLength(0);
        //        mainStream.Position = 0;
        //    }
        //}

    }
}
