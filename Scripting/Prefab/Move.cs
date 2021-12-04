﻿#pragma warning disable IDE0051

namespace Engine.Scripting.Prefab
{
    public sealed class Move : Script
    {
        public Vector Direction { get; set; } = Vector.Forward;

        public float Speed { get; set; } = 0.5f;

        void OnFrame() =>
            Entity.RelativePosition += Direction * Speed * Frame.DeltaTime;
    }
}
