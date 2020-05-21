using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Mirror;

public class PlayerBehaviour : CharacterBehaviour
{

    public UnityEngine.UI.Image hp1, hp2, hp3, hp4;


    private int lightningChargesNeeded = 3;

    public NetworkAnimator animator;

    private float playerHeight = 1f;

    public Timer timer;

    public AudioClip[] clips;

    private bool onGround = true;

    public Color red = new Color(1f, 0f, 0f, 1f);
    private Color white = new Color(1f, 1f, 1f, 1f);

    public int movingRight = 0;
    public int movingForward = 0;
    public int movingUp = 0;
    public float speedRight = 0f;
    public float speedForward = 0f;
    public float speedUp = 0f;

    public bool comingDown = false;

    private bool firstHit = true;
    private int currentRound;

    private bool isAIChar;

    // After using Pulse, number of times spells casted in the air will stop air momentum.
    private int stopMomentumCharges = 0;

    public GameManager gameManager;
    public string lastSpellCast;

    private Vector3 startGripMove, releaseGripMove;
    public int dragMoveSpeed = 10;
    private float movementCooldown = 2f;

    public float spellVelocity = 20;
    public string heldSpell = null;
    public GameObject castingHand, movingHand, spellReticle, baseReticle;
    public XRController castingHandController, movingHandController;
    public XRRayInteractor castingRay;
    public XRInteractorLineVisual castingLineRenderer;
    public SkinnedMeshRenderer castingHandRenderer, movementHandRenderer;
    public Material baseMaterial, spellHandGlow, movementCooldownGlow;
    public ParticleSystem fireParticles, lightningParticles, windParticles, royalParticles, iceParticles, damageParticles;
    public GameObject shieldSphere, arcanoSphere;
    public Dictionary<string, Color> handColours = new Dictionary<string, Color>();

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
        StartCoroutine(MovementCooldownManager());
    }

    public void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        red = new Color(1f, 0f, 0f, 1f);
        shields = new List<GameObject>();

        handColours.Add("fireball", new Color(1f, 0, 0));
		handColours.Add("shield", new Color(113/255f, 199/255f, 1f));
		handColours.Add("windslash", new Color(26/255f, 1f, 0));
		handColours.Add("lightning", new Color(1f, 247/255f, 103/255f));
		handColours.Add("arcanopulse", new Color(214/255f, 135/255f, 1f));
		handColours.Add("icespikes", new Color(50/255f, 50/255f, 1f));
		handColours.Add("royalfire", new Color(156/255f, 0f, 1f));

        castingHand = GameObject.Find("RightHand Controller");
        Debug.Log("Casting Hand GameObject: "+ castingHand);

        try {
            castingRay = castingHand.GetComponent<XRRayInteractor>();
            castingLineRenderer = castingHand.GetComponent<XRInteractorLineVisual>();
        } catch {
            Debug.Log("Could not get: casting ray and/or casting line renderer");
        }
        
        spellReticle = GameObject.Find("TargetingReticle");
        baseReticle = GameObject.Find("Reticle");

        movingHand = GameObject.Find("LeftHand Controller");
        Debug.Log("Moving Hand GameObject: "+ movingHand);

        castingHandRenderer = GameObject.Find("hands:Rhand").GetComponent<SkinnedMeshRenderer>();
        Debug.Log("Casting Hand render: " + GameObject.Find("hands:Rhand"));

        movementHandRenderer = GameObject.Find("hands:Lhand").GetComponent<SkinnedMeshRenderer>();
        Debug.Log("Movement Hand render: " + GameObject.Find("hands:Lhand"));

        fireParticles = GameObject.Find("FireParticles").GetComponent<ParticleSystem>();
        shieldSphere = GameObject.Find("RightHand Controller").transform.Find("ShieldSphere").gameObject;
        arcanoSphere = GameObject.Find("RightHand Controller").transform.Find("ArcanoSphere").gameObject;
        lightningParticles = GameObject.Find("LightningParticles").GetComponent<ParticleSystem>();
        windParticles = GameObject.Find("WindParticles").GetComponent<ParticleSystem>();
        royalParticles = GameObject.Find("RoyalParticles").GetComponent<ParticleSystem>();
        iceParticles = GameObject.Find("IceParticles").GetComponent<ParticleSystem>();
        damageParticles = GameObject.Find("AnchorDamageParticles").GetComponent<ParticleSystem>();

    }


    /* UI & GAME LOGIC 
    --------------------------------------------------------------------------------------------------------
        --------------------------------------------------------------------------------------------------------
            --------------------------------------------------------------------------------------------------------
                -------------------------------------------------------------------------------------------------------- */
    [Command]
    public void CmdResetMatch(){
        gameManager.ResetMatch();
    }

    [TargetRpc]
    public new void TargetResetPosition(NetworkConnection connection, Vector3 pos) {
        transform.position = pos;
    }

    public void InitializeUI() {
        //timer = GameObject.Find("Timer").GetComponent<Timer>();

        hp1 = GameObject.Find("HP1").GetComponent<UnityEngine.UI.Image>();
        hp2 = GameObject.Find("HP2").GetComponent<UnityEngine.UI.Image>();
        hp3 = GameObject.Find("HP3").GetComponent<UnityEngine.UI.Image>();
        hp4 = GameObject.Find("HP4").GetComponent<UnityEngine.UI.Image>();
    }

    [ClientRpc]
    public override void RpcResetUI() {
        // Disable rematch button
        // GameObject.Find("GameUI").transform.Find("ReadyPanel").gameObject.SetActive(false);

        // Wait for round to start before enabling input
        //StartCoroutine(WaitForRoundStart());

        // Recolour the health bubbles
        GameObject.Find("HP1").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);
        GameObject.Find("HP2").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);
        GameObject.Find("HP3").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);
        GameObject.Find("HP4").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);

        // Disable Win/Loss text
        try {
            GameObject.Find("RoundWinText").SetActive(false);
        } catch {
            Debug.Log("Could not disable round win text / already disabled");
        }
        //GameObject.Find("Drawing Plane").transform.Find("RoundWinText").gameObject.SetActive(false);
        //GameObject.Find("GameUI").transform.Find("LossPanel").gameObject.SetActive(false);

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
        try {
            GameObject winTextGO = GameObject.Find("Drawing Plane").transform.Find("RoundWinText").gameObject;
            winTextGO.SetActive(true);
            UnityEngine.UI.Text winText = winTextGO.GetComponent<UnityEngine.UI.Text>();
            if (p1Score >= 3) {
                winText.text = "You Win!";
            }
            else if (p2Score >= 3) {
                winText.text = "You have lost.";
            }
            else {
                winText.text = p1Score + " - " + p2Score;
            }
        } catch {
            Debug.Log("Error displaying Round Win Text");
        }
        
        
        // Display Rematch button
        try {
            GameObject.Find("Drawing Plane").transform.Find("Rematch").gameObject.SetActive(true);
            if (p1Score >= 3 || p2Score >= 3) {
                GameObject.Find("Drawing Plane").transform.Find("Rematch").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Rematch?";
            }
            else {
                GameObject.Find("Drawing Plane").transform.Find("Rematch").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Next Round";
            }
        } catch {
            Debug.Log("Error displaying Rematch button");
        }
        
        currentRound = round;

        //Update # of wins and round number
        GameObject.Find("WinsText").GetComponent<UnityEngine.UI.Text>().text = p1Score + " - " + p2Score;
    }
    [TargetRpc]
    public void TargetLoseRound(NetworkConnection connection, int p1Score, int p2Score, int round) {
        GameObject.Find("GameUI").transform.Find("LossPanel").gameObject.SetActive(true);
        
        // if (p1Score >= 3) {
        //     GameObject.Find("GameUI").transform.Find("LossPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "Player 1 Wins!";
        // }
        // else if (p2Score >= 3) {
        //     GameObject.Find("GameUI").transform.Find("LossPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "Player 2 Wins!";
        // }
        // else {
        //     GameObject.Find("GameUI").transform.Find("LossPanel").transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = p1Score + " - " + p2Score;
        // }

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







    void FixedUpdate() {
        if (castingHandController == null) {
            castingHandController = castingHand.GetComponent<XRController>();
        }

        if (movingHandController == null) {
            movingHandController = movingHand.GetComponent<XRController>();
        }

        if (heldSpell != null) {
            castingHandController.inputDevice.SendHapticImpulse(0, 0.5f, 0.1f);
        }
    }


    void Update()
    {
        if (otherPlayer != null) {
            transform.LookAt(otherPlayer.transform);
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }

        if (hp1 == null) {
            InitializeUI();
        }
        // // JOYSTICK MOVEMENT, LEAVE DISABLED
        // try {
        //     if (movingHand.GetComponent<XRController>().inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 position)) {
        //         ControllerJoystickMove(position);
        //     }
        // } catch {
        //     Debug.Log("Cannot move player");
        // }
        
        // else if( health <= 0 && GetComponent<Animator>().runtimeAnimatorController != null){
        //     // set a global death flag to enter finished screen
        //     //Debug.Log(this.gameObject.name +" is dead");
        //     //GetComponent<CapsuleCollider>().enabled = !GetComponent<CapsuleCollider>().enabled;
        //     GetComponent<Animator>().runtimeAnimatorController = null;

        //     // Enable rematch button
        //     GameObject.Find("GameUI").transform.Find("ReadyPanel").gameObject.SetActive(true);

        //     // Disable glyph input
        //     //GameObject.Find("Canvas").transform.Find("Basic Glyph Input").GetComponent<GlyphRecognition>().ClearAll();
        //     //GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject.SetActive(false);

        //     // Stop the timer
        //     //timer.StopTimer();
        // }

        // Needs to be in Update as there appear to be damage timing issues.
        if (hasAuthority) {
            if (health < 4) {
                hp4.color = red;
            } else {
                hp4.color = white;
            }
            if (health < 3) {
                hp3.color = red;
            } else {
                hp3.color = white;
            }
            if (health < 2) {
                hp2.color = red;
            } else {
                hp2.color = white;
            }
            if (health < 1) {
                hp1.color = red;
            } else {
                hp1.color = white;
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


























    /* MOVEMENT 
    --------------------------------------------------------------------------------------------------------
        --------------------------------------------------------------------------------------------------------
            --------------------------------------------------------------------------------------------------------
                -------------------------------------------------------------------------------------------------------- */
    IEnumerator Movement() {
        while (true) {
            float distanceFromCenter = DistanceToCenter();

            if (distanceFromCenter < 28){
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

    [TargetRpc]
    public override void TargetThrowPlayerBack(NetworkConnection target, float horizontal, float vertical, int duration){
        movingForward = -duration;
        speedForward = horizontal;
        movementCooldown = 1.0f;
    }

    public void StopAirMomentum() {
        if (stopMomentumCharges > 0) {
            stopMomentumCharges--;
            if (speedUp < 0) {
                speedUp = 0.2f;
            }
        }
    }

    private void ControllerJoystickMove (Vector2 position)
    {
        // Apply the touch position to the head's forward Vector
        Vector3 direction = new Vector3(position.x, 0, position.y);
        Vector3 headRotation = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);

        // Rotate the input direction by the horizontal head rotation
        direction = Quaternion.Euler(headRotation) * direction;

        // Apply speed and move
        Vector3 movement = direction * 3;
        transform.position += movement * Time.deltaTime;
    }

    IEnumerator MovementCooldownManager() {
        while (true) {
            if (movementCooldown > 0f) {
                movementCooldown -= Time.deltaTime;
            } else {
                movementCooldown = 0f;
                movementHandRenderer.material = baseMaterial;
            }
            yield return new WaitForFixedUpdate();
        }

    }

    public void StartGripMove(Vector3 startPos){
        startGripMove = new Vector3(startPos.x, 0f, startPos.z);
        //Debug.Log("StartGripMove:  "+startGripMove.ToString());
        movingHandController.inputDevice.SendHapticImpulse(0, 0.5f, 0.3f);
    }

    public void ReleaseGripMove(Vector3 endPos) {
        releaseGripMove = new Vector3(endPos.x, 0f, endPos.z);

        if (movementCooldown == 0) {
            Vector3 movement = startGripMove - releaseGripMove;
            //Debug.Log("Movement: "+movement.ToString()+ "   Magnitude: "+movement.magnitude);

            // Move player in direction of pull * speed
            transform.position += movement * dragMoveSpeed;

            // Set hand cooldown material
            movementHandRenderer.material = movementCooldownGlow;

            // Set movement Cooldown
            movementCooldown = 2.5f;

            
            try {
                movingHandController.inputDevice.SendHapticImpulse(0, 2f, 0.1f);
                movingHand.GetComponent<AudioSource>().Play();
            } catch {
                Debug.Log("Failure to send haptics or play sound effect");
            }
            
        } else {
            Debug.Log("Movement on cooldown");
        }
    }

















    /* SPELLCASTING FUNCTIONS
    --------------------------------------------------------------------------------------------------------
        --------------------------------------------------------------------------------------------------------
            --------------------------------------------------------------------------------------------------------
                -------------------------------------------------------------------------------------------------------- */

    //Server: Only the server executes the function. 
    //(However, because the variable is synced, clients will also see the HP decrease.)
    [Server]
    public new void TakeDamage(int dmg) {
        //Debug.Log("Damage Took:  "+this.name+"        health: "+health);
        health -= (dmg);
    }

    //TargetRpc: Effect will only appear on the targeted network client.
    [TargetRpc]
    public override void TargetShowDamageEffects(NetworkConnection target) {
        //UnityEngine.UI.Image redscreen = GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject.GetComponent<UnityEngine.UI.Image>();
        //redscreen.color = new Color(1f, 0f, 0f, 0.8f);
        //Debug.Log("Player HIT");
        damageParticles.Play();

        castingHandController.inputDevice.SendHapticImpulse(0, 2f, 0.5f);
        movingHandController.inputDevice.SendHapticImpulse(0, 2f, 0.5f);

        //Color red = new Color(1f, 0f, 0f, 1f);
    }

    public void SetHandGlow(string spell){
        if(castingHandRenderer && spellHandGlow){
            // Debug.Log("Setting hand colour to: "+handColours[spell]);
            spellHandGlow.SetColor("Color_682024A", handColours[spell]);
            // Debug.Log("Set hand material to "+spell);
            castingHandRenderer.material = spellHandGlow;
        }
    }

    public void EnableProjectileLine(float maxHeight, float sVelocity) {
        castingRay.lineType = XRRayInteractor.LineType.ProjectileCurve;
        castingRay.controlPointHeight = maxHeight;
        castingRay.enableUIInteraction = false;
        castingRay.Velocity = sVelocity != 0 ? sVelocity : spellVelocity;

        // Swap to spell reticle and enable it.
        castingLineRenderer.reticle = spellReticle;
        castingLineRenderer.reticle.SetActive(true);
    }

    public void DisableProjectileLine() {
        castingRay.lineType = XRRayInteractor.LineType.StraightLine;
        castingRay.enableUIInteraction = true;

        // Disable the spell reticle so that it doesnt persist
        castingLineRenderer.reticle.SetActive(false);

        // Swap back to small sphere reticle
        castingLineRenderer.reticle = baseReticle;
    }

    [Command]
    public void CmdSetAnimTrigger(string s) {
        //print("Calling animation for " + s);
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

    public void ReleaseSpellCast() {
        if (heldSpell != null) {
            // Debug.Log("Player Cast Held Spell: "+heldSpell);
            switch (heldSpell) {
                case "fireball":
                    CastHeldFireball(25,1f);
                    break;
                case "shield":
                    CastHeldShield();
                    break;
                case "lightning":
                    CastHeldLightning();
                    break;
                case "windslash":
                    CastHeldWindSlash();
                    break;
                case "royalfire":
                    CastHeldRoyalFire(-25,1f);
                    break;
                case "icespikes":
                    CastHeldIceSpikes();
                    break;
                case "arcanopulse":
                    CastHeldArcanoPulse();
                    break;
            }
            lastSpellCast = heldSpell;
            heldSpell = null;
            castingHandRenderer.material = baseMaterial;

            // Reset movement cooldown when a spell is cast
            movementCooldown = 0f;
            movementHandRenderer.material = baseMaterial;
        }
    }





















    /* SPELLS
    --------------------------------------------------------------------------------------------------------
        --------------------------------------------------------------------------------------------------------
            --------------------------------------------------------------------------------------------------------
                -------------------------------------------------------------------------------------------------------- */




    //  ------------- FIREBALL ------------------
    public virtual void CastFireball(int horizontal, float horizSpeed) {
        heldSpell = "fireball";
        SetHandGlow(heldSpell);
        EnableProjectileLine(7f, 25f);
        if (fireParticles) {
            //print("fire particles play");
            fireParticles.Play();
        }
        // Debug.Log("PLAYERFIRECAST");
    }

    public void CastHeldFireball(int horizontal, float horizSpeed) {
        StopAirMomentum();
        if (horizontal > 0f)
            CmdSetAnimTrigger("FireballRight");
        else 
            CmdSetAnimTrigger("FireballLeft");
        CmdCastFireball();
        fireParticles.Stop();
        DisableProjectileLine();

        try {
            otherPlayer.GetComponent<AIBehaviour>().ReactionCast("fireball");
        } catch {
            Debug.Log("Could not send react event to AI");
        }
    }

    [Command]
    public void CmdCastFireball() {
        GameObject newFireball = Instantiate(fireball, castingHand.transform.position, transform.rotation);
        newFireball.GetComponent<Fireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient, gameObject, true);

        // Get position of the casting projectile ray target hit
        RaycastHit rayHit;
        Vector3 target = otherPlayer.transform.position;
        if (castingRay.GetCurrentRaycastHit(out rayHit)) {
            target = rayHit.point;
        }

        newFireball.GetComponent<Fireball>().SetTarget(target);
        NetworkServer.Spawn(newFireball);
    }








    //  ------------- SHIELD ------------------
    public void CastShieldBack() {
        heldSpell = "shield";
        SetHandGlow(heldSpell);
        if (shieldSphere) {
            //print("activate shield sphere");
            shieldSphere.SetActive(true);
        }
        // Debug.Log("PLAYERSHIELDCAST");
    }

    public void CastHeldShield() {
        CmdSetAnimTrigger("ShieldBack");
        CmdCastShieldBack();
        shieldSphere.SetActive(false);

        try {
            otherPlayer.GetComponent<AIBehaviour>().ReactionCast("shield");
        } catch {
            Debug.Log("Could not send react event to AI");
        }
    }

    [Command]
    public void CmdCastShieldBack() {
        GameObject newShield = Instantiate(shield, castingHand.transform.position + (castingHand.transform.forward*2), castingHand.transform.rotation * Quaternion.Euler(90f, 0f, 90f));
        NetworkServer.Spawn(newShield);

        shields.Add(newShield);
        if (shields.Count > maxShields) {
            GameObject oldShield = shields[0];
            shields.RemoveAt(0);
            Destroy(oldShield);
        }
    }








    //  ------------- WIND SLASH ------------------
    public void CastWindForward() {
        heldSpell = "windslash";
        SetHandGlow(heldSpell);
        if (windParticles) {
            //print("wind particles play");
            windParticles.Play();
        }
        // Debug.Log("PLAYERWINDSLASHCAST");
    }

    public void CastHeldWindSlash(){
        StopAirMomentum();
        movingForward = 20;
        speedForward = 2f;
        CmdSetAnimTrigger("WindSlash");
        CmdCastWindForward();
        windParticles.Stop();

        try {
            otherPlayer.GetComponent<AIBehaviour>().ReactionCast("windslash");
        } catch {
            Debug.Log("Could not send react event to AI");
        }
    }

    [Command]
    public void CmdCastWindForward() {
        GameObject newWindSlash = Instantiate(windslash, transform.position + (transform.forward * 2f) + Vector3.up, transform.rotation);
        newWindSlash.GetComponent<WindSlash>().SetOwner(gameObject);
        newWindSlash.GetComponent<WindSlash>().SetTarget(otherPlayer);
        NetworkServer.Spawn(newWindSlash);
    }










    //  ------------- LIGHTNING ------------------
    public void CastLightningNeutral() {
        CmdPlayClip(0);
        StopAirMomentum();
        animator.ResetTrigger("LightningCharged");
        animator.ResetTrigger("LightningShoot");
        CmdSetAnimTrigger("LightningCharging");
        CmdCastLightningCharge();
        lightningCharge++;
        if (lightningCharge == lightningChargesNeeded) {
            heldSpell = "lightning";
            SetHandGlow("lightning");
            if (lightningParticles) {
                lightningParticles.Play();
            }
            // Debug.Log("PLAYERLIGHTNINGCAST");
        }
        else {
            StartCoroutine(WaitForLightningCharge());
        }

        // AI reaction happens on cast, and not spell release, bc that would be unfair
        try {
            otherPlayer.GetComponent<AIBehaviour>().ReactionCast("lightning");
        } catch {
            Debug.Log("Could not send react event to AI");
        }
    }

    public void CastHeldLightning() {
        StartCoroutine(WaitForLightning());
        lightningCharge = 0;
        lightningParticles.Stop();
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

    [Command]
    public void CmdCastLightningCharge() {
        GameObject newChargeEffect = Instantiate(lightningChargeObj, castingHand.transform.position, transform.rotation);
        newChargeEffect.GetComponent<LightningCharge>().SetOwner(gameObject);
        NetworkServer.Spawn(newChargeEffect);
    }

    [Command]
    public void CmdCastLightning() {
        GameObject newLightning = Instantiate(lightning, castingHand.transform.position, transform.rotation);
        newLightning.GetComponent<Lightning>().SetOwner(gameObject);
        newLightning.GetComponent<Lightning>().SetTarget(otherPlayer);
        NetworkServer.Spawn(newLightning);
    }








    //  ------------- ARCANOPULSE ------------------
    public void CastArcanePulse() {
        heldSpell = "arcanopulse";
        SetHandGlow(heldSpell);
        if (arcanoSphere) {
            //print("activate arcano sphere");
            arcanoSphere.SetActive(true);
        }
        // Debug.Log("PLAYERARCANOPULSECAST");
    }

    public void CastHeldArcanoPulse() {
        CmdPlayClip(1);
        CmdCastArcanePulse();
        arcanoSphere.SetActive(false);

        try {
            otherPlayer.GetComponent<AIBehaviour>().ReactionCast("arcanopulse");
        } catch {
            Debug.Log("Could not send react event to AI");
        }
    }

    [Command]
    public void CmdCastArcanePulse() {
        //Arcane Pulse should spawn at the feet
        GameObject newPulse = Instantiate(arcanePulse, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        newPulse.GetComponent<ArcanePulse>().SetOwner(gameObject);
        NetworkServer.Spawn(newPulse);
    }









    //  ------------- ICE SPIKES ------------------
    public void CastIceSpikes() {
        heldSpell = "icespikes";
        SetHandGlow(heldSpell);
        if (iceParticles) {
            //print("ice particles play");
            iceParticles.Play();
        }
    }

    public void CastHeldIceSpikes() {
        CmdSetAnimTrigger("ShieldBack");
        CmdCastIceSpikes();
        iceParticles.Stop();

        try {
            otherPlayer.GetComponent<AIBehaviour>().ReactionCast("icespikes", castingHand.transform.rotation.eulerAngles);
        } catch {
            Debug.Log("Could not send react event to AI");
        }
    }

    [Command]
    public void CmdCastIceSpikes() {
        //Ice spikes should spawn at the feet
        Quaternion dir = Quaternion.Euler(0, castingHand.transform.rotation.eulerAngles.y, 0);
        Vector3 startPos = new Vector3(castingHand.transform.position.x, 0f, castingHand.transform.position.z) + castingHand.transform.forward;
        GameObject newIceSpikes = Instantiate(iceSpikeProjectile, startPos, dir);
        newIceSpikes.GetComponent<IceSpikeProjectile>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient);
        NetworkServer.Spawn(newIceSpikes);
    }







    //  ------------- ROYAL FIRE ------------------
    public void CastRoyalFire(int horizontal, float horizSpeed) {
        heldSpell = "royalfire";
        SetHandGlow(heldSpell);
        EnableProjectileLine(6f, 16f);
        if (royalParticles) {
            //print("royal particles play");
            royalParticles.Play();
        }
        // Debug.Log("PLAYERROYALFIRECAST");
    }

    public void CastHeldRoyalFire(int horizontal, float horizSpeed){
        if (horizontal > 0f)
            CmdSetAnimTrigger("FireballRight");
        else 
            CmdSetAnimTrigger("FireballLeft");
        CmdCastRoyalFire();
        royalParticles.Stop();
        DisableProjectileLine();

        try {
            otherPlayer.GetComponent<AIBehaviour>().ReactionCast("royalfire");
        } catch {
            Debug.Log("Could not send react event to AI");
        }
    }

    [TargetRpc]
    public void TargetTakeRoyalBurn(NetworkConnection connection, float b) {
        royalBurn += b;
        if (royalBurn > 1f) {
            health -= 1;
            royalBurn = 0f;
        }
    }

    [Command]
    public void CmdCastRoyalFire() {
        GameObject newRoyalFireball = Instantiate(royalFireball, castingHand.transform.position, transform.rotation);
        newRoyalFireball.GetComponent<Royalfireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient, gameObject);

        // Get position of the casting projectile ray target hit
        RaycastHit rayHit;
        Vector3 target = otherPlayer.transform.position;
        if (castingRay.GetCurrentRaycastHit(out rayHit)) {
            target = rayHit.point;
        }

        newRoyalFireball.GetComponent<Royalfireball>().SetTarget(target);
        NetworkServer.Spawn(newRoyalFireball);
    }






    // ------ FIZZLE ------
    public void CastFizzle()
    {
        CmdCastFizzle();
    }

    [Command]
    public void CmdCastFizzle()
    {
        GameObject newFizzle = Instantiate(fizzle, new Vector3(castingHand.transform.position.x, 0f, castingHand.transform.position.z), transform.rotation);
        NetworkServer.Spawn(newFizzle);
    }
}