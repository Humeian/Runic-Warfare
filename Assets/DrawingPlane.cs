using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawingPlane : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject blockingPlane;

    public XRController controller;
    public InputHelpers.Button enableRayButton; 
    public float activationThreshold = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward);
        InputHelpers.IsPressed(controller.inputDevice, enableRayButton, out bool isActivated, activationThreshold);
        blockingPlane.SetActive(!isActivated);

        // float distance = Vector3.Distance( anchor.transform.position, player.transform.position );
        // if (distance > 2.0f) {
        //     anchor.transform.position
        // }
    }
}
