using System;
using UnityEngine;

public class Point
{
    public float x, y;
    public bool isPositionTaken;
    public ObjectType objectType;

    public Point() { }

    public Point(Point p)
    {
        x = p.x;
        y = p.y;
    }

    public Point(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }

    public static Point operator /(Point a, float b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException();
        }

        Point p = new Point(a);
        p.x /= b;
        p.y /= b;

        return p;
    }

    public static Point operator /(Point a, Point b)
    {
        if (b.x == 0 || b.y == 0)
        {
            throw new DivideByZeroException();
        }

        Point p = new Point(a);
        p.x /= b.x;
        p.y /= b.y;

        return p;
    }

    public static implicit operator Vector2(Point point)
    {
        Vector2 vector2 = new Vector2 { x = point.x, y = point.y };
        return vector2;
    }

    public static implicit operator Point(Vector2 vector2)
    {
        return new Point(vector2);
    }
}
