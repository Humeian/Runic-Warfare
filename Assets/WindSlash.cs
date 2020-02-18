using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSlash : MonoBehaviour
{
    public int damage = 1;
    Collider otherPlayer;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(GetComponent<BoxCollider>(), 0.5f);
        Destroy(gameObject, 1f);

        otherPlayer = GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>().otherPlayer.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        if (other == otherPlayer) {
            //other.GetComponent<PlayerBehaviour>().TakeDamage(damage);
            //GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>().currentPlayer.GetComponent<PlayerBehaviour>().ThrowPlayerBack(12, 2, 0.5f);
            Destroy(gameObject);
        } else if (other.tag == "Shield") {
            other.GetComponent<Shield>().Break();
            //GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>().currentPlayer.GetComponent<PlayerBehaviour>().ThrowPlayerBack(12, 2, 0.5f);
            Destroy(gameObject);
        }
    }
}
