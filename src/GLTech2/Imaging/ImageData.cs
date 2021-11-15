﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GLTech2.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe readonly struct ImageData : IDisposable
    {
        static ImageData()
        {
            if (!Environment.Is64BitProcess)
                throw new Exception("GL Tech 2.1 must run as x86-64.");
        }

        public const int DEFAULT_BPP = 4;
        public const PixelFormat DEFAULT_PIXEL_FORMAT = PixelFormat.Format32bppArgb;

        [FieldOffset(0)] readonly int width;
        [FieldOffset(4)] readonly int height;
        [FieldOffset(8)] readonly uint* uint_buffer;
        [FieldOffset(8)] readonly Pixel* pixel_buffer;

        [FieldOffset(16)] readonly internal float flt_width;
        [FieldOffset(20)] readonly internal float flt_height;

        public int Height => height;
        public int Width => width;
        public IntPtr Buffer => (IntPtr)uint_buffer;
        public Pixel* PixelBuffer => pixel_buffer;
        public uint* UintBuffer => uint_buffer;
        public long Size => DEFAULT_BPP * width * height;

        public ImageData(Bitmap source)
        {
            this = Clone(source);
        }

        public ImageData(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("negative dimensions");

            this.flt_width = this.width = width;
            this.flt_height = this.height = height;
            this.pixel_buffer = null;
            this.uint_buffer = (uint*)Marshal.AllocHGlobal(width * height * DEFAULT_BPP);
        }

        public ImageData(int width, int height, Pixel color)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("negative dimensions");

            this.flt_width = this.width = width;
            this.flt_height = this.height = height;
            this.pixel_buffer = null;
            this.uint_buffer = (uint*)Marshal.AllocHGlobal(width * height * DEFAULT_BPP);

            FillWith(color);
        }

        private ImageData(int width, int height, IntPtr buffer)
        {
            this.flt_width = this.width = width;
            this.flt_height = this.height = height;
            this.pixel_buffer = default;
            this.uint_buffer = (uint*)buffer;
        }

        public static ImageData Clone(Bitmap source)
        {
            ImageData clone = new(source.Width, source.Height);

            using var src32 = source.PixelFormat == DEFAULT_PIXEL_FORMAT ?
                source: source.Clone(DEFAULT_PIXEL_FORMAT) ??
                throw new ArgumentNullException("source");

            BitmapData lockdata = src32.LockBits();
            System.Buffer.MemoryCopy(
                source:                 (void*)lockdata.Scan0,
                destination:            clone.uint_buffer,
                sourceBytesToCopy:      clone.Size,
                destinationSizeInBytes: clone.Size);

            src32.UnlockBits(lockdata);
            return clone;
        }

        public static void BufferCopy(ImageData source, ImageData destination)
        {
            if (source.Size > destination.Size)
                throw new ArgumentOutOfRangeException("source");
            System.Buffer.MemoryCopy(
                source:                 source.UintBuffer,
                destination:            destination.UintBuffer,
                destinationSizeInBytes: destination.Size,
                sourceBytesToCopy:      source.Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Foreach(Func<Pixel, Pixel> transformation)
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

        public void FillWith(Pixel color)
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

        public Pixel this[int column, int line]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => PixelBuffer[column + Width * line];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => PixelBuffer[column + Width * line] = value;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Buffer);
        }

        // Perigoso
        public static explicit operator ImageData(Bitmap bitmap)
        {
            var lockdata = bitmap.LockBits();
            ImageData result = new(bitmap.Width, bitmap.Height, lockdata.Scan0);
            bitmap.UnlockBits(lockdata);
            return result;
        }

        public static explicit operator Bitmap(ImageData data)
        {
            return new Bitmap(
                data.width,
                data.height,
                DEFAULT_BPP * data.width,
                DEFAULT_PIXEL_FORMAT,
                data.Buffer);
        }
    }
}
