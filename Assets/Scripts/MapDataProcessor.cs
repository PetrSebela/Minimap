using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using UnityEngine;

public class MapDataProcessor
{
    Dictionary<long, Node> nodes = new();
    Dictionary<long, Building> buildings = new();
    Dictionary<long, Road> roads = new();

    Dictionary<Vector3, Chunk> chunkDictionary;
    Material defaultMaterial;
    Vector3 worldOrigin;

    public MapDataProcessor(Dictionary<Vector3, Chunk> targetDictionary, Material defaultMaterial, Vector3 worldOrigin)
    {
        chunkDictionary = targetDictionary;
        this.defaultMaterial = defaultMaterial;
        this.worldOrigin = worldOrigin;
    }
    
    public void LoadData(XmlNode root)
    {
        LoadNodes(root);
        LoadBuildings(root);
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
            nodes.Add(id, node);
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

            GameObject chunkObject = GetChunk(globalCenter).gameObject;

            int levels = GetBuildingLevels(buildingNode);            

            Building building = new(id, defaultMaterial, chunkObject.transform, buildingPerimeter, levels, worldOrigin);
            buildings.Add(id, building);
        }
    }

    private Chunk GetChunk(Vector3 position)
    {
        Vector3 chunkKey = new(
            Mathf.Round(position.x / 100),
            0,
            Mathf.Round(position.z / 100)
        );

        Chunk chunk;
        if (chunkDictionary.ContainsKey(chunkKey))
            chunk = chunkDictionary[chunkKey];
        else
        {
            Vector3 chunkOrigin = chunkKey * 100 - worldOrigin;
            chunk = new(chunkOrigin);
            chunkDictionary.Add(chunkKey, chunk);                
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

            if (!nodes.ContainsKey(nodeId))
                continue;

            Node node = nodes[nodeId];
            perimeter.Add(node);
        }

        return perimeter;
    }
}
