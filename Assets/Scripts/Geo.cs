using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Geo
{
    const double RADIUS_MAJOR = 6378137.0;
    // const double RADIUS_MINOR = 6356752.3142;
    public static Vector3 SphericalToCartesian(double lattitude, double longtitude) // Spherical mercator projection (ref DESIGN_DOC if better accuracy is needed) 
    {
        double x = Mathf.Deg2Rad * longtitude * RADIUS_MAJOR;
        double y = Math.Log(Math.Tan(Math.PI / 4 + (Math.PI / 180 * lattitude / 2))) * RADIUS_MAJOR;

        return new Vector3((float)x, 0 ,(float)y);
    }

    public static Coordinates CartesianToSpherical(Vector3 position)
    {
        double longitude = position.x / RADIUS_MAJOR * Mathf.Rad2Deg;
        double latitude = (2 * Math.Atan(Math.Exp(position.z / RADIUS_MAJOR)) - math.PI / 2) * (180 / Math.PI);

        return new Coordinates(latitude, longitude);
    }
}

[System.Serializable]
public struct Coordinates
{
    public double latitude;
    public double longitude;

    public Coordinates(double latitude, double longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
    }
}
