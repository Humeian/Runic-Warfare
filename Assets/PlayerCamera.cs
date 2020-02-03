using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCamera : NetworkBehaviour
{

    public GameObject otherPlayer;
    public GameObject currentPlayer;
    public float playerHeight;

    private GlyphRecognition glyphRecognition;

    // Start is called before the first frame update
    void Start()
    {
        //currentPlayer = GameObject.Find("Player1");
        //otherPlayer = GameObject.Find("Player2");

        glyphRecognition = GameObject.FindWithTag("GlyphRecognition").GetComponent<GlyphRecognition>();
    }

    // Update is called once per frame
    void Update()
    {
		if (!currentPlayer || !otherPlayer) {
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			for (int i = 0; i < players.Length; i++) {
				PlayerBehaviour behaviour = players[i].GetComponent<PlayerBehaviour>();
				if (behaviour.isLocalPlayer) {
					currentPlayer = players[i];
				}
                else {
                    otherPlayer = players[i];
                }
			}
		} 
        else {
            transform.position = currentPlayer.transform.position + new Vector3(0f, playerHeight, 0f);
            transform.LookAt(otherPlayer.transform);
        }
        

        /*
        if (Input.GetKeyDown("t")){
            toggleViewPoint();
        }
        */
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
