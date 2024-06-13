using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Node
{
    Vector3 position;
    long nodeID;
    Coordinates coordinates;
    public NodeType type;
    private Chunk parentChunk;

    private NodeStruct nodeStruct;
    MapSettings mapSettings;

    GameObject gameObject;
    
    public List<Building> usedByBuildings = new();
    public List<Road> usedByRoads = new();

    public Node(NodeStruct nodeStruct, GlobalMapData globalMapData, MapSettings mapSettings)
    {
        this.position = Geo.SphericalToCartesian(nodeStruct.latitude, nodeStruct.longitude);
        
        this.coordinates = new(nodeStruct.latitude, nodeStruct.longitude);
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

        foreach (Tag tag in mapSettings.tags)
        {
            if (type == tag.nodeType)
            {
                gameObject = GameObject.Instantiate(tag.gameObject);
                gameObject.GetComponent<Billboard>().cameraTransform = mapSettings.billboardTarget;
                gameObject.transform.position = GetPointWithOffset(mapSettings.worldOrigin);
                gameObject.transform.parent = parentChunk.gameObject.transform;
                break;
            }
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

    public Vector3 GetLocalizedPoint()
    {
        return this.position - mapSettings.worldOrigin;
    }

    public NodeStruct GetStruct()
    {
        return this.nodeStruct;
    }

    public void DrawGizmo()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetLocalizedPoint(), 1);
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