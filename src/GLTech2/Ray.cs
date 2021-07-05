﻿namespace GLTech2
{
    public unsafe struct Ray
    {
        internal Vector start;
        internal Vector direction;

        public Ray(Vector start, Vector direction)
        {
            this.start = start;
            this.direction = direction;
        }

        public Ray(Vector start, float angle)
        {
            this.start = start;
            direction = new Vector(angle);
        }
    }
}
