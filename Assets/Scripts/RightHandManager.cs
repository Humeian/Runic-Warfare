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
    private XRInteractorLineVisual lineVisual;

    public bool held = false;
    public InputHelpers.Button grip; 
    public GlyphRecognition glyphRecognition;
    public float activationThreshold = 0.1f;
    public PlayerBehaviour player;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<XRController>();
        inputDevice = controller.inputDevice;
        rayInteractor = GetComponent<XRRayInteractor>();
        lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    public bool CheckIfActivated(XRController controller){
        InputHelpers.IsPressed(controller.inputDevice, grip, out bool isGripped, activationThreshold);
        return (isGripped);
    }

    public bool CheckIfRayHit(XRController controller){
        return rayInteractor.GetCurrentRaycastHit(out RaycastHit rayhit);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            try {
                player = GameObject.Find("TestPlayer(Clone)").GetComponent<PlayerBehaviour>();
            } catch {
                // do nothing
            }
			

        if (rayInteractor && !held) {
            lineVisual.enabled = CheckIfRayHit(controller);
            //rayInteractor.enabled = CheckIfActivated(controller);
            lineVisual.reticle.SetActive(CheckIfRayHit(controller));
        }   

        if (player != null && CheckIfActivated(controller) && !held){
            held = true;
            glyphRecognition.Cast();
            lineVisual.enabled = true;
        }

        if (player != null && !CheckIfActivated(controller) && held) {
            held = false;
            player.ReleaseSpellCast();
        }

        //controller.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressed);
        //if (pressed) player.CastFireball(0, 0f);
    }
}
