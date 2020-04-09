using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public NewNetworkManager networkManager;
    public PlayerCamera cam;

    private PlayerBehaviour p1, p2;

    [SyncVar]
    public float timer;

    [SyncVar]
    public bool roundStarted = false;

    [SyncVar]
    public bool roundFinished = false;

    public GameObject spawn1, spawn2;
    public GameObject menu;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        roundStarted = false;
        timer = 10f;
        StartCoroutine(KeepTimer());
    }

    [Server]
    IEnumerator KeepTimer() {
        while (true) {
            if (roundStarted && timer > 0f) {
                timer -= 0.1f;
            }
            if (roundStarted && timer <= 0f) {
                roundFinished = true;
            }
            yield return new WaitForSeconds(0.1f);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //print(timer);
        if (networkManager.bothPlayersConnected && !roundStarted && !roundFinished) {
            roundStarted = true;

            // These gets will be called once per round, this is harmless
            p1 = networkManager.player1.GetComponent<PlayerBehaviour>();
            p2 = networkManager.player2.GetComponent<PlayerBehaviour>();
        }

        if (roundStarted && menu != null && menu.activeSelf){
            menu.SetActive(false);
        }

        if (roundFinished && roundStarted) {
            EndRound();
            roundStarted = false;
        }
    }

    [Server]
    public void ResetMatch() {
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

        p1.TargetResetPosition(p1.GetComponent<NetworkIdentity>().connectionToClient, spawn1.transform.position);
        p2.TargetResetPosition(p2.GetComponent<NetworkIdentity>().connectionToClient, spawn2.transform.position);

        p1.RpcResetUI();
        p2.RpcResetUI();

        timer = 60f;
        roundStarted = true;
    }

    [Server]
    public void EndRound() {
        p1.RpcDisableGlyphInput();
        p2.RpcDisableGlyphInput();

        float p1Distance = p1.DistanceToCenter();
        float p2Distance = p2.DistanceToCenter();
        Debug.Log("p1 distance: "+p1Distance+"     p2 distance: "+p2Distance);

        if (p1Distance < p2Distance){
            p1.RpcWinRound();
            p2.RpcLoseRound();
        } else {
            p2.RpcWinRound();
            p1.RpcLoseRound();
        }
    }
}
