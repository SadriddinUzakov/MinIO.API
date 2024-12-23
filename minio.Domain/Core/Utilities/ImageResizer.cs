using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace minio.Domain.Core.Utilities
{
    public  class ImageResizer
    {
        private static readonly string[] SupportedFormats = { "PNG", "JPEG", "JPG", "GIF", "BMP", "TIFF" };
        private readonly ILogger<ImageResizer> _logger;

        public void Resize0(string inputImagePath, string outputImagePath, int scaledWidth, int scaledHeight)
        {
            try
            {
                using var inputImage = ReadImage(inputImagePath);
                if (inputImage == null) return;

                using var outputImage = new Bitmap(scaledWidth, scaledHeight);
                using var graphics = Graphics.FromImage(outputImage);
                graphics.DrawImage(inputImage, 0, 0, scaledWidth, scaledHeight);

                var formatName = Path.GetExtension(outputImagePath).TrimStart('.').ToUpper();
                if (!SupportedFormats.Contains(formatName))
                {
                    throw new InvalidOperationException("Image format not supported");
                }

                outputImage.Save(outputImagePath, GetImageFormat(formatName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public void Resize1(string inputImagePath, string outputImagePath, double percent)
        {
            using var inputImage = ReadImage(inputImagePath);
            if (inputImage == null) return;

            var scaledWidth = (int)(inputImage.Width * percent);
            var scaledHeight = (int)(inputImage.Height * percent);
            Resize1(inputImagePath, outputImagePath, percent);
        }

        public void Resize2(string inputImagePath, string outputImagePath, double percent)
        {
            Resize2(inputImagePath.ToString(), outputImagePath.ToString(), percent);
        }

        public static void Resize3(string inputImagePath, string outputImagePath, int scaledWidth, int scaledHeight)
        {
            Resize3(inputImagePath.ToString(), outputImagePath.ToString(), scaledWidth, scaledHeight);
        }

        public void Resize4(string inputImagePath, string outputImagePath, int percent)
        {
            using var inputImage = ReadImage(inputImagePath.ToString());
            if (inputImage == null) return;

            var scaledWidth = inputImage.Width * percent / 100;
            var scaledHeight = inputImage.Height * percent / 100;
            Resize4(inputImagePath, outputImagePath, percent);
        }

        public static void ResizeImage(Image inputImage, string outputImagePath, int scaledWidth, int scaledHeight)
        {
            using var outputImage = new Bitmap(scaledWidth, scaledHeight);
            using var graphics = Graphics.FromImage(outputImage);
            graphics.DrawImage(inputImage, 0, 0, scaledWidth, scaledHeight);

            var formatName = Path.GetExtension(outputImagePath.ToString()).TrimStart('.').ToUpper();
            if (!SupportedFormats.Contains(formatName))
            {
                throw new InvalidOperationException("Image format not supported");
            }

            outputImage.Save(outputImagePath.ToString(), GetImageFormat(formatName));
        }

        public static void ResizeImage(Image inputImage, string outputImagePath, int percent)
        {
            var scaledWidth = inputImage.Width * percent / 100;
            var scaledHeight = inputImage.Height * percent / 100;
            ResizeImage(inputImage, outputImagePath, scaledWidth, scaledHeight);
        }

        public void ResizeWidthProportion(string inputImagePath, string outputImagePath, int width)
        {
            using var inputImage = ReadImage(inputImagePath.ToString());
            if (inputImage == null) return;

            var percent = width * 100 / inputImage.Width;
            ResizeImage(inputImage, outputImagePath, percent);
        }

        public void ResizeHeightProportion(string inputImagePath, string outputImagePath, int height)
        {
            using var inputImage = ReadImage(inputImagePath.ToString());
            if (inputImage == null) return;

            var percent = height * 100 / inputImage.Height;
            ResizeImage(inputImage, outputImagePath, percent);
        }

        public void ResizeWidthHeightProportion(string inputImagePath, string outputImagePath, int width, int height)
        {
            using var inputImage = ReadImage(inputImagePath.ToString());
            if (inputImage == null) return;

            var percent = inputImage.Height > inputImage.Width ?
                height * 100 / inputImage.Height :
                width * 100 / inputImage.Width;
            ResizeImage(inputImage, outputImagePath, percent);
        }

        public static string GetBaseName(string fileName)
        {
            var index = fileName.LastIndexOf('.');
            return index == -1 ? fileName : fileName.Substring(0, index);
        }

        public static string GetExtension(string fileName)
        {
            var index = fileName.LastIndexOf('.');
            return index == -1 ? string.Empty : fileName.Substring(index + 1);
        }

        private  Image ReadImage(string inputImagePath)
        {
            try
            {
                return Image.FromFile(inputImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading image: {0}", inputImagePath);
                return null;
            }
        }

        private static ImageFormat GetImageFormat(string formatName)
        {
            return formatName switch
            {
                "PNG" => ImageFormat.Png,
                "JPEG" => ImageFormat.Jpeg,
                "JPG" => ImageFormat.Jpeg,
                "GIF" => ImageFormat.Gif,
                "BMP" => ImageFormat.Bmp,
                "TIFF" => ImageFormat.Tiff,
                _ => throw new InvalidOperationException("Image format not supported")
            };
        }
    }
}


