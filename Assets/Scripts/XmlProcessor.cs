using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using UnityEngine;
using System.Linq;

public class XmlProcessor
{
    MapSettings mapSettings;
    GlobalMapData globalMapData;
    Vector3 worldOrigin;

    public XmlProcessor(GlobalMapData globalMapData, MapSettings mapSettings)
    {
        this.globalMapData = globalMapData;
        this.mapSettings = mapSettings;
        this.worldOrigin = mapSettings.worldOrigin;
    }
    
    public IEnumerator LoadData(XmlNode root)
    {        
        List<NodeStruct> areaNodes = GetNodes(root);

        Dictionary<long, NodeStruct> nsd = new(); 
        foreach (NodeStruct nodeStruct in areaNodes)
        {
            if (nsd.ContainsKey(nodeStruct.nodeID))
                continue;
            nsd.Add(nodeStruct.nodeID, nodeStruct);
        }

        yield return null;
        List<BuildingStruct> areaBuildings = LoadBuildings(root, nsd);
        
        // move upper part to separate threads
        
        int breakCounter = 0; 

        // injecting nodes
        foreach (NodeStruct nodeStruct in areaNodes)
        {
            if (globalMapData.nodes.ContainsKey(nodeStruct.nodeID))
                continue;
            
            Node node = new(nodeStruct, globalMapData, mapSettings);
            globalMapData.nodes.Add(nodeStruct.nodeID, node);
            
            breakCounter++;
            if (breakCounter % 1000 == 0)
            {
                Debug.Log(breakCounter);
                yield return null;
            }
        }

        // injecting buildings
        foreach (BuildingStruct buildingStruct in areaBuildings)
        {
            if (globalMapData.buildings.ContainsKey(buildingStruct.buildingID))
                continue;
            
            Building building = new(buildingStruct, globalMapData, mapSettings);
            globalMapData.buildings.Add(buildingStruct.buildingID, building);
            breakCounter++;
            if (breakCounter % 100 == 0)
            {
                Debug.Log(breakCounter);
                yield return null;
            }
        }
    }

    private List<NodeStruct> GetNodes(XmlNode root)
    {
        List<NodeStruct> nodeStructs = new();
        XmlNodeList nodeList = root.SelectNodes("descendant::node");
        
        foreach (XmlNode nodeTag in nodeList)
        {
            long id = Convert.ToInt64(nodeTag.Attributes.GetNamedItem("id").Value);
            double latitude = Convert.ToDouble(nodeTag.Attributes.GetNamedItem("lat").Value);
            double longitude = Convert.ToDouble(nodeTag.Attributes.GetNamedItem("lon").Value);

            NodeType type = NodeType.Generic;
            NodeStruct nodeStruct = new(latitude, longitude, id, type);

            nodeStructs.Add(nodeStruct);
        }
        Debug.Log("Loading node structs completed");

        return nodeStructs;
    }

    private List<BuildingStruct> LoadBuildings(XmlNode root, Dictionary<long, NodeStruct> nsd)
    {
        List<BuildingStruct> buildingStructs = new();
        XmlNodeList buildingsNodes = root.SelectNodes("descendant::way[tag[@k='building']]");

        foreach (XmlNode buildingNode in buildingsNodes)
        {
            long id = Convert.ToInt64(buildingNode.Attributes.GetNamedItem("id").Value);

            List<NodeStruct> perimeter = new();
            XmlNodeList perimeterNodes = buildingNode.SelectNodes("descendant::nd");
            
            foreach (XmlNode perimeterNode in perimeterNodes)
            {
                long perimeterNodeId = Convert.ToInt64(perimeterNode.Attributes.GetNamedItem("ref").Value);

                NodeStruct ns = nsd[perimeterNodeId];
                perimeter.Add(ns);
            }

            int levels = GetBuildingLevels(buildingNode);            
            BuildingStruct buildingStruct = new(id, perimeter, levels);
            buildingStructs.Add(buildingStruct);

            Debug.Log("Loading building structs completed");
        }
        return buildingStructs;
    }

    private int GetBuildingLevels(XmlNode buildingNode)
    {
        int levels = 1;
        XmlNode buildingLevelNode = buildingNode.SelectSingleNode("descendant::tag[@k='building:levels']");
        if (buildingLevelNode != null)
            int.TryParse(buildingLevelNode.Attributes.GetNamedItem("v").Value, out levels);
        return levels;
    }
}