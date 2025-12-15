using System.Collections;
using System.Collections.Generic;
namespace Connect.Generator
{
    public struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsPointValid(int maxCountX,int maxCountY)
        {
            return x < maxCountX && y < maxCountY && x > -1 && y > -1;
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y);
        }

        public static Point up => new Point(0, 1);
        public static Point left => new Point(-1, 0);
        public static Point down => new Point(0, -1);
        public static Point right => new Point(1, 0);
        public static Point zero => new Point(0, 0);
        public static bool operator ==(Point p1, Point p2) => p1.x == p2.x && p1.y == p2.y;
        public static bool operator !=(Point p1, Point p2) => p1.x != p2.x || p1.y != p2.y;
        public override bool Equals(object obj)
        {
            Point a = (Point)obj;
            return x == a.x && y == a.y;
        }
        public override int GetHashCode()
        {
            return (100 * x + y).GetHashCode();
        }

    }
}