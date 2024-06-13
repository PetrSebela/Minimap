using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoadType", menuName = "MapData/RoadType", order = 2)]
public class RoadTag : ScriptableObject
{
    public string xPath;
    public float laneWidth;
    public RoadType roadType;
}


//! fill in the types and coresponding SGOs from 'https://wiki.openstreetmap.org/wiki/Key:highway?uselang=en'
public enum RoadType
{
    road,
    tertiary,
    service,
    trunk_link,
    trunk,
    residential,
    secondary_link,
    secondary
}