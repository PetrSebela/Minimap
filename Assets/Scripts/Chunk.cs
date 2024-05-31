using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    Vector3 position;
    public GameObject gameObject;

    public List<Node> pointsOfInterest;
    public List<Building> buildings;
    public List<Road> roads;
    
    public Chunk(Vector3 position)
    {
        gameObject = new(position.ToString());
        gameObject.transform.position = position;
    }

    public void DrawGizmos()
    {
        Gizmos.DrawWireCube(gameObject.transform.position, new Vector3(100, 1, 100));
    }
}
