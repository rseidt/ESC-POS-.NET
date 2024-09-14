using SkiaSharp;
using System;

namespace SixLabors.ImageSharp
{
    public static class SkiaSharpExtensions
    {
        public static readonly int[,] Matrix16X16 =
        {
        { 5, 191, 48, 239, 12, 203, 60, 251, 3, 194, 51, 242, 15, 206, 63, 250 },
        { 127, 64, 175, 112, 139, 76, 187, 124, 130, 67, 178, 115, 142, 79, 190, 127 },
        { 32, 223, 16, 207, 44, 235, 28, 219, 35, 226, 19, 210, 47, 238, 31, 222 },
        { 159, 96, 143, 80, 171, 108, 155, 92, 162, 99, 146, 83, 174, 111, 158, 95 },
        { 8, 199, 56, 247, 4, 195, 52, 243, 11, 202, 59, 250, 7, 198, 55, 246 },
        { 135, 72, 183, 120, 131, 68, 179, 116, 138, 75, 186, 123, 134, 71, 182, 119 },
        { 40, 231, 24, 215, 36, 227, 20, 211, 43, 234, 27, 218, 39, 230, 23, 214 },
        { 167, 104, 151, 88, 163, 100, 147, 84, 170, 107, 154, 91, 166, 103, 150, 87 },
        { 2, 193, 50, 241, 14, 205, 62, 253, 1, 192, 49, 240, 13, 204, 61, 252 },
        { 129, 66, 177, 114, 141, 78, 189, 126, 128, 65, 176, 113, 140, 77, 188, 125 },
        { 34, 225, 18, 209, 46, 237, 30, 221, 33, 224, 17, 208, 45, 236, 29, 220 },
        { 161, 98, 145, 82, 173, 110, 157, 94, 160, 97, 144, 81, 172, 109, 156, 93 },
        { 10, 201, 58, 249, 6, 197, 54, 245, 9, 200, 57, 248, 5, 196, 53, 244 },
        { 137, 74, 185, 122, 133, 70, 181, 118, 136, 73, 184, 121, 132, 69, 180, 117 },
        { 42, 233, 26, 217, 38, 229, 22, 213, 41, 232, 25, 216, 37, 228, 21, 212 },
        { 169, 106, 153, 90, 165, 102, 149, 86, 168, 105, 152, 89, 164, 101, 148, 85 }
        };

        /// <summary>
        /// Converts the bitmap image to a bi-tonal (black/white) image using a simple threshold or
        /// dither based on the Bayer 16x16 matrix. The resulting buffer returned is a bi-tonal image
        /// buffer with the same width and height of the original image.
        /// </summary>
        private static unsafe byte[] BitonalFromBitmap(SKBitmap bitmap, bool dither = false)
        {
            // compute stride, allocate workspace
            var stride = (bitmap.Width + 7) / 8;
            var buffer = new byte[stride * bitmap.Height];

            // get pointer to image pixels
            var src = (byte*)bitmap.GetPixels().ToPointer();

            // process all image rows
            for (var y = 0; y < bitmap.Height; y++)
            {
                var dst = y * stride;
                byte mask = 0x80;
                byte b = 0;

                // process raster line pixels
                var p = src;
                for (var x = 0; x < bitmap.Width; x++)
                {
                    // compute pixel average
                    var c = (*p + *(p + 1) + *(p + 2)) / 3;
                    p += 4;

                    // dither or threshold
                    var t = dither ? Matrix16X16[y & 0x0f, x & 0x0f] : 128;
                    if (c < t)
                        b |= mask;

                    // adjust output mask
                    if ((mask >>= 1) == 0)
                    {
                        buffer[dst++] = b;
                        mask = 0x80;
                        b = 0;
                    }
                }

                // flush remaining byte
                if (mask != 0x80)
                    buffer[dst] = b;

                // point to next row
                src += bitmap.RowBytes;
            }

            // return bi-tonal image buffer
            return buffer;
        }




        public static byte[] ToSingleBitPixelByteArray(this SKBitmap image, bool rasterFormat = true, int? maxWidth = null, int? maxHeight = null, float threshold = 0.5F)
        {
            if (maxWidth.HasValue || maxHeight.HasValue)
            {
                decimal ratio = (decimal)image.Width / (decimal)image.Height;
                var newWidth = image.Width;
                var newHeight = image.Height;
                if (maxWidth.HasValue && maxWidth < newWidth)
                {
                    newWidth = maxWidth.Value;
                    newHeight = Convert.ToInt32((decimal)newWidth / ratio);
                }
                if (maxHeight.HasValue && maxHeight < newHeight)
                {
                    newHeight = maxHeight.Value;
                    newWidth = Convert.ToInt32((decimal)newHeight * ratio);
                }
                if (newWidth != image.Width || newHeight != image.Height)
                {
                    image = image.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
                }
            }
            var bytes = BitonalFromBitmap(image,true);
            return bytes;
        }
    }
}
