using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public NewNetworkManager networkManager;
    public PlayerCamera cam;

    [SyncVar]
    public float timer;

    [SyncVar]
    public bool roundStarted = false;

    public GameObject spawn1, spawn2;
    public GameObject menu;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        roundStarted = false;
        timer = 60f;
        StartCoroutine(KeepTimer());
    }

    [Server]
    IEnumerator KeepTimer() {
        while (true) {
            if (roundStarted && timer > 0f) {
                timer -= 0.1f;
            }
            yield return new WaitForSeconds(0.1f);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //print(timer);
        if (networkManager.bothPlayersConnected) {
            roundStarted = true;
        }

        if (roundStarted && menu != null && menu.activeSelf){
            menu.SetActive(false);
        }
    }

    [Server]
    public void ResetMatch() {
        PlayerBehaviour p1 = networkManager.player1.GetComponent<PlayerBehaviour>();
        PlayerBehaviour p2 = networkManager.player2.GetComponent<PlayerBehaviour>();
        p1.health = 3;
        p2.health = 3;
        
        p1.lightningCharge = 0;
        p2.lightningCharge = 0;

        GameObject[] shields = GameObject.FindGameObjectsWithTag("Shield");
        foreach (GameObject s in shields) {
            NetworkServer.Destroy(s);
        }
        
        GameObject[] iceSpikes = GameObject.FindGameObjectsWithTag("IceSpikes");
        foreach(GameObject i in iceSpikes) {
            NetworkServer.Destroy(i);
        }
        
        GameObject[] royalFires = GameObject.FindGameObjectsWithTag("RoyalFire");
        foreach (GameObject r in royalFires) {
            NetworkServer.Destroy(r);
        }

        p1.TargetResetPosition(p1.GetComponent<NetworkIdentity>().connectionToClient, spawn1.transform.position);
        p2.TargetResetPosition(p2.GetComponent<NetworkIdentity>().connectionToClient, spawn2.transform.position);

        p1.RpcResetUI();
        p2.RpcResetUI();

        timer = 60f;
        roundStarted = true;
    }
}
