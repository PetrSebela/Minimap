using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class CacheProcessor
{
    HashSet<int> cachedChunks;
    GlobalMapData globalMapData;
    MapSettings mapSettings;
    
    public CacheProcessor(GlobalMapData globalMapData, MapSettings mapSettings)
    {
        cachedChunks = GetCachedChunks();
        this.globalMapData = globalMapData;
        this.mapSettings = mapSettings;
    }

    public void LoadAllCachedChunks()
    {        
        DirectoryInfo directoryInfo = new(Application.persistentDataPath);
        FileInfo[] files = directoryInfo.GetFiles("*.mapdata");

        foreach (FileInfo file in files)
        {
            string jsonString = File.ReadAllText(file.FullName);
            ChunkStruct chunkStruct = JsonConvert.DeserializeObject<ChunkStruct>(jsonString);
            LoadChunk(chunkStruct);
        }
    }

    public void LoadChunk(ChunkStruct chunkStruct)
    {
        foreach (NodeStruct nodeStruct in chunkStruct.usedNodes.Values)
        {
            if (globalMapData.nodes.ContainsKey(nodeStruct.nodeID))
                continue;

            Node node = new(nodeStruct, globalMapData, mapSettings);
            globalMapData.nodes.Add(nodeStruct.nodeID, node);
        }

        // injecting buildings
        foreach (BuildingStruct buildingStruct in chunkStruct.buildings.Values)
        {
            if (globalMapData.buildings.ContainsKey(buildingStruct.buildingID))
                continue;
            
            Building building = new(buildingStruct, globalMapData, mapSettings);
            globalMapData.buildings.Add(buildingStruct.buildingID, building);
        }
    }

    public static HashSet<int> GetCachedChunks()
    {
        HashSet<int> cachedChunks = new();
        
        DirectoryInfo directoryInfo = new(Application.persistentDataPath);
        FileInfo[] files = directoryInfo.GetFiles("*.mapdata");

        foreach (FileInfo file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            int chunkCode = Convert.ToInt32(fileName);
            cachedChunks.Add(chunkCode);
        }
        return cachedChunks;
    }

    public bool ChunkInCache(Vector3 chunkKey)
    {
        int chunkCode = chunkKey.GetHashCode();
        return cachedChunks.Contains(chunkCode);
    }


    public static ChunkStruct LoadChunk(Vector3 chunkKey, GlobalMapData globalMapData, MapSettings mapSettings)
    {
        int chunkCode = chunkKey.GetHashCode();
        string fileName = string.Format("{0}.mapdata", chunkCode);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string jsonString = File.ReadAllText(path);

        ChunkStruct chunkStruct = JsonConvert.DeserializeObject<ChunkStruct>(jsonString);
        return chunkStruct;
    }
}
