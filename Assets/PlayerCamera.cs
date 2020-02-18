using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    public GameObject otherPlayer;
    public GameObject currentPlayer;
    public float playerHeight;

    public GlyphRecognition glyphRecognition;

    private float shakeFactor = 0f;
    private float shakeDecayFactor = 0.9f;

    // Start is called before the first frame update
    void Start()
    {

        glyphRecognition = GameObject.FindWithTag("GlyphRecognition").GetComponent<GlyphRecognition>();
    }

    // Update is called once per frame
    void Update()
    {
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
            transform.position = currentPlayer.transform.position + new Vector3(0f, playerHeight, 0f);
            transform.LookAt(otherPlayer.transform);

            if (shakeFactor >= 0.001f) {
                transform.position += (Vector3)(Random.insideUnitSphere * shakeFactor);
            }
            shakeFactor *= shakeDecayFactor;
        }

        if (Input.GetKeyDown("t")){
            toggleViewPoint();
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
}
