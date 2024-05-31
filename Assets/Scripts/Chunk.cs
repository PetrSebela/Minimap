using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    Vector3 position;
    public GameObject gameObject;

    public List<Node> nodes = new();
    public List<Building> buildings = new();
    public List<Road> roads = new();
    
    public Chunk(Vector3 position)
    {
        gameObject = new(position.ToString());
        gameObject.transform.position = position;   
        this.position = position;
    }

    public void Serialize()
    {
        Dictionary<long, NodeStruct> usedNodes = new();
        Dictionary<long, BuildingStruct> usedBuildings = new();

        foreach (Node node in nodes)
        {
            NodeStruct nodeStruct = node.GetStruct();
            usedNodes.Add(nodeStruct.nodeID, nodeStruct);
        }
        foreach (Building building in buildings)
        {
            List<NodeStruct> localNodes = new();
            foreach (Node node in building.perimeter)
            {
                NodeStruct poiStruct = node.GetStruct();
                if(usedNodes.ContainsKey(poiStruct.nodeID))
                    continue;
                usedNodes.Add(poiStruct.nodeID, poiStruct);
            }

            BuildingStruct bs = building.GetStruct();
            usedBuildings.Add(bs.buildingID, bs);
        }

        ChunkStruct st = new(usedNodes, usedBuildings);
        Debug.Log(usedBuildings.Count);
        Debug.Log(usedNodes.Count);

        string jsonString = JsonConvert.SerializeObject(st,Formatting.Indented);
        string fileName = string.Format("{0}|{1}.mapdata", position.x, position.z);
        StreamWriter outputFile = new(Path.Combine(Application.persistentDataPath, fileName));
        outputFile.WriteLine(jsonString);
        outputFile.Close();
    }


    public static void LoadAllStoredChunks(GlobalMapData globalMapData, MapSettings mapSetting)
    {
        DirectoryInfo directoryInfo = new(Application.persistentDataPath);
        FileInfo[] files = directoryInfo.GetFiles("*.mapdata");

        foreach (FileInfo fileInfo in files)
        {
            Chunk chunk = LoadChunk(fileInfo.FullName, globalMapData, mapSetting);
            if (globalMapData.chunkDictionary.ContainsKey(chunk.position))
                continue;

            globalMapData.chunkDictionary.Add(chunk.position, chunk);
        }
    }


    public static Chunk LoadChunk(string path, GlobalMapData globalMapData, MapSettings mapSetting)
    {
        string jsonString = File.ReadAllText(path);
        ChunkStruct chunkStruct = JsonConvert.DeserializeObject<ChunkStruct>(jsonString);

        string fileName = Path.GetFileNameWithoutExtension(path);
        string[] coordinateString = fileName.Split("|");
        double xCoordinate = Convert.ToDouble(coordinateString[0]);
        double yCoordinate = Convert.ToDouble(coordinateString[1]);

        Vector3 chunkKey = new((float)xCoordinate, 0, (float)yCoordinate);
        
        // chunk is already loaded
        if(globalMapData.chunkDictionary.ContainsKey(chunkKey))
            return globalMapData.chunkDictionary[chunkKey];

        Chunk chunk = new(chunkKey);

        List<Node> localNodes = new();
        foreach (NodeStruct nodeStruct in chunkStruct.usedNodes.Values)
        {
            long id = nodeStruct.nodeID;
            Node node;
            
            if(globalMapData.nodes.ContainsKey(id))
            {
                node = globalMapData.nodes[id];
                localNodes.Add(node);
            }
            else
            {
                node = new(nodeStruct.latitude, nodeStruct.longitude, id, nodeStruct.nodeType);
                globalMapData.nodes.Add(id, node);
                localNodes.Add(node);
            }
        }

        List<Building> localBuildings = new();
        foreach (BuildingStruct buildingStruct in chunkStruct.buildings.Values)
        {
            long id = buildingStruct.buildingID;
            Building building;
            
            if(globalMapData.buildings.ContainsKey(id))
            {
                building = globalMapData.buildings[id];
                localBuildings.Add(building);
            }
            else
            {
                List<Node> perimeter = new();
                
                foreach (NodeStruct perimeterNode in buildingStruct.perimeter)
                {
                    long nodeID = perimeterNode.nodeID;
                    if(!globalMapData.nodes.ContainsKey(nodeID))
                        continue;
                    Node node = globalMapData.nodes[nodeID];
                    perimeter.Add(node);
                }

                building = new(id, mapSetting.buildingMaterial, chunk.gameObject.transform, perimeter, buildingStruct.levels, mapSetting.worldOrigin);
                globalMapData.buildings.Add(id, building);
                localBuildings.Add(building);
            }
        }
        
        return chunk;        
    } 

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(gameObject.transform.position, new Vector3(100, 1, 100));
    }
}

public struct ChunkStruct
{
    public Dictionary<long, NodeStruct> usedNodes;
    public Dictionary<long, BuildingStruct> buildings;

    public ChunkStruct( Dictionary<long, NodeStruct> usedNodes, Dictionary<long, BuildingStruct> buildings)
    {
        this.usedNodes = usedNodes;
        this.buildings = buildings;
    }
}