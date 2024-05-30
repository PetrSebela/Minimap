using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Windows;

public class MapController : MonoBehaviour
{
    [SerializeField] private string mapFilePath;
    public int maxContinuousLoadingTimeMilis = 25;
    public Material material;


    Dictionary<long, Node> nodes = new();
    Dictionary<long, Building> buildings = new();
    Dictionary<long, Road> roads = new();

    Vector3 worldOffset;

    void Start()
    {
        StartCoroutine(LoadMap()); 
    }


    public IEnumerator LoadMap()
    {
        if (!File.Exists(Application.dataPath + "/" + mapFilePath))
        {
            Debug.LogError("Map file does not exist");
            yield break;
        }

        double start = Time.realtimeSinceStartupAsDouble;

        XmlDocument mapXml = new XmlDocument();
        mapXml.Load(Application.dataPath + "/" + mapFilePath);
        XmlNode root = mapXml.DocumentElement;

        XmlNode bound = root.SelectSingleNode("descendant::bounds");
        double minLat = Convert.ToDouble(bound.Attributes.GetNamedItem("minlat").Value);
        double minLon = Convert.ToDouble(bound.Attributes.GetNamedItem("minlon").Value);
        double maxLat = Convert.ToDouble(bound.Attributes.GetNamedItem("maxlat").Value);
        double maxLon = Convert.ToDouble(bound.Attributes.GetNamedItem("maxlon").Value);


        Vector3 destination = Geo.SphericalToCartesian(maxLat, maxLon);
        Vector3 origin = Geo.SphericalToCartesian(minLat, minLon);
        worldOffset =  origin + (destination - origin) / 2;

        Debug.LogFormat("Map bounds {0} | {1} | {2} | {3}", minLat, minLon, maxLat, maxLon);


        // loading all nodes
        XmlNodeList nodeList = mapXml.SelectNodes("descendant::node");
        
        double breakTimer = Time.realtimeSinceStartup;

        foreach (XmlNode nodeTag in nodeList)
        {
            long nodeID = Convert.ToInt64(nodeTag.Attributes.GetNamedItem("id").Value);
            double latitude = Convert.ToDouble(nodeTag.Attributes.GetNamedItem("lat").Value);
            double longitude = Convert.ToDouble(nodeTag.Attributes.GetNamedItem("lon").Value);

            NodeType type = NodeType.Generic;

            XmlNode tree = nodeTag.SelectSingleNode("descendant::tag[@k='natural' and @v='tree']");
            XmlNode lamp = nodeTag.SelectSingleNode("descendant::tag[@k='highway' and @v='street_lamp']");

            if (tree != null)
                type = NodeType.Tree;

            if (lamp != null)
                type = NodeType.Lamp;

            Node node = new(latitude, longitude, nodeID, type);
            nodes.Add(nodeID, node);


            if((Time.realtimeSinceStartup - breakTimer) * 1000 >= maxContinuousLoadingTimeMilis)
            {
                Debug.Log("taking a break");
                breakTimer = Time.realtimeSinceStartup;
                yield return null;
            }
        }
        Debug.Log("Loaded nodes");
        Debug.Log("Building loading");


        // loading all building
        XmlNodeList buildingsNodes = mapXml.SelectNodes("descendant::way[tag[@k='building']]");
        Debug.Log(buildingsNodes.Count);

        foreach (XmlNode buildingNode in buildingsNodes)
        {
            long id = Convert.ToInt64(buildingNode.Attributes.GetNamedItem("id").Value);

            // find perimeter
            List<Node> buildingPerimeter = new();
            XmlNodeList perimeterNodes = buildingNode.SelectNodes("descendant::nd");
            foreach (XmlNode nodeReference in perimeterNodes)
            {
                long nodeId = Convert.ToInt64(nodeReference.Attributes.GetNamedItem("ref").Value);

                if (!nodes.ContainsKey(nodeId))
                    continue;

                Node node = nodes[nodeId];
                buildingPerimeter.Add(node);
            }

            // find height
            int levels = 1;
            XmlNode buildingLevelNode = buildingNode.SelectSingleNode("descendant::tag[@k='building:levels']");
            if (buildingLevelNode != null)
            {
                int buildingLevels = Convert.ToInt32(buildingLevelNode.Attributes.GetNamedItem("v").Value);
                levels = buildingLevels;
            }

            Building building = new(id, material, this.transform, buildingPerimeter, levels, worldOffset);
            buildings.Add(id, building);
        
            
            if((Time.realtimeSinceStartup - breakTimer) * 1000 >= maxContinuousLoadingTimeMilis)
            {
                Debug.Log("taking a break");
                breakTimer = Time.realtimeSinceStartup;
                yield return null;
            }
        }

        // loading all roads
        XmlNodeList roadNodes = mapXml.SelectNodes("descendant::way[tag[@k='highway' and (@v='tertiary' or @v='residential')]]");
        foreach (XmlNode roadNode in roadNodes)
        {
            long id = Convert.ToInt64(roadNode.Attributes.GetNamedItem("id").Value);
            XmlNode nameTag = roadNode.SelectSingleNode("descendant::tag[@k='name']");
            string name = "";

            if (nameTag != null)
                name = nameTag.Attributes.GetNamedItem("v").Value;

            Road road = new(id, name);

            // find perimeter
            XmlNodeList referencedNodes = roadNode.SelectNodes("descendant::nd");

            foreach (XmlNode reference in referencedNodes)
            {
                long nodeId = Convert.ToInt64(reference.Attributes.GetNamedItem("ref").Value);

                if (!nodes.ContainsKey(nodeId))
                    continue;

                Node node = nodes[nodeId];
                road.nodes.Add(node);
            }

            roads.Add(id, road);


            if((Time.realtimeSinceStartup - breakTimer) * 1000 >= maxContinuousLoadingTimeMilis)
            {
                Debug.Log("taking a break");
                breakTimer = Time.realtimeSinceStartup;
                yield return null;
            }
        }


        Debug.LogFormat("Processed {0} nodes in {1}ms ({2}s)", nodeList.Count, (Time.realtimeSinceStartupAsDouble - start) * 1000, Time.realtimeSinceStartupAsDouble - start);
        Debug.Log("Map loaded");

        yield break;
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
        if (nodes == null)
            return;

        foreach (Node node in nodes.Values)
        {
            switch (node.type)
            {
                case NodeType.Lamp:
                    node.DrawGizmo(worldOffset, Color.yellow, 1);
                    break;

                case NodeType.Tree:
                    node.DrawGizmo(worldOffset, Color.green, 2);
                    break;

                default:
                    break;
            }
        }


        // if(buildings == null)
        //     return;


        foreach (Building building in buildings.Values)
        {
            // Drawing perimeter outline
            List<Vector3> points = new();

            foreach (Node node in building.perimeter)
                points.Add(node.GetPointWithOffset(worldOffset));
            points.Add(building.perimeter[0].GetPointWithOffset(worldOffset));


            Gizmos.color = building.representedColor;
            for (int i = 0; i < points.Count - 2; i++)
            {
                Vector3 a = points[i];
                Vector3 b = points[i + 1];
                Gizmos.DrawLine(a, b);
            }

            Gizmos.DrawSphere(building.buildingCenter, 0.1f);
        }

        foreach (Road road in roads.Values)
        {
            road.RenderGizmo(worldOffset);
        }
    }
}
