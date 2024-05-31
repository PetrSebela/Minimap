using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private string mapFilePath;
    public int maxContinuousLoadingTimeMilis = 25;
    public Material material;


    Dictionary<long, Node> nodes = new();
    Dictionary<long, Building> buildings = new();
    Dictionary<long, Road> roads = new();
    Dictionary<Vector3, Chunk> chunkDictionary = new();

    Vector3 worldOffset;

    void Start()
    {
        LoadTest();
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

        // map boundaries
        XmlNode bound = root.SelectSingleNode("descendant::bounds");
        double minLat = Convert.ToDouble(bound.Attributes.GetNamedItem("minlat").Value);
        double minLon = Convert.ToDouble(bound.Attributes.GetNamedItem("minlon").Value);
        double maxLat = Convert.ToDouble(bound.Attributes.GetNamedItem("maxlat").Value);
        double maxLon = Convert.ToDouble(bound.Attributes.GetNamedItem("maxlon").Value);

        // map center
        Vector3 destination = Geo.SphericalToCartesian(maxLat, maxLon);
        Vector3 origin = Geo.SphericalToCartesian(minLat, minLon);
        worldOffset =  origin + (destination - origin) / 2;

        MapSettings mapSettings = new(worldOffset, material);
        GlobalMapData globalMapData = new(nodes, buildings, roads, chunkDictionary);


        MapDataProcessor processor = new(globalMapData, mapSettings);
        processor.LoadData(root);

        Debug.LogFormat("Finished initial loading in {0}s", Time.realtimeSinceStartupAsDouble - startTime);
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
