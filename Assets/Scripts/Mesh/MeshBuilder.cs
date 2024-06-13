using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshBuilder
{
    public const float intersectionOffset = 2.5f;
    public static Vector3[] GetLineVertexNormals(RoadPoint[] line)
    {
        Vector3[] normals = new Vector3[line.Length];

        for (int vertexIndex = 0; vertexIndex < line.Length - 1; vertexIndex++)
        {
            Vector3 a = line[vertexIndex ].position;
            Vector3 b = line[vertexIndex + 1].position;
            Vector3 segment = b - a;

            Vector3 normal = new Vector3(-segment.z, 0.0f, segment.x).normalized;

            normals[vertexIndex] += normal;
            normals[vertexIndex + 1] += normal;       
        }

        for (int vertexIndex = 0; vertexIndex < line.Length ; vertexIndex++)
            normals[vertexIndex].Normalize();

        return normals;
    }

    public static RoadPoint[] GetRoadPoints(Node[] nodes)
    {
        if (nodes.Length <= 1)
            return null;
        
        List<RoadPoint> points = new();

        if (nodes[0].usedByRoads.Count != 1)
        {
            Vector3 a = nodes[0].GetLocalizedPoint();
            Vector3 b = nodes[1].GetLocalizedPoint();
            Vector3 localizedVector = (b - a).normalized;

            RoadPoint shared = new(a, false); 
            RoadPoint localized = new(a + localizedVector * intersectionOffset, true); 
            points.Add(shared);
            points.Add(localized);
        }
        else
        {
            RoadPoint shared = new(nodes[0].GetLocalizedPoint(), true); 
            points.Add(shared);
        }

        for (int i = 1; i < nodes.Length - 1; i++)
        {
            if(nodes[i].usedByRoads.Count == 1)
            {
                RoadPoint local = new(nodes[i].GetLocalizedPoint(), true); 
                points.Add(local);
                continue;
            }

            Vector3 a = nodes[i - 1].GetLocalizedPoint();
            Vector3 b = nodes[i].GetLocalizedPoint();
            Vector3 c = nodes[i + 1].GetLocalizedPoint();
            
            Vector3 localizedVector1 = (a - b).normalized;
            Vector3 localizedVector2 = (c - b).normalized;            


            RoadPoint shared = new(b, false); 
            RoadPoint localized1 = new(a + localizedVector1 * intersectionOffset, true); 
            RoadPoint localized2 = new(a + localizedVector2 * intersectionOffset, true); 

            points.Add(localized1);
            points.Add(shared);
            points.Add(localized2);
        }

        if (nodes[^1].usedByRoads.Count != 1)
        {
            Vector3 a = nodes[^1].GetLocalizedPoint();
            Vector3 b = nodes[^2].GetLocalizedPoint();
            Vector3 localizedVector = (b - a).normalized;
        
            RoadPoint shared = new(a, false); 
            RoadPoint localized = new(a + localizedVector * intersectionOffset, true); 
            points.Add(localized);
            points.Add(shared);
        }
        else
        {
            RoadPoint shared = new(nodes[^1].GetLocalizedPoint(), true); 
            points.Add(shared);
        }

        return points.ToArray();
    }
}


public struct RoadPoint
{
    public Vector3 position;
    public bool isLocal;

    public RoadPoint(Vector3 position, bool isLocal)
    {
        this.position = position;
        this.isLocal = isLocal;
    }
}