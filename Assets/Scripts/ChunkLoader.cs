using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Net.Http;

public static class ChunkLoader 
{
    // API stuff
    // https://overpass-api.de/api/map?bbox=16.59572,49.22596,16.59612,49.22678
    // left bottom right top
    const string overpassURL = "https://overpass-api.de/";
    const string overpassAPI = "api/map?bbox={0},{1},{2},{3}";
    
    
    // Loads area specified by AreaData struct

    public async static void AreaLoader(object data)
    {
        AreaData loaderData = (AreaData)data;
        HttpClient client = new();
        
        Coordinates chunkOrigin = Geo.CartesianToSpherical(loaderData.chunkPosition);
        Coordinates chunkDestination = Geo.CartesianToSpherical(loaderData.chunkPosition + new Vector3(loaderData.areaSize, 0, loaderData.areaSize));

        Debug.LogFormat("Loading {0} {1} {2} {3}",chunkOrigin.longitude, chunkOrigin.latitude, chunkDestination.longitude, chunkDestination.latitude);;
        
        client.BaseAddress = new Uri(overpassURL);
        string request = string.Format(overpassAPI,chunkOrigin.longitude, chunkOrigin.latitude, chunkDestination.longitude, chunkDestination.latitude);
        Debug.Log(request);
        HttpResponseMessage response = await client.GetAsync(request);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            lock (loaderData.callbackQueue)
                loaderData.callbackQueue.Enqueue(responseContent);
            Debug.Log("request succesful");
        }
        else
        {
            Debug.LogWarningFormat("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
        }
    }
}


public struct AreaData
{
    public Vector3 chunkPosition;
    public float areaSize;
    public Queue<string> callbackQueue;

    public AreaData(Vector3 chunkPosition, float areaSize, Queue<string> callbackQueue)
    {
        this.chunkPosition = chunkPosition;
        this.areaSize = areaSize;
        this.callbackQueue = callbackQueue;
    }
}
