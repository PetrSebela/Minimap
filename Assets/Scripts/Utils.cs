using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    public static float Angle(Vector2 a, Vector2 b, Vector2 c)
    {
        float temp = math.atan2(c.y - b.y, c.x - b.x) - math.atan2(a.y - b.y, a.x - b.x);
        return temp < 0 ? math.PI * 2 + temp : temp;
    }

    public static bool IsInFlatTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p)
    {
        float denominator = (p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z);

        float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.x) * (p.z - p3.z)) / denominator;
        float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.x) * (p.z - p3.z)) / denominator;
        float c = 1 - a - b;

        //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        // return a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f;

        return a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f;
    }

    public static bool SegmentsIntersect(Vector2 start_1, Vector2 end_1, Vector2 start_2, Vector2 end_2)
    {
        Vector2 segment_1 = end_1 - start_1;
        Vector2 segment_2 = end_2 - start_2;

        float cache = -segment_2.x * segment_1.y + segment_1.x * segment_2.y;

        float s = (-segment_1.y * (start_1.x - start_2.x) + segment_1.x * (start_1.y - start_2.y)) / cache;
        float t = (segment_2.x * (start_1.y - start_2.y) - segment_2.y * (start_1.x - start_2.x)) / cache;

        return 0 < t && t < 1 && 0 < s && s < 1;
    }
}
