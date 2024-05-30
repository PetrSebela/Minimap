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

    // GameObject gameObject;

    public Node(double latitude, double longitude, long nodeID, NodeType type)
    {
        this.position = Geo.SphericalToCartesian(latitude, longitude);
        this.latitude = latitude;
        this.longitude = longitude;
        this.nodeID = nodeID;
        this.type = type;
    }

    public Vector3 GetPointWithOffset(Vector3 origin)
    {
        return (this.position - origin);
    }

    public void DrawGizmo(Vector3 origin, Color color, float size = 0.1f)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(this.position - origin, size);
    }
}

public enum NodeType
{
    Generic,
    Lamp,
    Tree
}