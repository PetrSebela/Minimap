using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform cameraHolder;
    [SerializeField] Transform tiltAxis;
    Controls controls;
    Vector3 cameraVelocity = Vector3.zero;
    bool canPan = false;
    bool canTilt = false;
    float tiltAngle = 25f;
    float rotationAngle = 0;
    
    void Update()
    {
        // Keeping velocity after user stop panning
        if (canPan)
            cameraHolder.position += cameraHolder.forward * cameraVelocity.z + cameraHolder.right * cameraVelocity.x;
        else
        {
            cameraVelocity = Vector3.Lerp(Vector3.zero, cameraVelocity, 0.975f);
            cameraHolder.position += cameraHolder.forward * cameraVelocity.z + cameraHolder.right * cameraVelocity.x;
        }
    }

    void Awake()
    {
        controls = new();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Movement.Delta.performed += DeltaProcessor;
        controls.Movement.Delta.canceled += OnDeltaCancel;
        controls.Movement.Pan.performed += OnPan;
        controls.Movement.Pan.canceled += OnPanCancel;
        controls.Movement.Tilt.started += OnTilt;
        controls.Movement.Tilt.canceled += OnTiltCancel;
    }

    void OnDisable()
    {
        controls.Disable();
        controls.Movement.Delta.performed -= DeltaProcessor;
        controls.Movement.Delta.canceled -= OnDeltaCancel;
        controls.Movement.Pan.performed -= OnPan;
        controls.Movement.Pan.canceled -= OnPanCancel;
        controls.Movement.Tilt.started -= OnTilt;
        controls.Movement.Tilt.canceled -= OnTiltCancel;
    }

    void OnPan(InputAction.CallbackContext context)
    {
        cameraVelocity = Vector3.zero;
        canPan = true;
    }
    void OnPanCancel(InputAction.CallbackContext context)
    {
        canPan = false;
    }

    void OnTilt(InputAction.CallbackContext context)
    {
        canTilt = true;
    }
    void OnTiltCancel(InputAction.CallbackContext context)
    {
        canTilt = false;
    }

    void DeltaProcessor(InputAction.CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();

        if(canTilt)
        {
            rotationAngle += delta.x;
            cameraHolder.rotation = Quaternion.Euler(0, rotationAngle, 0); 

            tiltAngle += -delta.y;
            tiltAngle = Math.Clamp(tiltAngle, 10, 90);
            tiltAxis.localRotation = Quaternion.Euler(tiltAngle, 0,0);
        }

        if(canPan)
            cameraVelocity = new(-delta.x, 0 , -delta.y);
    }

    void OnDeltaCancel(InputAction.CallbackContext context)
    {
        // canPan = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(cameraHolder.position, cameraHolder.position + cameraHolder.forward * 10);
    }
}
