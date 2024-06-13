using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Threading;
using System.Collections;

public class MapController : MonoBehaviour
{

    // map data
    Dictionary<long, Node> nodes = new();
    Dictionary<long, Building> buildings = new();
    Dictionary<long, Road> roads = new();
    Dictionary<Vector3, Chunk> chunkDictionary = new();
    GlobalMapData globalMapData;


    [Header("Map settings")]
    public Tag[] nodeTags;
    public RoadTag[] roadTags;
    public Transform billboardTarget;
    public Material material;
    public bool loadFromAPI;




    // Tree at VUT FIT
    // latitude = 49.2264482;
    // longitude = 16.5953301;
    MapSettings mapSettings;

    [SerializeField] Coordinates worldOriginCoordinates;
    Vector3 worldOrigin;



    // Dynamic map loading 
    const float chunkSize = 250.0f; //! clear cashe if you change this number
    const int chunksInArea = 15;
    float areaSize;

    // API variables
    XmlProcessor xmlProcessor;
    CacheProcessor cacheProcessor;
    Queue<string> xmlMapDataQueue = new();

    
    void Start()
    {
        worldOrigin = Geo.SphericalToCartesian(worldOriginCoordinates.latitude, worldOriginCoordinates.longitude);

        mapSettings = new(worldOrigin, material, chunkSize, chunksInArea, nodeTags, billboardTarget, roadTags);
        globalMapData = new(nodes, buildings, roads, chunkDictionary);
        areaSize = chunksInArea * chunkSize;
        xmlProcessor = new(globalMapData, mapSettings);
        cacheProcessor = new(globalMapData, mapSettings);


        if (loadFromAPI)
        {
            // load area from API 
            AreaData loaderData = new(worldOrigin - new Vector3(areaSize / 2, 0, areaSize / 2), areaSize, xmlMapDataQueue);
            ThreadPool.QueueUserWorkItem(ChunkLoader.AreaLoader, loaderData);
        }
        else
        {
            // load chunks from cache
            cacheProcessor.LoadAllCachedChunks();
        }
    }

    void FixedUpdate()
    {
        ProcessXmlMapData();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        foreach (Road road in roads.Values)
            road.DrawGizmo();

        foreach (Node node in nodes.Values)
            node.DrawGizmo();
    }

    public void ProcessXmlMapData()
    {
        string mapData;
        lock (xmlMapDataQueue)
        {
            if (xmlMapDataQueue.Count <= 0)
                return;
            mapData = xmlMapDataQueue.Dequeue();
        }
        Debug.Log("Processing data");
        XmlDocument mapXml = new();
        mapXml.LoadXml(mapData);
        XmlNode root = mapXml.DocumentElement;
        StartCoroutine(xmlProcessor.LoadData(root));
    }
}



public struct MapSettings
{
    public Vector3 worldOrigin;
    public Material buildingMaterial;
    public float chunkSize;
    public int chunksInArea;
    public Tag[] tags;
    public Transform billboardTarget;
    public RoadTag[] roadTags;

    public MapSettings(Vector3 worldOrigin, Material buildingMaterial, float chunkSize, int chunksInArea, Tag[] tags, Transform billboardTarget, RoadTag[] roadTags)
    {
        this.worldOrigin = worldOrigin;
        this.buildingMaterial = buildingMaterial;
        this.chunkSize = chunkSize;
        this.chunksInArea = chunksInArea;
        this.tags = tags;
        this.billboardTarget = billboardTarget;
        this.roadTags = roadTags;
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
