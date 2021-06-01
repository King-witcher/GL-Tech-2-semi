﻿using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace GLTech2
{
    [StructLayout(LayoutKind.Explicit)]
    public struct RGB
    {
        //Union
        [FieldOffset(0)]
        public uint rgb;
        [FieldOffset(0)]
        public byte b;
        [FieldOffset(1)]
        public byte g;
        [FieldOffset(2)]
        public byte r;
        [FieldOffset(3)]
        private byte a;

        public float Luminosity => (r + g + b) / (255f * 3f);
        public byte Luminosity256 => (byte)((r + g + b) / 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RGB Average(RGB rgb)
        {
            rgb.r = (byte)((r + rgb.r) >> 1);
            rgb.g = (byte)((g + rgb.g) >> 1);
            rgb.b = (byte)((b + rgb.b) >> 1);

            return rgb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RGB Mix(RGB rgb, float factor)
        {
            ushort parcel1, parcel2;

            parcel1 = (ushort)(r * (1 - factor));
            parcel2 = (ushort)(rgb.r * factor);
            rgb.r = (byte)(parcel1 + parcel2);

            parcel1 = (ushort)(g * (1 - factor));
            parcel2 = (ushort)(rgb.g * factor);
            rgb.g = (byte)(parcel1 + parcel2);

            parcel1 = (ushort)(b * (1 - factor));
            parcel2 = (ushort)(rgb.b * factor);
            rgb.b = (byte)(parcel1 + parcel2);

            return rgb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGB operator *(RGB rgb, float factor)
        {
            ulong red = (ulong)(rgb.r * factor);
            if (red > 255)
                red = 255;
            ulong green = (ulong)(rgb.g * factor);
            if (green > 255)
                green = 255;
            ulong blue = (ulong)(rgb.b * factor);
            if (blue > 255)
                blue = 255;

            rgb.r = (byte) (red);
            rgb.g = (byte) (green);
            rgb.b = (byte) (blue);

            return rgb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGB operator /(RGB color, float divisor)
        {
            return color * (1 / divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(RGB rgb) => rgb.rgb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator RGB(uint rgb) => new RGB{rgb = rgb};
    }
}