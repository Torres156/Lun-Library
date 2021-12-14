using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Lun
{
    using MessagePack;
    using SFML.System;
    [StructLayout(LayoutKind.Sequential)]
    [MessagePackObject(true)]
    public struct Point : IEquatable<Point>
    {
        public int x;
        public int y;

        public static readonly Point Zero = new Point(0);
        public static readonly Point One = new Point(1);


        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="value"></param>
        public Point(int value) : this(value, value)
        { }

        /// <summary>
        /// Verifica se o vetor é igual
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Point other)
            => other.x == x && other.y == y;

        /// <summary>
        /// Verifica se o vetor é igual
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Vector2 other)
            => (int)other.x == x && (int)other.y == y;

        /// <summary>
        /// Distância entre 2 pontos
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int Distance(Point other)
            => (int)Math.Sqrt(Math.Pow(x - other.x, 2) + Math.Pow(y - other.y, 2));

        public override int GetHashCode()
            => x.GetHashCode() + y.GetHashCode();

        public override bool Equals(object obj)
            => (obj is Point || obj is Vector2) && Equals((Point)obj);

        public Vector2 ToVector2()
            => new Vector2(x, y);

        public static Point Max(Point v1, Point v2)
            => new Point(Math.Max(v1.x, v2.x), Math.Max(v1.y, v2.y));

        public static Point Min(Point v1, Point v2)
            => new Point(Math.Min(v1.x, v2.x), Math.Min(v1.y, v2.y));

        #region Operators
        public static Point operator -(Point value, Point other)
            => new Point(value.x - other.x, value.y - other.y);

        public static Point operator -(Point value, int other)
            => new Point(value.x - other, value.y - other);

        public static Point operator +(Point value, Point other)
            => new Point(value.x + other.x, value.y + other.y);

        public static Point operator +(Point value, int other)
            => new Point(value.x + other, value.y + other);

        public static Point operator *(Point value, Point other)
            => new Point(value.x * other.x, value.y * other.y);

        public static Point operator *(Point value, int other)
            => new Point(value.x * other, value.y * other);

        public static Point operator /(Point value, Point other)
            => new Point(value.x / other.x, value.y / other.y);

        public static Point operator /(Point value, int other)
            => new Point(value.x / other, value.y / other);

        public static bool operator ==(Point value, Point other)
            => value.x == other.x && value.y == other.y;

        public static bool operator !=(Point value, Point other)
            => value.x != other.x || value.y != other.y;

        public static implicit operator Vector2f(Point v)
            => new Point(v.x, v.y);

        public static explicit operator Point(Vector2f v)
            => new Point((int)v.X, (int)v.Y);

        public static implicit operator Vector2i(Point v)
            => new Vector2i(v.x, v.y);

        public static explicit operator Point(Vector2i v)
            => new Point(v.X, v.Y);

        public static implicit operator Vector2u(Point v)
            => new Vector2u((uint)v.x, (uint)v.y);

        public static explicit operator Point(Vector2u v)
            => new Point((int)v.X, (int)v.Y);

        public static explicit operator Point(Vector2 v)
            => new Point((int)v.x, (int)v.y);
        #endregion



    }
}
