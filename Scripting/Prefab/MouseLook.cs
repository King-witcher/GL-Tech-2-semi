﻿using Engine.Input;

namespace Engine.Scripting.Prefab
{
    public sealed class MouseLook : Script
    {
        public bool Enabled { get; set; } = true;

        public float Sensitivity { get; set; } = 2.2f;

        public MouseLook() { }

        public MouseLook(float sensitivity)
        {
            Sensitivity = sensitivity;
        }

        void OnFrame()
        {
            if (Enabled)
            {
                Entity.Rotate(Mouse.Shift.X * 0.022f * Sensitivity);
            }
        }
    }
}
