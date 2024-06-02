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
    
    public CacheProcessor()
    {
        cachedChunks = GetCachedChunks();
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
