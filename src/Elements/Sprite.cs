﻿
using System;
using System.Runtime.InteropServices;

namespace GLTech2
{
    internal unsafe sealed class Sprite : Element
    {
        internal SpriteData* unmanaged;

        private protected override Vector AbsolutePosition
        {
            get => unmanaged->position;
            set => unmanaged->position = value;
        }

        private protected override Vector AbsoluteNormal { get; set; }

        public Sprite(Vector position, Material material)
        {
            unmanaged = SpriteData.Alloc(position, material);
            UpdateRelative();
        }

        public override void Dispose() =>
            Marshal.FreeHGlobal((IntPtr)unmanaged);
    }
}