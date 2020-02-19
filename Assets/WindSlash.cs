﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WindSlash : NetworkBehaviour
{
    public int damage = 1;

    [SyncVar]
    public GameObject otherPlayer;

    [SyncVar]
    public GameObject owner;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        Destroy(GetComponent<BoxCollider>(), 0.6f);
        Destroy(gameObject, 1f);

        //otherPlayer = GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>().otherPlayer.GetComponent<Collider>();
    }

    public void SetTarget(GameObject g) {
        otherPlayer = g;
    }

    public void SetOwner(GameObject g) {
        owner = g;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (owner != null)
            transform.position = owner.transform.position + (transform.forward * 2f);
    }

    [Server]
    void OnTriggerStay(Collider other) {
        if (owner != null && otherPlayer != null) {
            if (other.gameObject == otherPlayer) {
                Debug.Log("testing");
                other.GetComponent<PlayerBehaviour>().TakeDamage(damage);
                other.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
                owner.GetComponent<PlayerBehaviour>().TargetThrowPlayerBack(owner.GetComponent<NetworkIdentity>().connectionToClient, 0.8f, 2, 40);
                Destroy(gameObject);
            } else if (other.tag == "Shield") {
                other.GetComponent<Shield>().Break();
                owner.GetComponent<PlayerBehaviour>().TargetThrowPlayerBack(owner.GetComponent<NetworkIdentity>().connectionToClient, 0.4f, 2, 40);
                Destroy(gameObject);
            }
        }
        
    }
}
