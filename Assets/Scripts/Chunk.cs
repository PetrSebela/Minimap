using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    public Vector3 position;
    public GameObject gameObject;

    public List<Node> nodes = new();
    public List<Building> buildings = new();
    public List<Road> roads = new();
    GlobalMapData globalMapData;
    MapSettings mapSettings;

    public Chunk(Vector3 position, GlobalMapData globalMapData, MapSettings mapSettings)
    {
        gameObject = new(position.ToString());
        gameObject.transform.position = position;
        gameObject.isStatic = true;
        this.position = position;

        this.globalMapData = globalMapData;
        this.mapSettings = mapSettings;
    }

    public void Serialize()
    {
        Dictionary<long, NodeStruct> usedNodes = new();
        Dictionary<long, BuildingStruct> usedBuildings = new();
        Dictionary<long, RoadStruct> usedRoads = new();

        foreach (Node node in nodes)
        {
            NodeStruct nodeStruct = node.GetStruct();
            if(!usedNodes.ContainsKey(nodeStruct.nodeID))
                usedNodes.Add(nodeStruct.nodeID, nodeStruct);
        }

        foreach (Building building in buildings)
        {
            foreach (Node node in building.perimeter)
            {
                NodeStruct poiStruct = node.GetStruct();
                if (usedNodes.ContainsKey(poiStruct.nodeID))
                    continue;
                usedNodes.Add(poiStruct.nodeID, poiStruct);
            }

            BuildingStruct bs = building.GetStruct();

            if(!usedBuildings.ContainsKey(bs.buildingID))
                usedBuildings.Add(bs.buildingID, bs);
        }


        foreach (Road road in roads)
        {
            foreach (Node node in road.nodes)
            {
                NodeStruct roadNode = node.GetStruct();
                if (usedNodes.ContainsKey(roadNode.nodeID))
                    continue;
                usedNodes.Add(roadNode.nodeID, roadNode);
            }
            RoadStruct roadStruct = road.GetStruct();

            if(!usedRoads.ContainsKey(roadStruct.roadID))
                usedRoads.Add(roadStruct.roadID, roadStruct);
        }

        ChunkStruct st = new(usedNodes, usedBuildings, usedRoads);

        string jsonString = JsonConvert.SerializeObject(st, Formatting.Indented);
        int chunkCode = position.GetHashCode();
        string fileName = string.Format("{0}.mapdata", chunkCode);
        StreamWriter outputFile = new(Path.Combine(Application.persistentDataPath, fileName));
        outputFile.WriteLine(jsonString);
        outputFile.Close();
    }

    public void DrawGizmos()
    {
        Vector3 chunkSizeVector = new(mapSettings.chunkSize, 0.1f, mapSettings.chunkSize );
        Gizmos.DrawSphere(gameObject.transform.position, 1);
        Gizmos.DrawWireCube(gameObject.transform.position, chunkSizeVector);
    }
}

public struct ChunkStruct
{
    public Dictionary<long, NodeStruct> usedNodes;
    public Dictionary<long, BuildingStruct> buildings;
    public Dictionary<long, RoadStruct> roads;

    public ChunkStruct(Dictionary<long, NodeStruct> usedNodes, Dictionary<long, BuildingStruct> buildings, Dictionary<long, RoadStruct> roads)
    {
        this.usedNodes = usedNodes;
        this.buildings = buildings;
        this.roads = roads;
    }
}