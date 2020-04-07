using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerCamera : MonoBehaviour
{
    public GameManager manager;

    public GameObject otherPlayer;
    public GameObject currentPlayer;
    public float playerHeight;
    public GameObject lookAtObject;
    public Transform playerHipBone;
    public Vector3 playerHipBonePos;

    public GlyphRecognition glyphRecognition;
    public PostProcessVolume ppv;
    private DepthOfField dof;

    public GameObject tutorialPanel;
    public GameObject gameSettings;

    private float shakeFactor = 0f;
    private float shakeDecayFactor = 0.9f;

    public float thirdPersonXOffset;
    public float thirdPersonYOffset;
    public float thirdPersonZOffset;

    public float mouseOffsetX;
    public float mouseOffsetY;
    
    private Vector2 currentMouseOffset;

    // Start is called before the first frame update
    void Start()
    {
        glyphRecognition = GameObject.FindWithTag("GlyphRecognition").GetComponent<GlyphRecognition>();
        ppv.profile.TryGetSettings(out dof);
        currentMouseOffset = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float mousePosX = (Input.mousePosition.x - (Screen.width / 2)) / (Screen.width / 2);
        float mousePosY = (Input.mousePosition.y - (Screen.height / 2)) / (Screen.height / 2);
        currentMouseOffset = new Vector2(Mathf.Lerp(currentMouseOffset.x, mousePosX, 0.05f), Mathf.Lerp(currentMouseOffset.y, mousePosY, 0.05f));

        if (!currentPlayer || !otherPlayer) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players) {
                if (p.GetComponent<PlayerBehaviour>().isLocalPlayer) {
                    currentPlayer = p;
                    glyphRecognition.player = p.GetComponent<PlayerBehaviour>();
                }
                else {
                    otherPlayer = p;
                }
            }
        }
        else {
            // Get body position for deathcam.
            if (playerHipBone == null) {
                playerHipBone = currentPlayer.transform.GetChild(0).GetChild(0).GetChild(0);
                playerHipBonePos = currentPlayer.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().position;
            }

            playerHipBonePos = playerHipBone.position;

            if (currentPlayer.GetComponent<PlayerBehaviour>().health <= 0) {
                transform.position = playerHipBone.position + new Vector3(4f, 4f, 0f);
                transform.LookAt(playerHipBone);

                //adjust depth of view focus distance
                dof.focusDistance.value = Vector3.Distance(transform.position, playerHipBone.position);
            }
            else {
                transform.position = currentPlayer.transform.position + new Vector3(0f, playerHeight, 0f) 
                + (currentPlayer.transform.right * thirdPersonXOffset) 
                + (currentPlayer.transform.up * thirdPersonYOffset)
                + (currentPlayer.transform.forward * thirdPersonZOffset);
                lookAtObject.transform.position = otherPlayer.transform.position + new Vector3(0f, playerHeight * 0.8f, 0f);
                transform.LookAt(lookAtObject.transform);

                //adjust depth of view focus distance
                dof.focusDistance.value = Vector3.Distance(transform.position, lookAtObject.transform.position);

                //rotate camera slightly towards mouse position
                transform.rotation *= Quaternion.Euler(currentMouseOffset.y * -0.5f, currentMouseOffset.x * 0.5f, 0f);
            }

            //if object is close, switch to narrow depth of field
            if (dof.focusDistance.value < 16f) {
                dof.focalLength.value = Mathf.Lerp(dof.focalLength, 100f, 0.05f);
            }
            else {
                dof.focalLength.value = Mathf.Lerp(dof.focalLength, 50f, 0.05f);
            }

            if (shakeFactor >= 0.001f) {
                transform.position += (Vector3)(Random.insideUnitSphere * shakeFactor);
            }
            shakeFactor *= shakeDecayFactor;
        }

        if (Input.GetKeyDown("t")){
            showTutorial();
        }
        if (Input.GetKeyDown("s")){
            showSettings();
        }
    }

    void showTutorial(){
        if (tutorialPanel != null){
            tutorialPanel.SetActive(!tutorialPanel.active);
        } else {
            Debug.Log("Tutorial panel cannot be found");
        }
    }

    void showSettings(){
        if (gameSettings != null){
            gameSettings.SetActive(!gameSettings.active);
        } else {
            Debug.Log("Settings panel cannot be found");
        }
    }

    public void Shake(float s) {
        shakeFactor += s;
    }

    public void Rematch() {
        // Ask Player script to command server to reset server-side elements
        currentPlayer.GetComponent<PlayerBehaviour>().CmdResetMatch();
    }
}
