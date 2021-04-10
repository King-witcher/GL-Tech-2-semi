﻿#pragma warning disable CS0661 // O tipo define os operadores == ou !=, mas não substitui o Object.GetHashCode()
#pragma warning disable CS0659 // O tipo substitui Object. Equals (objeto o), mas não substitui o Object.GetHashCode()
#define DEVELOPMENT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GLTech2
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Vector
    {
        private readonly float x;
        private readonly float y;

        public Vector(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector(float angle)
        {
            x = (float)Math.Sin(angle * Math.PI / 180);
            y = (float)Math.Cos(angle * Math.PI / 180);
        }

        public float Angle { get => 180 * (float)Math.Asin(X / Module) / (float)Math.PI; }
        public float Module { get => (float)Math.Sqrt(X * X + Y * Y); }
        public float X { get => x; }
        public float Y { get => y; }
        public static Vector Origin { get => new Vector(0, 0); }
        public static Vector FromAngle(float angle) => new Vector(angle);
        public static Vector FromAngle(float angle, float module)
        {
            float x = (float)Math.Sin(angle * Math.PI / 180);
            float y = (float)Math.Cos(angle * Math.PI / 180);
            return new Vector(module * x, module * y);
        }
        public float GetDistance(Vector p)
        {
            float dx = p.X - X;
            float dy = p.Y - Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        public override string ToString()
        {
            return $"<{this.X}, {this.Y}>";
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Vector))
            {
                return false;
            }
            bool xeq = x.Equals(((Vector)obj).x);
            bool yeq = y.Equals(((Vector)obj).y);
            if (xeq && yeq)
                return true;
            else
                return false;
        }

        public static Vector operator +(Vector left, Vector right) => new Vector(left.X + right.X, left.Y + right.Y);
        public static Vector operator -(Vector left, Vector right) => new Vector(left.X - right.X, left.Y - right.Y);
        public static Vector operator *(Vector left, Vector right) => new Vector(left.X * right.X, left.Y * right.Y);
        public static Vector operator *(float scalar, Vector vector) => new Vector(vector.X * scalar, vector.Y * scalar);
        public static Vector operator *(Vector vector, float scalar) => scalar * vector;
        public static Vector operator /(Vector vector, float scalar) => new Vector(vector.X / scalar, vector.Y / scalar);
        public static bool operator ==(Vector left, Vector right)
        {
            bool xeq = left.x == right.x;
            bool yeq = left.y == right.y;
            return xeq && yeq;
        }
        public static bool operator !=(Vector left, Vector right)
        {
            bool xdif = left.x != right.x;
            bool ydif = left.y != right.y;
            return xdif || ydif;
        }
    }
}
