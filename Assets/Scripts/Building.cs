// using System.Collections;
// using System;
using System.Collections.Generic;

// using Unity.Mathematics;
using Unity.VisualScripting;

// using Unity.Mathematics;
using UnityEngine;

public class Building
{
    public List<Node> perimeter = new();
    private long ID;
    public Color representedColor;
    public int levels = 1;
    const float storyHeight = 3;

    private GameObject gameObject;
    private Mesh mesh;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    public Vector3 center;

    public bool clockwise = false;

    private static readonly int[] quarOrder = { 0, 1, 2, 1, 3, 2 };
    private static readonly int[] quarOrderReversed = { 1, 0, 2, 3, 1, 2 };
    private static readonly int[] triangleOrder = { 0, 2, 1 };
    private static readonly int[] triangleOrderReversed = { 0, 1, 2 };


    // Start is called before the first frame update
    public Building(long ID, Material defaultMaterial)
    {
        this.ID = ID;

        this.representedColor = new(
            (float)Random.Range(0, 255) / 255,
            (float)Random.Range(0, 255) / 255,
            (float)Random.Range(0, 255) / 255
        );

        gameObject = new();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = defaultMaterial;
    }
    // Update is called once per frame
    public void DrawGizmo(Vector3 origin, Color color, float size = 0.1f)
    {
        Gizmos.color = color;
        foreach (Node node in this.perimeter)
            node.DrawGizmo(origin, representedColor, size);
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

    public void UpdateMesh(Vector3 origin)
    {
        // normalize direction of points
        float sum = 0;
        for (int index = 0; index < perimeter.Count - 1; index++)
        {
            Vector3 a = perimeter[index].GetPointWithOffset(origin);
            Vector3 b = perimeter[index + 1].GetPointWithOffset(origin);
            sum += (b.x - a.x) * (b.z + a.z);
        }
        if (sum > 0)
            perimeter.Reverse();


        if (perimeter.Count < 2)
            return;

        float realHeight = levels * storyHeight;

        // closing polygon and computing its center
        Vector3 center = Vector3.zero;
        List<Vector3> closedPerimeter = new();
        List<Vector3> roofPolygon = new();

        foreach (Node node in perimeter)
        {
            Vector3 point = node.GetPointWithOffset(origin);
            center += point;
            closedPerimeter.Add(point);
            roofPolygon.Add(point + realHeight * Vector3.up);
        }
        closedPerimeter.Add(perimeter[0].GetPointWithOffset(origin));
        roofPolygon.Add(perimeter[0].GetPointWithOffset(origin) + realHeight * Vector3.up);

        center /= perimeter.Count;
        this.center = center;


        // finding direction of polygon






        // constructing mesh
        List<Vector3> vertices = new();
        List<int> indices = new();

        int vertexCount = 0;

        for (int index = 0; index < closedPerimeter.Count - 1; index++)
        {
            Vector3 a = closedPerimeter[index];
            Vector3 au = closedPerimeter[index] + realHeight * Vector3.up;
            Vector3 b = closedPerimeter[index + 1];
            Vector3 bu = closedPerimeter[index + 1] + realHeight * Vector3.up;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(au);
            vertices.Add(bu);

            int[] triangles = clockwise ? quarOrder : quarOrderReversed;
            foreach (int indexOffset in triangles)
                indices.Add(vertexCount + indexOffset);

            vertexCount += 4;
        }



        // polygon triangulation
        int i = -1;
        int limit = 100; // in case something goes terribly wrong
        while (roofPolygon.Count > 2 && limit > 0)
        {
            i++;
            limit--;
            Debug.Log(roofPolygon.Count);

            Vector3 a = roofPolygon[i % roofPolygon.Count];
            Vector3 b = roofPolygon[(i + 1) % roofPolygon.Count];
            Vector3 c = roofPolygon[(i + 2) % roofPolygon.Count];


            float determinant = a.x * b.z + c.x * a.z + b.x * c.z - a.x * c.z - c.x * b.z - b.x * a.z;
            if (determinant < 0f)
                continue;

            bool skip = false;
            for (int j = 0; j < roofPolygon.Count; j++)
            {
                if (Utils.IsInFlatTriangle(a, b, c, roofPolygon[j]))
                {
                    skip = true;
                    break;
                }
            }
            if (skip)
                continue;

            roofPolygon.RemoveAt((i + 1) % roofPolygon.Count); // erase middle point
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            foreach (int indexOffset in triangleOrder)
                indices.Add(vertexCount + indexOffset);
            vertexCount += 3;
        }



        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
    }
}
