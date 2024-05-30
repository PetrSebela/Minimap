using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geo
{
    const double RADIUS_MAJOR = 6378137.0;
    // const double RADIUS_MINOR = 6356752.3142;
    public static Vector3 SphericalToCartesian(double lattitude, double longtitude)
    {
        double x = Mathf.Deg2Rad * longtitude * RADIUS_MAJOR;
        double y = Mathf.Log(Mathf.Tan(Mathf.PI / 4 + (Mathf.Deg2Rad * (float)lattitude / 2))) * RADIUS_MAJOR;

        return new Vector3((float)x, 0 ,(float)y);
    }
}
