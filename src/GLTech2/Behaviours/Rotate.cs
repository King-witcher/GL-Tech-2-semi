﻿#pragma warning disable IDE0051

namespace GLTech2.Behaviours
{
    public sealed class Rotate : Behaviour
    {
        public float AngularSpeed { get; set; } = 30f;

        void OnFrame() =>
            Entity.Rotate(AngularSpeed * Frame.DeltaTime);
    }
}
