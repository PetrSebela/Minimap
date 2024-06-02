using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node
{
    Vector3 position;
    long nodeID;
    double latitude;
    double longitude;
    public NodeType type;

    private NodeStruct nodeStruct;

    public Node(NodeStruct nodeStruct, GlobalMapData globalMapData, MapSettings mapSettings)
    {
        this.position = Geo.SphericalToCartesian(nodeStruct.latitude, nodeStruct.longitude);
        this.latitude = nodeStruct.latitude;
        this.longitude = nodeStruct.longitude;
        this.nodeID = nodeStruct.nodeID;
        this.type = nodeStruct.nodeType;
        this.nodeStruct = nodeStruct;


        Vector3 planePosition = GetPointWithOffset(mapSettings.worldOrigin);
        Vector3 chunkKey = new (
            Mathf.Round(planePosition.x / mapSettings.chunkSize) * mapSettings.chunkSize,
            0,
            Mathf.Round(planePosition.z / mapSettings.chunkSize) * mapSettings.chunkSize
        );

        Chunk parentChunk;
        if (globalMapData.chunkDictionary.ContainsKey(chunkKey))
            parentChunk = globalMapData.chunkDictionary[chunkKey];
        else
        {
            parentChunk = new(chunkKey, globalMapData, mapSettings);
            globalMapData.chunkDictionary.Add(chunkKey, parentChunk);
        }

        parentChunk.nodes.Add(this);
    }

    public Vector3 GetPointWithOffset(Vector3 origin)
    {
        return this.position - origin;
    }

    public Vector3 GetRawPoint()
    {
        return this.position;
    }

    public NodeStruct GetStruct()
    {
        return this.nodeStruct;
    }

    public void DrawGizmo(Vector3 origin, Color color, float size = 0.1f)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(this.position - origin, size);
    }
}



public struct NodeStruct
{
    public double latitude;
    public double longitude;
    public long nodeID;
    public NodeType nodeType;
    public NodeStruct(double latitude, double longitude, long nodeID, NodeType nodeType)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.nodeID = nodeID;
        this.nodeType = nodeType;
    }
}


[System.Serializable]
public enum NodeType
{
    Generic,
    Lamp,
    Tree
}