﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class LeftHandManager : MonoBehaviour
{
    public XRController controller;
    private InputDevice inputDevice;
    public List<InputHelpers.Button> startMoveButtons; 
    public float activationThreshold = 0.2f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
