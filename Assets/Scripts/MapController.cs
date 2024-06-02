using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Threading;
using System.Collections;

public class MapController : MonoBehaviour
{
    [SerializeField] private string mapFilePath;
    public Material material;

    // map data
    Dictionary<long, Node> nodes = new();
    Dictionary<long, Building> buildings = new();
    Dictionary<long, Road> roads = new();
    Dictionary<Vector3, Chunk> chunkDictionary = new();


    Vector3 worldOrigin;

    // Tree at VUT FIT
    // latitude = 49.2264482;
    // longitude = 16.5953301;
    [SerializeField] Coordinates worldOriginCoordinates;




    // map settings
    MapSettings mapSettings;
    GlobalMapData globalMapData;


    // Dynamic map loading 
    const float chunkSize = 250.0f; //! clear cashe if you change this number
    const int chunksInArea = 50;
    Queue<string> xmlMapDataQueue = new();

    float areaSize;
    XmlProcessor processor;

    void Start()
    {
        worldOrigin = Geo.SphericalToCartesian(worldOriginCoordinates.latitude, worldOriginCoordinates.longitude);

        mapSettings = new(worldOrigin, material, chunkSize, chunksInArea);
        globalMapData = new(nodes, buildings, roads, chunkDictionary);
        areaSize = chunksInArea * chunkSize;
        processor = new(globalMapData, mapSettings);

        AreaData loaderData = new(worldOrigin - new Vector3(areaSize / 2, 0, areaSize / 2), areaSize, xmlMapDataQueue);
        ThreadPool.QueueUserWorkItem(ChunkLoader.AreaLoader, loaderData);
    }

    void FixedUpdate()
    {
        ProcessXmlMapData();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (Chunk chunk in chunkDictionary.Values)
            chunk.DrawGizmos();
    }

    public void ProcessXmlMapData()
    {

        string mapData;
        lock (xmlMapDataQueue)
        {
            if(xmlMapDataQueue.Count <= 0)
                return;
            mapData = xmlMapDataQueue.Dequeue();
        }
        Debug.Log("Processing data");
        XmlDocument mapXml = new();
        mapXml.LoadXml(mapData);
        XmlNode root = mapXml.DocumentElement;
        StartCoroutine(processor.LoadData(root));
    }

    public void LoadTest()
    {
        Debug.Log("Starting map loading");
        double startTime = Time.realtimeSinceStartupAsDouble;

        if (!System.IO.File.Exists(Application.dataPath + "/" + mapFilePath))
        {
            Debug.LogError("Map file does not exist");
            return;
        }

        // load xml map data
        XmlDocument mapXml = new();
        mapXml.Load(Application.dataPath + "/" + mapFilePath);
        XmlNode root = mapXml.DocumentElement;

        MapSettings mapSettings = new(worldOrigin, material, chunkSize, chunksInArea);
        GlobalMapData globalMapData = new(nodes, buildings, roads, chunkDictionary);

        XmlProcessor processor = new(globalMapData, mapSettings);
        processor.LoadData(root);

        Debug.LogFormat("Finished initial loading in {0}s", Time.realtimeSinceStartupAsDouble - startTime);
    }
}



public struct MapSettings
{
    public Vector3 worldOrigin;
    public Material buildingMaterial;
    public float chunkSize;
    public int chunksInArea;

    public MapSettings(Vector3 worldOrigin, Material buildingMaterial, float chunkSize, int chunksInArea)
    {
        this.worldOrigin = worldOrigin;
        this.buildingMaterial = buildingMaterial;
        this.chunkSize = chunkSize;
        this.chunksInArea = chunksInArea;
    }
}


public struct GlobalMapData
{
    public Dictionary<long, Node> nodes;
    public Dictionary<long, Building> buildings;
    public Dictionary<long, Road> roads;
    public Dictionary<Vector3, Chunk> chunkDictionary;


    public GlobalMapData(Dictionary<long, Node> nodes,
    Dictionary<long, Building> buildings,
    Dictionary<long, Road> roads,
    Dictionary<Vector3, Chunk> chunkDictionary)
    {
        this.nodes = nodes;
        this.buildings = buildings;
        this.roads = roads;
        this.chunkDictionary = chunkDictionary;
    }
}
