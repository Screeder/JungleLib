using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

class Utils
{
    public static bool IsValidVector3(Vector3 vector)
    {
        if (vector.X.CompareTo(0.0f) == 0 && vector.Y.CompareTo(0.0f) == 0 && vector.Z.CompareTo(0.0f) == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool IsValidFloat(float t)
    {
        if (t.CompareTo(0) != 0 && !float.IsNaN(t) && t.CompareTo(float.MaxValue) != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static Vector3 perpendicular(Vector3 v)
    {
        return new Vector3(-v.Z, v.Y, v.X);
    }

    public static Vector3 perpendicular2(Vector3 v)
    {
        return new Vector3(v.Z, v.Y, -v.X);
    }

    public static Vector3[] CircleCircleIntersection(Vector3 from, Vector3 middlePoint, float width, float distance)
    {
        float D = (float)GetDistance(from, middlePoint);
        float A = (width * width - distance * distance + D * D) / (2 * D);
        float H = (float)Math.Sqrt(width * width - A * A);
        Vector3 Direction = middlePoint - from;
        Direction.Normalize();
        Vector3 PA = from + A * Direction;
        Vector3 S1 = PA + H * Direction;
        Utils.perpendicular(S1);
        Vector3 S2 = PA - H * Direction;
        Utils.perpendicular(S2);
        return new Vector3[] { S1, S2 };
    }

    public static bool Close(float a, float b, float eps)
    {
        if (Utils.IsValidFloat(eps))
            eps = eps;
        else
            eps = (float)1e-9;
        return Math.Abs(a - b) <= eps;
    }

    public static double RadianToDegree(double angle)
    {
        return angle * (180.0 / Math.PI);
    }

    public static float Polar(Vector3 v1)
    {
        if (Close(v1.X, 0, 0))
        {
            float area1 = v1.Y;
            if (area1 > 0)
            {
                return 90;
            }
            else if (area1 < 0)
            {
                return 270;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            float area1 = v1.Y;
            var theta = (float)RadianToDegree(Math.Atan((area1) / v1.X));
            if (v1.X < 0)
            {
                theta = theta + 180;
            }
            if (theta < 0)
            {
                theta = theta + 360;
            }
            return theta;
        }
    }

    public static float AngleBetween(Vector3 self, Vector3 v1, Vector3 v2)
    {
        Vector3 p1 = (-self + v1);
        Vector3 p2 = (-self + v2);
        float theta = Polar(p1) - Polar(p2);
        if (theta < 0)
            theta = theta + 360;
        if (theta > 180)
            theta = 360 - theta;
        return theta;
    }

    public static double GetDistanceSqr(Vector3 distance1, Vector3 distance2)
    {
        float area1 = distance1.Y;
        float area2 = distance2.Y;
        double distance = Math.Pow((distance1.X - distance2.X), 2) +
                            Math.Pow(((area1) - (area2)), 2);
        return distance;
    }

    public static double GetDistance(Vector3 distance1, Vector3 distance2)
    {
        double distance = GetDistanceSqr(distance1, distance2);
        distance = Math.Sqrt(distance);
        return distance;
    }
}
