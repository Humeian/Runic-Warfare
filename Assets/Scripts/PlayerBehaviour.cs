﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerBehaviour : CharacterBehaviour
{

    public UnityEngine.UI.Image hp1, hp2, hp3;


    private int lightningChargesNeeded = 3;

    public NetworkAnimator animator;

    private float playerHeight = 1f;

    public Timer timer;

    public AudioClip[] clips;

    private bool onGround = true;

    private Color red;
    private Color white = new Color(1f, 1f, 1f, 1f);

    int movingRight = 0;
    int movingForward = 0;
    int movingUp = 0;
    float speedRight = 0f;
    float speedForward = 0f;
    float speedUp = 0f;

    bool comingDown = false;

    private bool firstHit = true;
    private int currentRound;

    private bool isAIChar;

    // After using Pulse, number of times spells casted in the air will stop air momentum.
    private int stopMomentumCharges = 0;

    public GameManager gameManager;

    // OnServerStart: called when GameObject is created on the server (not called on client).
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    // OnServerAuthority: called when GameObject is created on the client with authority.
    // By default, clients only have authority over their Player object and nothing else. 
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        StartCoroutine(Movement());
    }

    public void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        red = new Color(1f, 0f, 0f, 1f);
        shields = new List<GameObject>();
    }

    IEnumerator testmove() {
        while (true) {
            transform.position += transform.forward * Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    [Command]
    public void CmdResetMatch(){
        gameManager.ResetMatch();
    }

    [TargetRpc]
    public new void TargetResetPosition(NetworkConnection connection, Vector3 pos) {
        transform.position = pos;
    }

    [TargetRpc]
    public void TargetTakeRoyalBurn(NetworkConnection connection, float b) {
        royalBurn += b;
        if (royalBurn > 1f) {

            health -= 1;
            
            //This is basically TargetShowDamageEffects, but called client-side
            UnityEngine.UI.Image redscreen = GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject.GetComponent<UnityEngine.UI.Image>();
            redscreen.color = new Color(1f, 0f, 0f, 0.8f);
            Camera.main.GetComponent<PlayerCamera>().Shake(5f);
            Color red = new Color(1f, 0f, 0f, 1f);

            royalBurn = 0f;
        }
    }

    public void InitializeUI() {
        timer = GameObject.Find("Timer").GetComponent<Timer>();

        hp1 = GameObject.Find("HP1").GetComponent<UnityEngine.UI.Image>();
        hp2 = GameObject.Find("HP2").GetComponent<UnityEngine.UI.Image>();
        hp3 = GameObject.Find("HP3").GetComponent<UnityEngine.UI.Image>();
    }

    [ClientRpc]
    public override void RpcResetUI() {
        // Disable rematch button
        GameObject.Find("GameUI").transform.Find("ReadyPanel").gameObject.SetActive(false);

        // Wait for round to start before enabling input
        StartCoroutine(WaitForRoundStart());

        // Recolour the health bubbles
        GameObject.Find("HP1").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);
        GameObject.Find("HP2").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);
        GameObject.Find("HP3").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);

        // Disable Win/Loss text
        GameObject.Find("GameUI").transform.Find("WinPanel").gameObject.SetActive(false);
        GameObject.Find("GameUI").transform.Find("LossPanel").gameObject.SetActive(false);

        shields.Clear();
        lightningCharge = 0;

        //reset momentum
        movingForward = 0;
        movingRight = 0;
        movingUp = 0;

        if (gameManager.round == 1) {
            GameObject.Find("WinsText").GetComponent<UnityEngine.UI.Text>().text = 0 + " - " + 0;
        }
    }

    private IEnumerator WaitForRoundStart() {
        yield return new WaitForSeconds(3.0f);
        // Enable glyph input & reboot the color cleaning coroutine
        GameObject glyphInput = GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject;
        glyphInput.SetActive(true);
        glyphInput.GetComponent<GlyphRecognition>().InitCleanScreen();
    }

    [ClientRpc]
    public void RpcDisableGlyphInput(){
        GameObject.Find("Canvas").transform.Find("Basic Glyph Input").GetComponent<GlyphRecognition>().ClearAll();
        GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject.SetActive(false);
    }

    [TargetRpc]
    public void TargetWinRound(NetworkConnection connection, int p1Score, int p2Score, int round) {
        // Display Win text
        GameObject.Find("GameUI").transform.Find("WinPanel").gameObject.SetActive(true);

        if (p1Score >= 3) {
            GameObject.Find("GameUI").transform.Find("WinPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "Player 1 Wins!";
        }
        else if (p2Score >= 3) {
            GameObject.Find("GameUI").transform.Find("WinPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "Player 2 Wins!";
        }
        else {
            GameObject.Find("GameUI").transform.Find("WinPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = p1Score + " - " + p2Score;
        }
        
        // Display Rematch button
        GameObject.Find("GameUI").transform.Find("ReadyPanel").gameObject.SetActive(true);
        if (p1Score >= 3 || p2Score >= 3) {
            GameObject.Find("GameUI").transform.Find("ReadyPanel").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Rematch?";
        }
        else {
            GameObject.Find("GameUI").transform.Find("ReadyPanel").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Next Round";
        }

        //Update # of wins and round number
        GameObject.Find("WinsText").GetComponent<UnityEngine.UI.Text>().text = p1Score + " - " + p2Score;
        currentRound = round;
    }
    [TargetRpc]
    public void TargetLoseRound(NetworkConnection connection, int p1Score, int p2Score, int round) {
        GameObject.Find("GameUI").transform.Find("LossPanel").gameObject.SetActive(true);
        
        if (p1Score >= 3) {
            GameObject.Find("GameUI").transform.Find("LossPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "Player 1 Wins!";
        }
        else if (p2Score >= 3) {
            GameObject.Find("GameUI").transform.Find("LossPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "Player 2 Wins!";
        }
        else {
            GameObject.Find("GameUI").transform.Find("LossPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = p1Score + " - " + p2Score;
        }

        GameObject.Find("GameUI").transform.Find("ReadyPanel").gameObject.SetActive(true);
        if (p1Score >= 3 || p2Score >= 3) {
            GameObject.Find("GameUI").transform.Find("ReadyPanel").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Rematch?";
        }
        else {
            GameObject.Find("GameUI").transform.Find("ReadyPanel").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Next Round";
        }
        
        //Update # of wins and round number
        GameObject.Find("WinsText").GetComponent<UnityEngine.UI.Text>().text = p1Score + " - " + p2Score;
        currentRound = round;
    }

    [TargetRpc]
    public void TargetEndTutorial(NetworkConnection connection)
    {
        GameObject.Find("GameUI").transform.Find("BackToMenuPanel").gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        if (otherPlayer != null) {
            transform.LookAt(otherPlayer.transform);
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }

        if (health > 0 && GetComponent<Animator>().runtimeAnimatorController == null) {
            GetComponent<Animator>().runtimeAnimatorController = controller;
            //GetComponent<CapsuleCollider>().enabled = !GetComponent<CapsuleCollider>().enabled;
        }

        else if( health <= 0 && GetComponent<Animator>().runtimeAnimatorController != null){
            // set a global death flag to enter finished screen
            //Debug.Log(this.gameObject.name +" is dead");
            //GetComponent<CapsuleCollider>().enabled = !GetComponent<CapsuleCollider>().enabled;
            GetComponent<Animator>().runtimeAnimatorController = null;

            // Enable rematch button
            GameObject.Find("GameUI").transform.Find("ReadyPanel").gameObject.SetActive(true);

            // Disable glyph input
            //GameObject.Find("Canvas").transform.Find("Basic Glyph Input").GetComponent<GlyphRecognition>().ClearAll();
            //GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject.SetActive(false);

            // Stop the timer
            //timer.StopTimer();
        }

        if (Input.GetKey(KeyCode.H)) {
            CmdRestoreHealth(3);
        }

        // Needs to be in Update as there appear to be damage timing issues.
        if (hasAuthority) {
            if (health < 3) {
                hp1.color = red;
            } else {
                hp1.color = white;
            }
            if (health < 2) {
                hp2.color = red;
            } else {
                hp2.color = white;
            }
            if (health < 1) {
                hp3.color = red;
            } else {
                hp3.color = white;
            }
        }

        if (royalBurn > 0f) {
            royalBurn -= royalBurnRecovery * Time.deltaTime; 
        }
        else 
            royalBurn = 0f;
    }

    public new void RestoreHealth(int h)
    {
        CmdRestoreHealth(h);
    }
    [Command]
    public void CmdRestoreHealth(int h) {
        health = h;
    }

    IEnumerator Movement() {
        while (true) {
            float distanceFromCenter = DistanceToCenter();

            if (distanceFromCenter < 24){
                if (movingRight != 0) {
                    transform.position += transform.right * Time.deltaTime * (movingRight * speedRight);

                    if      (movingRight < 0) movingRight++;
                    else if (movingRight > 0) movingRight--;
                }

                if (movingForward != 0) {
                    transform.position += transform.forward * Time.deltaTime * (movingForward * speedForward);

                    if      (movingForward < 0) movingForward++;
                    else if (movingForward > 0) movingForward--;
                }
            } else {
                movingRight = 0;
                movingForward = 0;
                transform.position -= (transform.position - GameObject.Find("CenterMark").transform.position).normalized;
            }

            if (!onGround) {
                //fall faster if falling down and no spell has been used -- easier to time reflect pulse
                if (speedUp < 0 && stopMomentumCharges > 0) {
                    if (!comingDown && hasAuthority) {
                        CmdSetAnimTrigger("PulseDown");
                        comingDown = true;
                    }
                    transform.position += transform.up * speedUp * 250f * Time.deltaTime;
                }
                else {
                    transform.position += transform.up * speedUp * 60f * Time.deltaTime;
                }

                speedUp -= Time.deltaTime;
                
                if (transform.position.y <= 0f) {
                    transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
                    onGround = true;
                    if (stopMomentumCharges > 0) {
                        CmdCastArcanePulse();
                    }
                    stopMomentumCharges = 0;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    //Server: Only the server executes the function. 
    //(However, because the variable is synced, clients will also see the HP decrease.)
    [Server]
    public new void TakeDamage(int dmg) {
        health -= (dmg);
    }

    //TargetRpc: Effect will only appear on the targeted network client.
    [TargetRpc]
    public void TargetShowDamageEffects(NetworkConnection target) {
        UnityEngine.UI.Image redscreen = GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject.GetComponent<UnityEngine.UI.Image>();
        redscreen.color = new Color(1f, 0f, 0f, 0.8f);

        Camera.main.GetComponent<PlayerCamera>().Shake(5f);

        Color red = new Color(1f, 0f, 0f, 1f);
    }

    public void CastFireball(int horizontal, float horizSpeed) {
        StopAirMomentum();
        //transform.position += transform.TransformDirection(Vector3.right);
        movingRight = horizontal;
        speedRight = horizSpeed;
        movingForward = 25;
        speedForward = 0.4f;
        if (horizontal > 0f)
            CmdSetAnimTrigger("FireballRight");
        else 
            CmdSetAnimTrigger("FireballLeft");
        CmdCastFireball();
    }
    [TargetRpc]
    public void TargetPaintScreen(NetworkConnection target, Color c) {
        UnityEngine.UI.Image screen = GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject.GetComponent<UnityEngine.UI.Image>();
        screen.color = c;
    }

    [Command]
    public void CmdSetAnimTrigger(string s) {
        print("Calling animation for " + s);
        animator.SetTrigger(s);
    }

    [Command]
    public void CmdPlayClip(int a) {
        RpcPlayClip(a);
    }

    [ClientRpc]
    public void RpcPlayClip(int a) {
        GetComponent<AudioSource>().clip = clips[a];
        GetComponent<AudioSource>().Play();
    }

    // For outside animation triggers such as WindSlashRecoil.
    [TargetRpc]
    public override void TargetSetAnimTrigger(NetworkConnection target, string s) {
        CmdSetAnimTrigger(s);
    }

    public void CastWindForward() {
        StopAirMomentum();
        movingForward = 20;
        speedForward = 2f;
        CmdSetAnimTrigger("WindSlash");
        CmdCastWindForward();
    }

    public void CastShieldBack() {
        StopAirMomentum();
        movingForward = -30;
        speedForward = 0.4f;
        CmdSetAnimTrigger("ShieldBack");
        CmdCastShieldBack();
    }

    public void CastLightningNeutral() {
        CmdPlayClip(0);
        StopAirMomentum();
        animator.ResetTrigger("LightningCharged");
        animator.ResetTrigger("LightningShoot");
        CmdSetAnimTrigger("LightningCharging");
        CmdCastLightningCharge();
        lightningCharge++;
        if (lightningCharge == lightningChargesNeeded) {
            StartCoroutine(WaitForLightning());
            lightningCharge = 0;
        }
        else {
            StartCoroutine(WaitForLightningCharge());
        }
    }

    public void CastArcanePulse() {
        CmdPlayClip(1);
        onGround = false;
        speedUp = 0.5f;
        comingDown = false;
        stopMomentumCharges = 1;
        animator.ResetTrigger("PulseDown");
        CmdSetAnimTrigger("PulseUp");
        //CmdCastArcanePulse is called during Update, when the player hits the ground.
    }

    public void CastIceSpikes() {
        onGround = false;
        speedUp = 0.6f;
        movingForward = -80;
        speedForward = 0.1f;
        //do not fall faster
        stopMomentumCharges = 0;
        CmdSetAnimTrigger("ShieldBack");
        CmdCastIceSpikes();
    }

    public void CastRoyalFire(int horizontal, float horizSpeed) {
        onGround = false;
        speedUp = 0.4f;
        stopMomentumCharges = 0;
        movingRight = horizontal;
        speedRight = horizSpeed;
        if (horizontal > 0f)
            CmdSetAnimTrigger("FireballRight");
        else 
            CmdSetAnimTrigger("FireballLeft");
        CmdCastRoyalFire();
    }

    public void CastFizzle()
    {
        CmdCastFizzle();
    }

    private void StopAirMomentum() {
        if (stopMomentumCharges > 0) {
            stopMomentumCharges--;
            if (speedUp < 0) {
                speedUp = 0.2f;
            }
        }
    }

    IEnumerator WaitForLightning() {
        yield return new WaitForSeconds(0.35f);
        CmdSetAnimTrigger("LightningShoot");
        yield return new WaitForSeconds(0.1f);
        CmdCastLightning();
    }

    IEnumerator WaitForLightningCharge() {
        yield return new WaitForSeconds(0.3f);
        CmdSetAnimTrigger("LightningCharged");
    }

    [TargetRpc]
    public override void TargetThrowPlayerBack(NetworkConnection target, float horizontal, float vertical, int duration){
        movingForward = -duration;
        speedForward = horizontal;
        //speedUp = vertical;
        //StartCoroutine(ThrowBack(horizontal, vertical, duration));
    }

    //-----Commands: Client sends a message to the server; server executes the function.
    // For instantiating attacks, set owners and targets before NetworkServer.Spawn().

    [Command]
    public void CmdCastFireball() {
        GameObject newFireball = Instantiate(fireball, transform.position + Vector3.up, transform.rotation);
        newFireball.GetComponent<Fireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient, gameObject);
        newFireball.GetComponent<Fireball>().SetTarget(otherPlayer.transform.position);
        NetworkServer.Spawn(newFireball);
        //StartCoroutine(DashRight());
    }

    [Command]
    public void CmdCastRoyalFire() {
        GameObject newRoyalFireball = Instantiate(royalFireball, transform.position + Vector3.up, transform.rotation);
        newRoyalFireball.GetComponent<Royalfireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient);
        newRoyalFireball.GetComponent<Royalfireball>().SetTarget(otherPlayer.transform.position);
        NetworkServer.Spawn(newRoyalFireball);
        //StartCoroutine(DashRight());
    }

    [Command]
    public void CmdCastShieldBack() {
        GameObject newShield = Instantiate(shield, transform.position + Vector3.up, transform.rotation * Quaternion.Euler(90f, 0f, 90f));
        NetworkServer.Spawn(newShield);

        shields.Add(newShield);
        if (shields.Count > maxShields) {
            GameObject oldShield = shields[0];
            shields.RemoveAt(0);
            Destroy(oldShield);
        }
        //StartCoroutine(DashBack());
    }

    [Command]
    public void CmdCastWindForward() {
        GameObject newWindSlash = Instantiate(windslash, transform.position + (transform.forward * 2f) + Vector3.up, transform.rotation);
        newWindSlash.GetComponent<WindSlash>().SetOwner(gameObject);
        newWindSlash.GetComponent<WindSlash>().SetTarget(otherPlayer);
        NetworkServer.Spawn(newWindSlash);
        //StartCoroutine(DashForward());
    }

    [Command]
    public void CmdCastLightningCharge() {
        GameObject newChargeEffect = Instantiate(lightningChargeObj, transform.position + Vector3.up, transform.rotation);
        newChargeEffect.GetComponent<LightningCharge>().SetOwner(gameObject);
        NetworkServer.Spawn(newChargeEffect);
    }

    [Command]
    public void CmdCastLightning() {
        GameObject newLightning = Instantiate(lightning, transform.position + Vector3.up, transform.rotation);
        newLightning.GetComponent<Lightning>().SetOwner(gameObject);
        newLightning.GetComponent<Lightning>().SetTarget(otherPlayer);
        NetworkServer.Spawn(newLightning);
    }

    [Command]
    public void CmdCastArcanePulse() {
        //Arcane Pulse should spawn at the feet
        GameObject newPulse = Instantiate(arcanePulse, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        newPulse.GetComponent<ArcanePulse>().SetOwner(gameObject);
        NetworkServer.Spawn(newPulse);
    }

    [Command]
    public void CmdCastIceSpikes() {
        //Arcane Pulse should spawn at the feet
        GameObject newIceSpikes = Instantiate(iceSpikeProjectile, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        newIceSpikes.GetComponent<IceSpikeProjectile>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient);
        NetworkServer.Spawn(newIceSpikes);
    }

    [Command]
    public void CmdCastFizzle()
    {
        GameObject newFizzle = Instantiate(fizzle, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        NetworkServer.Spawn(newFizzle);
    }

    IEnumerator ThrowBack(float throwHorizontal, float throwVertical, float duration=0.4f) {
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 3*duration) {
            transform.position -= transform.forward * throwHorizontal * 2*duration * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * throwVertical * 0.5f) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
    }
}