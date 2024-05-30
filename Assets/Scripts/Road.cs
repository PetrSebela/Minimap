using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road
{
    public List<Node> nodes = new();
    string name;
    long id;

    public Road(long id, string name)
    {
        this.name = name;
        this.id = id;
    }

    public void RenderGizmo(Vector3 offset)
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector3 a = nodes[i].GetPointWithOffset(offset);
            Vector3 b = nodes[i + 1].GetPointWithOffset(offset);
            Gizmos.DrawLine(a, b);
        }
    }
}
