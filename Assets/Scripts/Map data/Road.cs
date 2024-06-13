using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Road
{
    public List<Node> nodes = new();
    string name;
    long id;

    MapSettings mapSettings;
    GlobalMapData globalMapData;
    RoadStruct roadStruct;
    Chunk parentChunk;
    public RoadType type;

    GameObject gameObject;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh mesh;
    Vector3 center;

    private static readonly int[] quarOrder = { 0, 1, 2, 2, 1, 3 };

    public Road(RoadStruct roadStruct, GlobalMapData globalMapData, MapSettings mapSettings)
    {

        this.id = roadStruct.roadID;
        this.mapSettings = mapSettings;
        this.globalMapData = globalMapData;

        foreach (NodeStruct nodeStruct in roadStruct.nodes)
        {
            Node node = globalMapData.nodes[nodeStruct.nodeID];
            node.usedByRoads.Add(this);
            nodes.Add(node);
        }

        this.roadStruct = roadStruct;
        this.type = roadStruct.roadType;

        center = GetRoadCenter();
        Vector3 chunkKey = new(
            Mathf.Round(center.x / mapSettings.chunkSize) * mapSettings.chunkSize,
            0,
            Mathf.Round(center.z / mapSettings.chunkSize) * mapSettings.chunkSize
        );

        if (globalMapData.chunkDictionary.ContainsKey(chunkKey))
            parentChunk = globalMapData.chunkDictionary[chunkKey];
        else
        {
            parentChunk = new(chunkKey, globalMapData, mapSettings);
            globalMapData.chunkDictionary.Add(chunkKey, parentChunk);
        }
        parentChunk.roads.Add(this);

        gameObject = new(roadStruct.roadID.ToString());
        gameObject.transform.parent = parentChunk.gameObject.transform;
        gameObject.transform.position = center;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mapSettings.buildingMaterial;
    }

    public RoadStruct GetStruct()
    {
        return roadStruct;
    }

    public void UpdateMesh()
    {
        if (nodes.Count < 2)
            return;

        List<Vector3> vertices = new();
        List<int> triangles = new();

        RoadPoint[] points = MeshBuilder.GetRoadPoints(nodes.ToArray());
        Vector3[] normals = MeshBuilder.GetLineVertexNormals(points);

        for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
        {
            Vector3 point = points[pointIndex].position - center;

            Vector3 a = point - normals[pointIndex] * roadStruct.lanes / 2 * 4;
            Vector3 b = point + normals[pointIndex] * roadStruct.lanes / 2 * 4;

            vertices.Add(a);
            vertices.Add(b);
        }

        // temporarly move road up
        // mesh is too close to the ground and renders improperly
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] += Vector3.up * 0.1f;

        for (int i = 0; i < points.Length - 1; i++)
        {
            if(!points[i].isLocal || !points[i + 1].isLocal)
                continue;

            foreach (int vertexOffset in quarOrder)
                triangles.Add(i * 2 + vertexOffset);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    public Vector3 GetRoadCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (Node node in nodes)
        {
            Vector3 point = node.GetPointWithOffset(mapSettings.worldOrigin);
            center += point;
        }
        return center / nodes.Count;
    }

    public Chunk GetParentChunk()
    {
        return parentChunk;
    }

    public void DrawGizmo()
    {

        Gizmos.color = Color.gray;

        if (type != RoadType.road)
            Gizmos.color = Color.green;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector3 a = nodes[i].GetPointWithOffset(mapSettings.worldOrigin);
            Vector3 b = nodes[i + 1].GetPointWithOffset(mapSettings.worldOrigin);
            Gizmos.DrawLine(a, b);
        }
    }
}


public struct RoadStruct
{
    public long roadID;
    public List<NodeStruct> nodes;
    public float lanes;
    public RoadType roadType;

    public RoadStruct(long roadID, List<NodeStruct> nodes, float lanes, RoadType roadType)
    {
        this.roadID = roadID;
        this.nodes = nodes;
        this.lanes = lanes;
        this.roadType = roadType;
    }
}