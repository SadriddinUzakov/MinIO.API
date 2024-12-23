namespace minio.Domain.Core.Utilities
{
    public class ImageSize
    {
        public SizeUnit Unit { get; set; }
        public int? Size { get; set; }

        public ImageSize(SizeUnit unit, int? size)
        {
            Unit = unit;
            Size = size;
        }

        public static ImageSize GetWidthSize(List<ImageSize> sizes)
        {
            return sizes.Find(size => size.Unit == SizeUnit.WIDTH);
        }

        public static ImageSize GetHeightSize(List<ImageSize> sizes)
        {
            return sizes.Find(size => size.Unit == SizeUnit.HEIGHT);
        }
    }
}

