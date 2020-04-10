using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using AdVd.GlyphRecognition;

public class GameManager : NetworkBehaviour
{
    public NewNetworkManager networkManager;
    public GlyphRecognition glyphRecognizer;
    public PlayerCamera cam;

    private CharacterBehaviour p1, p2;

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

    public Glyph fireball;
    public Glyph shield;

    public GameObject introduction;
    public GameObject shootFireball;
    public GameObject blockFireball;
    public GameObject storeSpell;
    public GameObject retrieveSpell;
    public GameObject UIGameRules;
    public GameObject tracingPanel;
    public GameObject conclusion;

    public GameObject tutorialPanel;

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
            p1 = networkManager.player1.GetComponent<CharacterBehaviour>();
            p2 = networkManager.player2.GetComponent<CharacterBehaviour>();
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

        p1.TargetResetPosition(p1.connectionToClient, spawn1.transform.position);
        p2.TargetResetPosition(p2.connectionToClient, spawn2.transform.position);

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
        PlayerBehaviour p1PlayerBehaviour = networkManager.player1.GetComponent<PlayerBehaviour>();
        PlayerBehaviour p2PlayerBehaviour = networkManager.player2.GetComponent<PlayerBehaviour>();
        p1PlayerBehaviour.RpcDisableGlyphInput();
        if(p2PlayerBehaviour != null)
            p2PlayerBehaviour.RpcDisableGlyphInput();

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
            p1PlayerBehaviour.TargetWinRound(p1.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, round);
            if (p2PlayerBehaviour != null)
                p2PlayerBehaviour.TargetLoseRound(p2.GetComponent<NetworkIdentity>().connectionToClient, p2Wins, round);
        } else {
            p2Wins += 1;
            p1PlayerBehaviour.TargetWinRound(p2.GetComponent<NetworkIdentity>().connectionToClient, p2Wins, round);
            if (p2PlayerBehaviour != null)
                p2PlayerBehaviour.TargetLoseRound(p1.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, round);
        }


    }

    public IEnumerator Tutorial()
    {
        yield return new WaitForSecondsRealtime(2);

        Time.timeScale = 0;

        introduction.SetActive(true);

        while(!Input.GetMouseButtonDown(0)) { yield return new WaitForSecondsRealtime(0.1f); }

        introduction.SetActive(false);

        Time.timeScale = 1;



        shootFireball.SetActive(true);

        GameObject.Find("Glyph Display").GetComponent<GlyphDisplay>().glyph = fireball;

        while (glyphRecognizer.lastCast == null || glyphRecognizer.lastCast.target.ToString() != "Fireball4") { yield return new WaitForSecondsRealtime(0.1f); }

        while (networkManager.player2.GetComponent<CharacterBehaviour>().health == 3) { yield return new WaitForSecondsRealtime(0.1f); }

        shootFireball.SetActive(false);


        networkManager.player2.GetComponent<AIBehaviour>().CastFireball(0, 0f);

        yield return new WaitForSecondsRealtime(1);

        Time.timeScale = 0;

        blockFireball.SetActive(true);

        GameObject.Find("Glyph Display").GetComponent<GlyphDisplay>().glyph = shield;

        while (glyphRecognizer.lastCast == null || glyphRecognizer.lastCast.target.ToString() != "Shield2") { yield return new WaitForSecondsRealtime(0.1f); }

        glyphRecognizer.lastCast = null;

        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(3);

        blockFireball.SetActive(false);


        storeSpell.SetActive(true);

        GameObject.Find("Glyph Display").GetComponent<GlyphDisplay>().glyph = shield;

        while (glyphRecognizer.storedGlyph.Length <= 1 || (glyphRecognizer.Match(glyphRecognizer.storedGlyph) != null && glyphRecognizer.Match(glyphRecognizer.storedGlyph).target.ToString() != "Shield2")) { yield return new WaitForSecondsRealtime(0.1f); }

        storeSpell.SetActive(false);


        networkManager.player2.GetComponent<AIBehaviour>().CastFireball(0, 0f);

        yield return new WaitForSecondsRealtime(1);

        Time.timeScale = 0;

        retrieveSpell.SetActive(true);

        while (glyphRecognizer.lastCast == null || glyphRecognizer.lastCast.target.ToString() != "Shield2") { yield return new WaitForSecondsRealtime(0.1f); }

        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(3);

        Time.timeScale = 0;

        retrieveSpell.SetActive(false);

        tracingPanel.SetActive(true);

        tutorialPanel.SetActive(true);

        while (!Input.GetMouseButtonDown(0)) { yield return new WaitForSecondsRealtime(0.1f); }

        tracingPanel.SetActive(false);

        tutorialPanel.SetActive(false);

        yield return new WaitForSecondsRealtime(0.2f);

        UIGameRules.SetActive(true);

        while (!Input.GetMouseButtonDown(0)) { yield return new WaitForSecondsRealtime(0.1f); }

        UIGameRules.SetActive(false);

        yield return new WaitForSecondsRealtime(0.2f);

        conclusion.SetActive(true);

        while (!Input.GetMouseButtonDown(0)) { yield return new WaitForSecondsRealtime(0.1f); }

        conclusion.SetActive(false);

        networkManager.player2.GetComponent<AIBehaviour>().AIAttacks = true;

        Time.timeScale = 1;
    }
}
