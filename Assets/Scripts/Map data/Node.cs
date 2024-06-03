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
    private Chunk parentChunk;

    private NodeStruct nodeStruct;
    // GameObject gameObject;
    Color gizmoColor = Color.white;
    bool hasGizmo = false;
    MapSettings mapSettings;
    GameObject gameObject;

    public Node(NodeStruct nodeStruct, GlobalMapData globalMapData, MapSettings mapSettings)
    {
        this.position = Geo.SphericalToCartesian(nodeStruct.latitude, nodeStruct.longitude);
        
        this.latitude = nodeStruct.latitude;
        this.longitude = nodeStruct.longitude;
        this.nodeID = nodeStruct.nodeID;
        this.type = nodeStruct.nodeType;
        this.nodeStruct = nodeStruct;
        this.mapSettings = mapSettings;

        Vector3 planePosition = GetPointWithOffset(mapSettings.worldOrigin);
        Vector3 chunkKey = new (
            Mathf.Round(planePosition.x / mapSettings.chunkSize) * mapSettings.chunkSize,
            0,
            Mathf.Round(planePosition.z / mapSettings.chunkSize) * mapSettings.chunkSize
        );

        
        if (globalMapData.chunkDictionary.ContainsKey(chunkKey))
            parentChunk = globalMapData.chunkDictionary[chunkKey];
        else
        {
            parentChunk = new(chunkKey, globalMapData, mapSettings);
            globalMapData.chunkDictionary.Add(chunkKey, parentChunk);
        }

        if(type == NodeType.Surveillance)
        {
            gameObject = GameObject.Instantiate(mapSettings.tag);
            gameObject.GetComponent<Billboard>().cameraTransform = mapSettings.billboardTransform;
            gameObject.transform.position = GetPointWithOffset(mapSettings.worldOrigin);
            gameObject.transform.parent = parentChunk.gameObject.transform;
        }
        

        switch (type)
        {
            case NodeType.Surveillance:
                hasGizmo = true;
                gizmoColor = Color.red;
                break;
            default:
                break;
        }

        parentChunk.nodes.Add(this);
    }

    public Chunk GetParentChunk()
    {
        return parentChunk;
    }

    public Vector3 GetPointWithOffset(Vector3 origin)
    {
        return this.position - origin;
    }

    public NodeStruct GetStruct()
    {
        return this.nodeStruct;
    }

    public void DrawGizmo()
    {
        if(!hasGizmo)
            return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(position - mapSettings.worldOrigin + new Vector3(0, 35, 0), 3.65f);
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
    Tree,
    Chimney,
    CoolingTower,
    CommunicationsTower,
    Antenna,
    Surveillance
}