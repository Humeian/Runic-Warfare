using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerLocomotionController : MonoBehaviour
{

    public XRController leftInteractorRay;
    public XRController rightInteractorRay;
    public InputHelpers.Button teleportActivationButton;
    public float activationThreshold = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool CheckIfActivated(XRController controller){
        InputHelpers.IsPressed(controller.inputDevice, teleportActivationButton, out bool isActivated, activationThreshold);
        return isActivated;
    }
    // Update is called once per frame
    void Update()
    {
        if (leftInteractorRay) {
            leftInteractorRay.gameObject.SetActive(CheckIfActivated(leftInteractorRay));
        }

        if (rightInteractorRay) {
            rightInteractorRay.gameObject.SetActive(CheckIfActivated(rightInteractorRay));
        }
    }
}
