using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tag", menuName = "MapData/Tag", order = 1)]
public class Tag : ScriptableObject
{
    public GameObject gameObject;
    public string localXpath;
    public NodeType nodeType;
}
