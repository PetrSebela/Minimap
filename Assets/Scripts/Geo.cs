using System;
using System.Collections;
using System.Collections.Generic;
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
}
