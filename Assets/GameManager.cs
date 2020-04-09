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

    [SyncVar]
    public int p1Wins = 0;
    [SyncVar]
    public int p2Wins = 0;
    [SyncVar]
    public int round = 1; 

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

        if (roundStarted && !roundFinished){
            if (p1.health <= 0 || p2.health <= 0) {
                roundStarted = false;
                roundFinished = true;
                EndRound(0);
            }
        }

        if (roundFinished && roundStarted) {
            EndRound(1);
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
        roundFinished = false;
    }

    // Ending the round with a reason of 0 means a player has died
    //                         reason of 1 means a timeout has occured
    [Server]
    public void EndRound(int reason) {
        p1.RpcDisableGlyphInput();
        p2.RpcDisableGlyphInput();

        round += 1;

        bool p1win;

        if (reason == 1){
            float p1Distance = p1.DistanceToCenter();
            float p2Distance = p2.DistanceToCenter();
            Debug.Log("p1 distance: "+p1Distance+"     p2 distance: "+p2Distance);
            p1win = p1Distance < p2Distance;
        } else {
            p1win = p2.health <= 0;
        }
        
        if (p1win){
            p1Wins += 1;
            p1.TargetWinRound(p1.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, round);
            p2.TargetLoseRound(p2.GetComponent<NetworkIdentity>().connectionToClient, p2Wins, round);
        } else {
            p2Wins += 1;
            p2.TargetWinRound(p2.GetComponent<NetworkIdentity>().connectionToClient, p2Wins, round);
            p1.TargetLoseRound(p1.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, round);
        }


    }
}
