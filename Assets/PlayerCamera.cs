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

    private float shakeFactor = 0f;
    private float shakeDecayFactor = 0.9f;

    public float thirdPersonXOffset;
    public float thirdPersonYOffset;
    public float thirdPersonZOffset;

    public float mouseOffsetX;
    public float mouseOffsetY;
    
    private Vector2 currentMouseOffset;

    private bool isWaiting = true;
    private bool isInIntro = false;
    private bool introCompleted = false;

    public enum CameraState {PreGame, Intro, InGame, MyPlayerDead, OtherPlayerDead};
    public CameraState cameraState;

    public AudioClip roundWin;

    // Start is called before the first frame update
    void Start()
    {
        glyphRecognition = GameObject.FindWithTag("GlyphRecognition").GetComponent<GlyphRecognition>();
        ppv.profile.TryGetSettings(out dof);
        currentMouseOffset = Vector2.zero;
        cameraState = CameraState.PreGame;
        StartCoroutine(Preview());
    }

    // Update is called once per frame
    void Update()
    {
        float mousePosX = (Input.mousePosition.x - (Screen.width / 2)) / (Screen.width / 2);
        float mousePosY = (Input.mousePosition.y - (Screen.height / 2)) / (Screen.height / 2);
        currentMouseOffset = new Vector2(Mathf.Lerp(currentMouseOffset.x, mousePosX, 0.05f), Mathf.Lerp(currentMouseOffset.y, mousePosY, 0.05f));

        if (cameraState == CameraState.PreGame) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players) {
                if (p.GetComponent<PlayerBehaviour>().isLocalPlayer) {
                    currentPlayer = p;
                    glyphRecognition.player = p.GetComponent<PlayerBehaviour>();
                }
                else {
                    otherPlayer = p;
                }

                if (currentPlayer && otherPlayer) {
                    cameraState = CameraState.Intro;
                    StartCoroutine(SpinRoutine());
                }
            }
        }
        else if (cameraState == CameraState.Intro) {
            //stay idle, let routine switch state.
        }
        else if (cameraState == CameraState.InGame) {
            transform.position = currentPlayer.transform.position + new Vector3(0f, playerHeight, 0f) 
            + (currentPlayer.transform.right * thirdPersonXOffset) 
            + (currentPlayer.transform.up * thirdPersonYOffset)
            + (currentPlayer.transform.forward * thirdPersonZOffset);
            lookAtObject.transform.position = otherPlayer.transform.position + new Vector3(0f, playerHeight * 0.8f, 0f);
            transform.LookAt(lookAtObject.transform);

            //adjust depth of view focus distance
            dof.focusDistance.value = Vector3.Distance(transform.position, lookAtObject.transform.position);

            if (currentPlayer.GetComponent<PlayerBehaviour>().health <= 0) {
                cameraState = CameraState.MyPlayerDead;
            }
            if (otherPlayer.GetComponent<PlayerBehaviour>().health <= 0) {
                cameraState = CameraState.OtherPlayerDead;
                GetComponent<AudioSource>().clip = roundWin;
                GetComponent<AudioSource>().Play();
            }
        }
        else if (cameraState == CameraState.MyPlayerDead) {
            playerHipBone = currentPlayer.transform.GetChild(0).GetChild(0).GetChild(0);
            playerHipBonePos = currentPlayer.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().position;

            playerHipBonePos = playerHipBone.position;
            transform.position = playerHipBone.position + new Vector3(4f, 4f, 0f);
            transform.LookAt(playerHipBone);
            
            //adjust depth of view focus distance
            dof.focusDistance.value = Vector3.Distance(transform.position, playerHipBone.position);

            if (currentPlayer.GetComponent<PlayerBehaviour>().health > 0) {
                cameraState = CameraState.Intro;
                StartCoroutine(SpinRoutine());
            }
        }
        else if (cameraState == CameraState.OtherPlayerDead) {
            playerHipBone = otherPlayer.transform.GetChild(0).GetChild(0).GetChild(0);
            playerHipBonePos = otherPlayer.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().position;

            playerHipBonePos = playerHipBone.position;
            transform.position = playerHipBone.position + new Vector3(4f, 4f, 0f);
            transform.LookAt(playerHipBone);
            
            //adjust depth of view focus distance
            dof.focusDistance.value = Vector3.Distance(transform.position, playerHipBone.position);

            if (otherPlayer.GetComponent<PlayerBehaviour>().health > 0) {
                cameraState = CameraState.Intro;
                StartCoroutine(SpinRoutine());
            }
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

        //rotate camera slightly towards mouse position
        transform.rotation *= Quaternion.Euler(currentMouseOffset.y * -0.5f, currentMouseOffset.x * 0.5f, 0f);
    }

    public IEnumerator SpinRoutine() {
        float duration = 3f;
        float timer = duration;
        Vector3 target = currentPlayer.transform.position + (Vector3.up * playerHeight)
            + (currentPlayer.transform.right * thirdPersonXOffset) 
            + (currentPlayer.transform.up * thirdPersonYOffset)
            + (currentPlayer.transform.forward * thirdPersonZOffset);
        Vector3 lookTarget = currentPlayer.transform.position + (Vector3.up * playerHeight * 0.8f);
        while (timer >= 0f) {
            transform.position = target - (currentPlayer.transform.forward * Mathf.Pow(timer, 3f) * 1f) + (Vector3.up * Mathf.Pow(timer, 3f) * 0.5f) + (currentPlayer.transform.right * Mathf.Pow(timer, 3f) * 1f);

            lookTarget = Vector3.Lerp(currentPlayer.transform.position, otherPlayer.transform.position + (Vector3.up * playerHeight * 0.8f), 1 - (Mathf.Pow(timer, 2f) / Mathf.Pow(duration, 2f)));
            print(Mathf.Pow(duration, 1.5f) - Mathf.Pow(timer, 1.5f));
            transform.LookAt(lookTarget);
            timer -= Time.deltaTime;

            dof.focusDistance.value = Vector3.Distance(transform.position, lookTarget);
            yield return new WaitForEndOfFrame();
        }
        cameraState = CameraState.InGame;
    }

    private IEnumerator Preview() {
        // rotate around this point
        transform.position = new Vector3(0f, 10f, -50f);
        Vector3 target = new Vector3(0f, 5f, 0f);
        while (cameraState == CameraState.PreGame) {
            transform.RotateAround(target, transform.up, -4f * Time.deltaTime);
            transform.LookAt(target);
            yield return new WaitForEndOfFrame();
        }
    }

    public void Shake(float s) {
        shakeFactor += s;
    }

    public void toggleViewPoint() {
        Debug.Log("Switch positions");
        if (currentPlayer.name == "Player1") {
            currentPlayer = GameObject.Find("Player2");
            otherPlayer = GameObject.Find("Player1");
        } else {
            currentPlayer = GameObject.Find("Player1");
            otherPlayer = GameObject.Find("Player2");
        }

        glyphRecognition.ChangePlayer(currentPlayer);
    }

    public void Rematch() {
        // Ask Player script to command server to reset server-side elements
        currentPlayer.GetComponent<PlayerBehaviour>().CmdResetMatch();
    }
}
