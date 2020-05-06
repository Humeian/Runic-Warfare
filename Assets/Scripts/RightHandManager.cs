using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RightHandManager : MonoBehaviour
{
    public XRController controller;
    private InputDevice inputDevice;
    private XRRayInteractor rayInteractor;
    private LineRenderer lineRenderer;
    private XRInteractorLineVisual lineVisual;
    public InputHelpers.Button enableRayButton; 
    public GlyphRecognition glyphRecognition;
    public float activationThreshold = 0.1f;
    public VRPlayerBehaviour player;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<XRController>();
        inputDevice = controller.inputDevice;
        rayInteractor = GetComponent<XRRayInteractor>();
        lineRenderer = GetComponent<LineRenderer>();
        lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    public bool CheckIfActivated(XRController controller){
        InputHelpers.IsPressed(controller.inputDevice, enableRayButton, out bool isActivated, activationThreshold);
        return isActivated;
    }

    public bool CheckIfRayHit(XRController controller){
        return rayInteractor.GetCurrentRaycastHit(out RaycastHit rayhit);
    }

    // Update is called once per frame
    void Update()
    {
        if (rayInteractor) {
            lineVisual.enabled = CheckIfRayHit(controller);
            //rayInteractor.enabled = CheckIfActivated(controller);
            lineVisual.reticle.SetActive(CheckIfRayHit(controller));
        }   

        if (CheckIfActivated(controller)){
            glyphRecognition.Cast();
        }

        controller.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressed);
        if (pressed) player.CastFireball(0, 0f);
    }
}
