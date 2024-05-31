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

    public Node(double latitude, double longitude, long nodeID, NodeType type)
    {
        this.position = Geo.SphericalToCartesian(latitude, longitude);
        this.latitude = latitude;
        this.longitude = longitude;
        this.nodeID = nodeID;
        this.type = type;

        nodeStruct = new(latitude, longitude, nodeID, type);
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