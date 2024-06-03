using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform cameraTransform;
    void Update()
    {
        this.transform.rotation = cameraTransform.rotation;
    }
}
