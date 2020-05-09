using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class LeftHandManager : MonoBehaviour
{
    public XRController controller;
    private InputDevice inputDevice;
    public XRDirectInteractor interactor;
    public List<InputHelpers.Button> startMoveButtons;
    public InputHelpers.Button grip;  
    public float activationThreshold = 0.2f;

    public PlayerBehaviour player;
    public bool held = false;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<XRController>();
        inputDevice = controller.inputDevice;
        interactor = GetComponent<XRDirectInteractor>();
    }

    public bool CheckIfActivated(XRController controller){
        InputHelpers.IsPressed(controller.inputDevice, grip, out bool isGripped, activationThreshold);
        return (isGripped);
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

        if (CheckIfActivated(controller) && !held){
            held = true;
            if (interactor.selectTarget == null) {
                player.StartGripMove(transform.position);
            }
        }

        if (!CheckIfActivated(controller) && held) {
            held = false;
            if (interactor.selectTarget == null) {
                player.ReleaseGripMove(transform.position);
            }
        }
    }
}
