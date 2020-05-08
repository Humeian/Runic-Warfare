using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementProvider : LocomotionProvider
{
    public List<XRController> controllers;
    private CharacterController characterController;
    private GameObject head;
    public GameObject drawPanel;

    public GlyphRecognition glyphRecognition;

    public float speed = 1.0f;
    public float gravityMultiplier = 1.0f;

    private bool playerFound = false;
    private bool playerParented = false;

    public GameObject player;
    public GameObject drawingAnchor;

    protected override void Awake()
    {
        characterController = GetComponent<CharacterController>();
        head = GetComponent<XRRig>().cameraGameObject;
    }

    private void Start()
    {
        PositionController();
    }

    private void Update()
    {
        PositionController();
        CheckForInput();
        //ApplyGravity();

        if(!playerFound){
            try {
                player = GameObject.Find("TestPlayer(Clone)");
                if (player != null) {
                    playerFound = true;
                    //Debug.Log("PLAYER SET:    "+player);
                }
            } catch {
                Debug.Log("Cannot find player to select");
            }
        } else {
            if (!playerParented) {
                transform.parent = player.transform;
                //drawPanel.transform.parent = player.transform;

                transform.position = player.transform.position + new Vector3(-4.0f, 3.0f, 0.2f);
                //drawPanel.transform.position = player.transform.position + new Vector3(1.0f, 0f, 0f);
                glyphRecognition.player = player.GetComponent<PlayerBehaviour>();

                playerParented = true;
                //Debug.Log("PLAYER PARENTED:  "+player.name);
            }
        }

        // if ( Vector3.Distance(drawingAnchor.transform.position, transform.position) > 0.5f ) {
        //     drawingAnchor.transform.position = Vector3.MoveTowards(drawingAnchor.transform.position, transform.position, 0.1f);
        // } 


    }

    private void PositionController()
    {
        // Get the head in local, playspace ground
        float headHeight = Mathf.Clamp(head.transform.localPosition.y, 1,2);
        characterController.height = headHeight;

        // Cut in half, add skin
        Vector3 newCenter = Vector3.zero;
        newCenter.y = characterController.height / 2;
        newCenter.y += characterController.skinWidth;

        // Let's move the capsule in local space as well
        newCenter.x = head.transform.localPosition.x;
        newCenter.z = head.transform.localPosition.z;

        // Apply
        characterController.center = newCenter;
    }

    private void CheckForInput()
    {
        foreach(XRController controller in controllers) {
            if(controller.enableInputActions) {
                CheckForMovement(controller.inputDevice);
            }
        }
    }

    private void CheckForMovement(InputDevice device)
    {
        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 position)) {
            StartMove(position);
        }
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool goDown);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool goUp);

        if (goDown){
            characterController.Move(Vector3.down * Time.deltaTime);
        }
        if (goUp){
            characterController.Move(Vector3.up * Time.deltaTime);
        }
    }

    private void StartMove(Vector2 position)
    {
        // Apply the touch position to the head's forward Vector
        Vector3 direction = new Vector3(position.x, 0, position.y);
        Vector3 headRotation = new Vector3(0, head.transform.eulerAngles.y, 0);

        // Rotate the input direction by the horizontal head rotation
        direction = Quaternion.Euler(headRotation) * direction;

        // Apply speed and move
        Vector3 movement = direction * speed;
        characterController.Move(movement * Time.deltaTime);
        
        // if (drawPanel) {
        //     drawPanel.transform.position += movement * Time.deltaTime;
        // }
    }

    // private void ApplyGravity()
    // {
    //     Vector3 gravity = new Vector3(0, Physics.gravity.y * gravityMultiplier, 0);
    //     gravity.y *= Time.deltaTime;

    //     characterController.Move(gravity * Time.deltaTime);
    // }
}