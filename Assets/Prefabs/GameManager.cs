using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject topPanel, bottomPanel;

    public Glyph fireball;
    public Glyph shield;

    public GameObject rematchButton;

    public GameObject introduction;
    public GameObject shootFireball;
    public GameObject blockFireball;
    public GameObject storeSpell;
    public GameObject retrieveSpell;
    public GameObject UIGameRules;
    public GameObject tracingPanel;
    public GameObject conclusion;

    public GameObject tutorialPanel;

    public bool isTutorial = false;

    public string difficulty = "Hard";
    public Text difficultyText;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        roundStarted = false;
        timer = 63f;
        StartCoroutine(KeepTimer());
    }

    [Server]
    IEnumerator KeepTimer() {
        while (true) {
            if (roundStarted && timer > 0f) {
                timer -= Time.deltaTime;
            }
            if (roundStarted && timer <= 0f) {
                roundFinished = true;
            }
            yield return new WaitForEndOfFrame();
        }

    }

    public void toggleAIDifficulty () {
        switch (difficulty) {
            case "Hard":
                difficulty = "Medium";
                break;
            case "Medium":
                difficulty = "Easy";
                break;
            case "Easy":
                difficulty = "Expert";
                break;
            case "Expert":
                difficulty = "Hard";
                break;
        }
        difficultyText.text = difficulty;

        try {
            AIBehaviour ai = GameObject.Find("FahrGrimm(Clone)").GetComponent<AIBehaviour>();
            ai.ToggleDifficulty(difficulty);
        } catch {
            Debug.Log("No AI found");
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
            

            //Debug.Log("__________________________________________-------------------------------------------------------------- HERE");
        }

        if (roundStarted && rematchButton != null && rematchButton.activeInHierarchy) {
            rematchButton.SetActive(false);
        }

        if (roundStarted && menu != null && menu.activeInHierarchy){
            menu.SetActive(false);
            // topPanel.SetActive(true);
            // GameObject.Find("HP1").GetComponent<UnityEngine.UI.Image>().color = Color.white;
            // GameObject.Find("HP2").GetComponent<UnityEngine.UI.Image>().color = Color.white;
            // GameObject.Find("HP3").GetComponent<UnityEngine.UI.Image>().color = Color.white;
        }

        if (roundStarted && !roundFinished){
            //Debug.Log("P1:  "+ p1 + "   P2:  "+p2);
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

    public void SetPlayerSpellVelocity(float velocity) {
        GameObject.Find("TestPlayer(Clone)").GetComponent<PlayerBehaviour>().spellVelocity = velocity;
        Debug.Log("Spell Velocity: "+velocity);
    }

    [Server]
    public void ResetMatch() {
        p1.health = 4;
        p2.health = 4;
        
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

        //p1.TargetResetPosition(p1.connectionToClient, spawn1.transform.position);
        if (p1.GetComponent<AIBehaviour>() != null)
            p1.GetComponent<AIBehaviour>().ResetPosition(spawn1.transform.position);
        else {
            p1.TargetResetPosition(p1.connectionToClient, spawn1.transform.position);
        }

        if (p2.GetComponent<AIBehaviour>() != null)
            p2.GetComponent<AIBehaviour>().ResetPosition(spawn2.transform.position);
        else {
            p2.TargetResetPosition(p2.connectionToClient, spawn2.transform.position);
        }
        

        p1.RpcResetUI();
        p2.RpcResetUI();

        timer = 63f;

        if (round == 1) {
            p1Wins = 0;
            p2Wins = 0;
        }

        roundStarted = true;
        roundFinished = false;
    }

    [Server]
    public void WipeAI () {

    }

    // Ending the round with a reason of 0 means a player has died
    //                         reason of 1 means a timeout has occured
    [Server]
    public void EndRound(int reason) {
        PlayerBehaviour p1PlayerBehaviour = networkManager.player1.GetComponent<PlayerBehaviour>();
        PlayerBehaviour p2PlayerBehaviour = null;
        try {
            p2PlayerBehaviour = networkManager.player2.GetComponent<PlayerBehaviour>();
        } catch {
            Debug.Log("Round end against AI / No Player2 Found");
        }

        if (p2.GetComponent<AIBehaviour>() != null)
            p2.GetComponent<AIBehaviour>().disableAttacking();
        
        // p1PlayerBehaviour.RpcDisableGlyphInput();
        // if(p2PlayerBehaviour != null)
        //     p2PlayerBehaviour.RpcDisableGlyphInput();

        bool p1win;

        if (reason == 1){
            float p1Distance = p1.DistanceToCenter();
            float p2Distance = p2.DistanceToCenter();
            Debug.Log("p1 distance: "+p1Distance+"     p2 distance: "+p2Distance);
            p1win = p1Distance < p2Distance;
        } else {
            p1win = p2.health <= 0;
        }
        
        //If either p1 or p2 win three rounds, reset round number to 1.
        if (isTutorial)
        {
            p1PlayerBehaviour.TargetEndTutorial(p1.GetComponent<NetworkIdentity>().connectionToClient);
        } else
        {
            if (p1win)
            {
                p1Wins += 1;
                if (p1Wins >= 3)
                {
                    round = 1;
                }
                else
                {
                    round++;
                }
                p1PlayerBehaviour.TargetWinRound(p1.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, p2Wins, round);
                if (p2PlayerBehaviour != null)
                    p2PlayerBehaviour.TargetLoseRound(p2.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, p2Wins, round);
            }
            else
            {
                p2Wins += 1;
                if (p2Wins >= 3)
                {
                    round = 1;
                }
                else
                {
                    round++;
                }
                if (p2PlayerBehaviour != null)
                    p2PlayerBehaviour.TargetWinRound(p2.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, p2Wins, round);
                p1PlayerBehaviour.TargetWinRound(p1.GetComponent<NetworkIdentity>().connectionToClient, p1Wins, p2Wins, round);
            }
            print("p1Win " + p1Wins + ", p2Win " + p2Wins);
        }
    }

    public IEnumerator Tutorial()
    {
        GlyphDisplay glyphDisplay = GameObject.Find("Glyph Display").GetComponent<GlyphDisplay>();
        yield return new WaitForSecondsRealtime(2);

        Time.timeScale = 0;

        introduction.SetActive(true);

        while(!Input.GetMouseButtonDown(0)) { yield return null; }

        introduction.SetActive(false);

        Time.timeScale = 1;


        yield return new WaitForSecondsRealtime(1);

        shootFireball.SetActive(true);

        glyphDisplay.glyph = fireball;

        Time.timeScale = 0;

        while (glyphRecognizer.lastCast == null || glyphRecognizer.lastCast.target.ToString() != "Fireball4") {
            glyphDisplay.RebuildGlyph();
            yield return new WaitForSecondsRealtime(0.1f);
        }

        Time.timeScale = 1;

        while (networkManager.player2.GetComponent<CharacterBehaviour>().health == 3) { yield return new WaitForSecondsRealtime(0.1f); }

        shootFireball.SetActive(false);

        yield return new WaitForSecondsRealtime(2);


        networkManager.player2.GetComponent<AIBehaviour>().CastFireball(0, 0f);

        yield return new WaitForSecondsRealtime(1);

        Time.timeScale = 0;

        blockFireball.SetActive(true);

        glyphDisplay.glyph = shield;

        while (glyphRecognizer.lastCast == null || glyphRecognizer.lastCast.target.ToString() != "Shield2") {
            glyphDisplay.RebuildGlyph();
            yield return new WaitForSecondsRealtime(0.1f);
        }

        glyphRecognizer.lastCast = null;

        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(3);

        blockFireball.SetActive(false);


        storeSpell.SetActive(true);

        glyphDisplay.glyph = shield;

        Time.timeScale = 0;

        while (glyphRecognizer.storedGlyph.Length <= 1 || (glyphRecognizer.Match(glyphRecognizer.storedGlyph) != null && glyphRecognizer.Match(glyphRecognizer.storedGlyph).target.ToString() != "Shield2")) {
            glyphDisplay.RebuildGlyph();
            yield return new WaitForSecondsRealtime(0.1f);
        }

        GameObject.Find("Glyph Display").GetComponent<GlyphDisplay>().glyph = null;

        Time.timeScale = 1;

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

        while (!Input.GetMouseButtonDown(0)) { yield return null; }

        tracingPanel.SetActive(false);

        tutorialPanel.SetActive(false);

        yield return new WaitForSecondsRealtime(0.2f);

        UIGameRules.SetActive(true);

        while (!Input.GetMouseButtonDown(0)) { yield return null; }

        UIGameRules.SetActive(false);

        yield return new WaitForSecondsRealtime(0.2f);

        conclusion.SetActive(true);

        while(!Input.GetMouseButtonDown(0)) { yield return null; }

        conclusion.SetActive(false);

        networkManager.player2.GetComponent<AIBehaviour>().AIAttacks = true;

        Time.timeScale = 1;
    }
}
