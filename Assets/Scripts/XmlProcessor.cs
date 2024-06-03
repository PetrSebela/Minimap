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
        
        // move all code above onto separate thread
        
        int breakCounter = 0; 
        
        HashSet<Chunk> effectedChunks = new();

        // injecting nodes
        foreach (NodeStruct nodeStruct in areaNodes)
        {
            if (globalMapData.nodes.ContainsKey(nodeStruct.nodeID))
                continue;

            Node node = new(nodeStruct, globalMapData, mapSettings);
            globalMapData.nodes.Add(nodeStruct.nodeID, node);
            effectedChunks.Add(node.GetParentChunk());
            breakCounter++;
            if (breakCounter % 1000 == 0)
                yield return null;
        }

        // injecting buildings
        foreach (BuildingStruct buildingStruct in areaBuildings)
        {
            if (globalMapData.buildings.ContainsKey(buildingStruct.buildingID))
                continue;
            
            Building building = new(buildingStruct, globalMapData, mapSettings);
            globalMapData.buildings.Add(buildingStruct.buildingID, building);
            effectedChunks.Add(building.GetParentChunk());

            breakCounter++;
            if (breakCounter % 100 == 0)
                yield return null;
        }


        foreach (Chunk chunk in effectedChunks)
            chunk.Serialize();
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

            NodeType type = GetNodeType(nodeTag);
            NodeStruct nodeStruct = new(latitude, longitude, id, type);

            nodeStructs.Add(nodeStruct);
        }
        Debug.Log("Loading node structs completed");

        return nodeStructs;
    }

    private NodeType GetNodeType(XmlNode root)
    {
        NodeType nodeType = NodeType.Generic;

        if (null != root.SelectSingleNode("descendant::tag[@k='man_made' and @v='surveillance']"))
            nodeType = NodeType.Surveillance;
        
        if (null != root.SelectSingleNode("descendant::tag[@k='highway' and @v='street_lamp']"))
            nodeType = NodeType.Lamp;

        if (null != root.SelectSingleNode("descendant::tag[@k='man_made' and @v='chimney']"))
            nodeType = NodeType.Chimney;

        if (null != root.SelectSingleNode("descendant::tag[@k='man_made' and @v='cooling_tower']"))
            nodeType = NodeType.CoolingTower;
        
        if (null != root.SelectSingleNode("descendant::tag[@k='man_made' and @v='communications_tower']"))
            nodeType = NodeType.CommunicationsTower;
        
        if (null != root.SelectSingleNode("descendant::tag[@k='man_made' and @v='antenna']"))
            nodeType = NodeType.Antenna;
        return nodeType;
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