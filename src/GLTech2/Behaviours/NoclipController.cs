﻿
namespace GLTech2.Behaviours
{
    /// <summary>
    /// Allows the user to move the camera around the map using keyboard input in a quake-like way. May not work as expected yet.
    /// </summary>
    public sealed class NoclipController : Behaviour
    {
        public bool AlwaysRun { get; set; } = true;
        public float MaxSpeed { get; set; } = 2f;
        public float TurnSpeed { get; set; } = 90f;
        public float Friction { get; set; } = 10f;
        public float Acceleration { get; set; } = 10f;
        public Key StepForward { get; set; } = Key.W;
        public Key StepBack { get; set; } = Key.S;
        public Key StepLeft { get; set; } = Key.A;
        public Key StepRight { get; set; } = Key.D;
        public Key TurnRight { get; set; } = Key.Right;
        public Key TurnLeft { get; set; } = Key.Left;
        public Key ChangeRun_Walk { get; set; } = Key.ShiftKey;

        // Relative to the space.
        Vector velocity;

        void Start()
        {
            velocity = Vector.Origin;
        }

        void OnFrame()
        {
            UpdateVelocity(GetMaxSpeed());
            UpdatePosition();
        }

        float GetMaxSpeed()
        {
            bool run = AlwaysRun;
            if (Keyboard.IsKeyDown(ChangeRun_Walk))
                run = !run;

            if (run)
                return MaxSpeed;
            else
                return MaxSpeed / 2;
        }

        void UpdateVelocity(float maxspeed)
        {
            ApplyFriction();
            Vector wishdir = GetWishDir();

            float currentspeed = Vector.DotProduct(velocity, wishdir);
            float addspeed = Acceleration * Frame.DeltaTime * maxspeed;
            if (addspeed < 0)
                return;
            else if (addspeed > maxspeed - currentspeed)
                addspeed = maxspeed - currentspeed;

            velocity += addspeed * wishdir;
            if (velocity.Module < .01f) velocity = (0, 0);
        }

        void UpdatePosition()
        {
            element.Translation += velocity * Frame.DeltaTime;
        }

        Vector GetWishDir()
        {
            Vector result = Vector.Origin;

            if (Keyboard.IsKeyDown(StepForward))
                result += Vector.Forward;
            if (Keyboard.IsKeyDown(StepBack))
                result += Vector.Backward;
            if (Keyboard.IsKeyDown(StepLeft))
                result += Vector.Left;
            if (Keyboard.IsKeyDown(StepRight))
                result += Vector.Right;

            result *= Element.Rotation;

            if (result.Module == 0)
                return Vector.Origin;
            else
                return result / result.Module;
        }


        void ApplyFriction()
        {
            velocity -= velocity * Frame.DeltaTime * Friction;
        }
    }
}