﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe readonly struct Image : IDisposable
    {
        static Image()
        {
            if (!Environment.Is64BitProcess)
                throw new Exception("GL Tech 2.1 must run as x86-64.");
        }

        public const int DefaultBytesPerPixel = 4;
        public const PixelFormat DefaultPixelFormat = PixelFormat.Format32bppArgb;

        [FieldOffset(0)] readonly int width;
        [FieldOffset(4)] readonly int height;
        [FieldOffset(8)] readonly uint* uint_buffer;
        [FieldOffset(8)] readonly Color* pixel_buffer;

        [FieldOffset(16)] readonly internal float flt_width;
        [FieldOffset(20)] readonly internal float flt_height;

        public int Height => height;
        public int Width => width;
        public IntPtr Buffer => (IntPtr)uint_buffer;
        public Color* PixelBuffer => pixel_buffer;
        public uint* UintBuffer => uint_buffer;
        public long MemorySize => DefaultBytesPerPixel * width * height;

        public Image(Bitmap source)
        {
            this = FromBitmap(source);
        }

        public Image(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("negative dimensions");

            this.flt_width = this.width = width;
            this.flt_height = this.height = height;
            this.pixel_buffer = null;
            this.uint_buffer = (uint*)Marshal.AllocHGlobal(width * height * DefaultBytesPerPixel);
        }

        public Image(int width, int height, Color color)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("negative dimensions");

            this.flt_width = this.width = width;
            this.flt_height = this.height = height;
            this.pixel_buffer = null;
            this.uint_buffer = (uint*)Marshal.AllocHGlobal(width * height * DefaultBytesPerPixel);

            FillWith(color);
        }

        private Image(int width, int height, IntPtr buffer)
        {
            this.flt_width = this.width = width;
            this.flt_height = this.height = height;
            this.pixel_buffer = default;
            this.uint_buffer = (uint*)buffer;
        }

        public static Image FromBitmap(Bitmap source)
        {
            Image image = new(source.Width, source.Height);

            // Converts the source into a standarized bits-per-pixel bitmap.
            using Bitmap src32 = source.Clone(DefaultPixelFormat) ??
                throw new ArgumentNullException("source");

            BitmapData lockdata = src32.LockBits();
            System.Buffer.MemoryCopy(
                source:                 (void*)lockdata.Scan0,
                destination:            image.uint_buffer,
                sourceBytesToCopy:      image.MemorySize,
                destinationSizeInBytes: image.MemorySize);

            src32.UnlockBits(lockdata);
            return image;
        }

        public static void BufferCopy(Image source, Image destination)
        {
            if (source.MemorySize > destination.MemorySize)
                throw new ArgumentOutOfRangeException("source");
            System.Buffer.MemoryCopy(
                source:                 source.UintBuffer,
                destination:            destination.UintBuffer,
                destinationSizeInBytes: destination.MemorySize,
                sourceBytesToCopy:      source.MemorySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Foreach(Func<Color, Color> transformation)
        {
            int height = Height;
            int width = Width;
            uint* buffer = UintBuffer;

            Parallel.For(0, width, x =>
            {
                for (int y = 0; y < height; y++)
                {
                    int cur = width * y + x;
                    buffer[cur] = transformation(buffer[cur]);
                }
            });
        }

        public void FillWith(Color color)
        {
            int height = Height;
            int width = Width;
            uint* buffer = UintBuffer;

            Parallel.For(0, width, x =>
            {
                for (int y = 0; y < height; y++)
                {
                    int cur = width * y + x;
                    buffer[cur] = color;
                }
            });
        }

        public Color this[int column, int line]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => PixelBuffer[column + Width * line];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => PixelBuffer[column + Width * line] = value;
        }

        public override string ToString()
        {
            return $"{width}x{height} {this.GetType().Name} -> {Buffer}";
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Buffer);
        }

        // Perigoso
        public static explicit operator Image(Bitmap bitmap)
        {
            var lockdata = bitmap.LockBits();
            Image result = new(bitmap.Width, bitmap.Height, lockdata.Scan0);
            bitmap.UnlockBits(lockdata);
            return result;
        }

        public static explicit operator Bitmap(Image data)
        {
            return new Bitmap(
                data.width,
                data.height,
                DefaultBytesPerPixel * data.width,
                DefaultPixelFormat,
                data.Buffer);
        }
    }
}
