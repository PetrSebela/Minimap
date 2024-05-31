using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using UnityEngine;

public class MapDataProcessor
{
    MapSettings mapSettings;
    GlobalMapData globalMapData;
    Vector3 worldOrigin;

    public MapDataProcessor(GlobalMapData globalMapData, MapSettings mapSettings)
    {
        this.globalMapData = globalMapData;
        this.mapSettings = mapSettings;
        this.worldOrigin = mapSettings.worldOrigin;
    }
    
    public void LoadData(XmlNode root)
    {
        Chunk.LoadAllStoredChunks(globalMapData, mapSettings);
        
        
        // LoadNodes(root);
        // LoadBuildings(root);


        // foreach(Chunk chunk in globalMapData.chunkDictionary.Values)
        //     chunk.Serialize();
    }

    private void LoadNodes(XmlNode root)
    {
        XmlNodeList nodeList = root.SelectNodes("descendant::node");
        foreach (XmlNode nodeTag in nodeList)
        {
            long id = Convert.ToInt64(nodeTag.Attributes.GetNamedItem("id").Value);
            double latitude = Convert.ToDouble(nodeTag.Attributes.GetNamedItem("lat").Value);
            double longitude = Convert.ToDouble(nodeTag.Attributes.GetNamedItem("lon").Value);

            NodeType type = NodeType.Generic;
            Node node = new(latitude, longitude, id, type);
            Chunk chunk = GetChunk(node.GetRawPoint());
            chunk.nodes.Add(node);
            globalMapData.nodes.Add(id, node);
        }
    }

    private void LoadBuildings(XmlNode root)
    {
        XmlNodeList buildingsNodes = root.SelectNodes("descendant::way[tag[@k='building']]");

        foreach (XmlNode buildingNode in buildingsNodes)
        {
            long id = Convert.ToInt64(buildingNode.Attributes.GetNamedItem("id").Value);

            List<Node> buildingPerimeter = GetBuildingPerimeter(buildingNode);

            Vector3 globalCenter = Vector3.zero;
            foreach (Node node in buildingPerimeter)
                globalCenter += node.GetRawPoint();
            globalCenter /= buildingPerimeter.Count;


            Chunk chunk = GetChunk(globalCenter);
            GameObject chunkObject = chunk.gameObject;

            int levels = GetBuildingLevels(buildingNode);            

            Building building = new(id, mapSettings.buildingMaterial, chunkObject.transform, buildingPerimeter, levels, worldOrigin);
            chunk.buildings.Add(building);
            globalMapData.buildings.Add(id, building);
        }
    }

    private Chunk GetChunk(Vector3 position)
    {
        Vector3 chunkKey = new(
            Mathf.Round(position.x / 100) * 100,
            0,
            Mathf.Round(position.z / 100) * 100
        );

        Chunk chunk;
        if (globalMapData.chunkDictionary.ContainsKey(chunkKey))
            chunk = globalMapData.chunkDictionary[chunkKey];
        else
        {
            Vector3 chunkOrigin = chunkKey - worldOrigin;
            chunk = new(chunkOrigin);
            globalMapData.chunkDictionary.Add(chunkKey, chunk);                
        }
        return chunk;
    }

    private int GetBuildingLevels(XmlNode buildingNode)
    {
        int levels = 1;
        XmlNode buildingLevelNode = buildingNode.SelectSingleNode("descendant::tag[@k='building:levels']");
        if (buildingLevelNode != null)
        {
            int buildingLevels = Convert.ToInt32(buildingLevelNode.Attributes.GetNamedItem("v").Value);
            levels = buildingLevels;
        }
        return levels;
    }

    private List<Node> GetBuildingPerimeter(XmlNode buildingNode)
    {
        List<Node> perimeter = new();
        XmlNodeList perimeterNodes = buildingNode.SelectNodes("descendant::nd");
        foreach (XmlNode nodeReference in perimeterNodes)
        {
            long nodeId = Convert.ToInt64(nodeReference.Attributes.GetNamedItem("ref").Value);

            if (!globalMapData.nodes.ContainsKey(nodeId))
                continue;

            Node node = globalMapData.nodes[nodeId];
            perimeter.Add(node);
        }

        return perimeter;
    }
}


public struct MapSettings
{
    public Vector3 worldOrigin;
    public Material buildingMaterial;

    public MapSettings(Vector3 worldOrigin, Material buildingMaterial)
    {
        this.worldOrigin = worldOrigin;
        this.buildingMaterial = buildingMaterial;
    }
}