using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    private GameObject otherPlayer;
    private GameObject currentPlayer;
    public float playerHeight;

    // Start is called before the first frame update
    void Start()
    {
        currentPlayer = GameObject.Find("Player1");
        otherPlayer = GameObject.Find("Player2");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = currentPlayer.transform.position + new Vector3(0f, playerHeight, 0f);
        transform.LookAt(otherPlayer.transform);
    }
}
