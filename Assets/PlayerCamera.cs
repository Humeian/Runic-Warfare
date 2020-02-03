using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    public GameObject otherPlayer;
    public GameObject currentPlayer;
    public float playerHeight;

    private GlyphRecognition glyphRecognition;

    // Start is called before the first frame update
    void Start()
    {
        currentPlayer = GameObject.Find("Player1");
        otherPlayer = GameObject.Find("Player2");

        glyphRecognition = GameObject.FindWithTag("GlyphRecognition").GetComponent<GlyphRecognition>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = currentPlayer.transform.position + new Vector3(0f, playerHeight, 0f);
        transform.LookAt(otherPlayer.transform);

        if (Input.GetKeyDown("t")){
            toggleViewPoint();
        }
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
