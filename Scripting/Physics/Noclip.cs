﻿using System;

namespace Engine.Scripting.Physics
{
    public class Noclip : KinematicBody
    {
        public bool HandleCollisions { get; set; } = true;

        public override void Accelerate(Vector direction)
        {
            Velocity += direction * Frame.DeltaTime;
        }

        public override void AddAngularVelocity(float momentum)
        {
            throw new NotImplementedException();
        }

        public override void AddVelocity(Vector velocity)
        {
            Velocity += velocity;
        }

        public override void AngularAccelerate(float acceleration)
        {
            throw new NotImplementedException();
        }

        void OnFrame()
        {
            Entity.WorldPosition += Velocity * Frame.DeltaTime;
        }
    }
}
