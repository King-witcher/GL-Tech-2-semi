﻿using System;
using Engine.Structs;

namespace Engine.World
{
    public unsafe sealed class Camera : Entity, IDisposable
    {
        #region What should happen to the unmanaged data if its position/direction changes? Here's where the class answers it.
        internal CameraStruct* unmanaged;

        private protected override Vector PositionData
        {
            get => unmanaged->position;
            set => unmanaged->position = value;
        }

        private protected override Vector DirectionData
        {
            get => new Vector(unmanaged->rotation);
            set => unmanaged->rotation = value.Angle;
        }
        #endregion

        internal Camera()
        {
            unmanaged = CameraStruct.Create(Vector.Zero, 0f);
        }

        public override void Dispose()
        {
            CameraStruct.Delete(unmanaged);
            unmanaged = null;
        }
    }
}
