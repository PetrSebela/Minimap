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
    private long id;
    public Color representedColor;
    public int levels = 1;
    const float storyHeight = 3;

    private GameObject gameObject;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    public Vector3 buildingCenter;

    private static readonly int[] quarOrder = { 0, 2, 1, 1, 2, 3 };
    private static readonly int[] triangleOrder = { 0, 2, 1 };
   
    private BuildingStruct buildingStruct;

    public Building(long id, Material defaultMaterial, Transform parent, List<Node> perimeter, int levels, Vector3 origin)
    {
        this.id = id;

        this.representedColor = new(
            (float)Random.Range(0, 255) / 255,
            (float)Random.Range(0, 255) / 255,
            (float)Random.Range(0, 255) / 255
        );

        gameObject = new(id.ToString());
        gameObject.transform.SetParent(parent);
        meshFilter = gameObject.AddComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = defaultMaterial;

        this.perimeter = perimeter;
        this.levels = levels;
        this.buildingCenter = GetBuildingCenter(origin);
        this.gameObject.transform.position = this.buildingCenter;


        List<NodeStruct> localNodes = new();
        foreach (Node node in perimeter)
        {
            NodeStruct nodeStruct = node.GetStruct();
            localNodes.Add(nodeStruct);
        }

        buildingStruct = new(id, localNodes, levels);
        UpdateMesh(origin);
    }
    // Update is called once per frame
    public void DrawGizmo(Vector3 origin, Color color, float size = 0.1f)
    {
        Gizmos.color = color;
        foreach (Node node in this.perimeter)
            node.DrawGizmo(origin, representedColor, size);
    }

    public BuildingStruct GetStruct()
    {
        return this.buildingStruct;
    }

    public Vector3 GetBuildingCenter(Vector3 origin)
    {
        Vector3 center = Vector3.zero;
        foreach (Node node in perimeter)
        {
            Vector3 point = node.GetPointWithOffset(origin);
            center += point;
        }
        return center / perimeter.Count;
    }

    public void NormalizePerimeterDirection()
    {
        float sum = 0;
        for (int index = 0; index < perimeter.Count - 1; index++)
        {
            Vector3 a = perimeter[index].GetPointWithOffset(Vector3.zero);
            Vector3 b = perimeter[index + 1].GetPointWithOffset(Vector3.zero);
            sum += (b.x - a.x) * (b.z + a.z);
        }
        if (sum > 0)
            perimeter.Reverse();
    }


    public void UpdateMesh(Vector3 origin)
    {
        // normalize direction of points     
        if (perimeter.Count < 2)
            return;

        NormalizePerimeterDirection();

        float realHeight = levels * storyHeight;

        List<Vector3> closedPerimeter = new();
        List<Vector3> roofPolygon = new();

        // getting mesh vertices
        foreach (Node node in perimeter)
        {
            Vector3 point = node.GetPointWithOffset(origin) - this.buildingCenter;
            Vector3 roofPoint = point + realHeight * Vector3.up;
            closedPerimeter.Add(point);
            roofPolygon.Add(roofPoint);
        }

        Vector3 startPoint = perimeter[0].GetPointWithOffset(origin) - this.buildingCenter;
        closedPerimeter.Add(startPoint);
        roofPolygon.Add(startPoint + realHeight * Vector3.up);

        // constructing mesh
        mesh.Clear();
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

            foreach (int indexOffset in quarOrder)
                indices.Add(vertexCount + indexOffset);

            vertexCount += 4;
        }

        // roof polygon triangulation
        int i = -1;
        int limit = 100; // in case something goes terribly wrong
        while (roofPolygon.Count > 2 && limit > 0)
        {
            i++;
            limit--;

            Vector3 a = roofPolygon[i % roofPolygon.Count];
            Vector3 b = roofPolygon[(i + 1) % roofPolygon.Count];
            Vector3 c = roofPolygon[(i + 2) % roofPolygon.Count];

            // triangle orientation
            float determinant = a.x * b.z + c.x * a.z + b.x * c.z - a.x * c.z - c.x * b.z - b.x * a.z;
            if (determinant < 0f)
                continue;

            // if triangle is inside roof polygon
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

            roofPolygon.RemoveAt((i + 1) % roofPolygon.Count); // erase ear vertex

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


public struct BuildingStruct
{
    public long buildingID;
    public List<NodeStruct> perimeter;
    public int levels;

    public BuildingStruct(long buildingID, List<NodeStruct> perimeter, int levels)
    {
        this.buildingID = buildingID;
        this.perimeter = perimeter;
        this.levels = levels;
    }
}