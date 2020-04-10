using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerBehaviour : NetworkBehaviour
{
    [SyncVar]
    public GameObject otherPlayer;

    [SyncVar]
    public int health = 3;

    public UnityEngine.UI.Image hp1, hp2, hp3;

    [SyncVar]
    public int lightningCharge = 0;

    //Royal Fire will deal damage if royalBurn reaches 1
    [SyncVar]
    public float royalBurn = 0f;
    
    //royalBurn decreases by this every second
    public float royalBurnRecovery = 0.2f;

    private int lightningChargesNeeded = 3;

    public NetworkAnimator animator;

    public float dashSpeed;
    public float dashHeight;
    private float playerHeight = 1f;

    public GameObject fireball;
    public GameObject royalFireball;
    public GameObject shield;
    public GameObject windslash;
    public GameObject lightningChargeObj;
    public GameObject lightning;
    public GameObject arcanePulse;
    public GameObject iceSpikeProjectile;
    public GameObject fizzle;
    public RuntimeAnimatorController controller;
    public Timer timer;

    public AudioClip[] clips;

    private bool onGround = true;

    private Color red;

    public List<GameObject> shields;
    public int maxShields = 2;

    int movingRight = 0;
    int movingForward = 0;
    int movingUp = 0;
    float speedRight = 0f;
    float speedForward = 0f;
    float speedUp = 0f;

    bool comingDown = false;

    private bool firstHit = true;

    // After using Pulse, number of times spells casted in the air will stop air momentum.
    private int stopMomentumCharges = 0;

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
        timer = GameObject.Find("Timer").GetComponent<Timer>();

        hp1 = GameObject.Find("HP1").GetComponent<UnityEngine.UI.Image>();
        hp2 = GameObject.Find("HP2").GetComponent<UnityEngine.UI.Image>();
        hp3 = GameObject.Find("HP3").GetComponent<UnityEngine.UI.Image>();
    }

    public void Start() {
        red = new Color(1f, 0f, 0f, 1f);
        shields = new List<GameObject>();
    }

    IEnumerator testmove() {
        while (true) {
            transform.position += transform.forward * Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    //called by NewNetworkManager
    public void SetOtherPlayer(GameObject op) {
        otherPlayer = op;
        //timer.StartTimer();
    }

    [Command]
    public void CmdResetMatch(){
        GameObject.Find("GameManager").GetComponent<GameManager>().ResetMatch();

        /*
        //CmdRestoreHealth(3);
        //lightningCharge = 0;

        // Disable rematch button

        // Enable glyph input & reboot the color cleaning coroutine
        GameObject glyphInput = GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject;
        glyphInput.SetActive(true);
        glyphInput.GetComponent<GlyphRecognition>().InitCleanScreen();

        // Timer reset is done in the onClick() of the rematch button

        // Reset health bubble colour
        GameObject.Find("First").GetComponent<UnityEngine.UI.Image>().color = new Color(245, 245, 245);
        GameObject.Find("Last").GetComponent<UnityEngine.UI.Image>().color = new Color(245, 245, 245);
        //firstHit = true;
        */
    }

    [TargetRpc]
    public void TargetResetPosition(NetworkConnection connection, Vector3 pos) {
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

    [ClientRpc]
    public void RpcResetUI() {
        // Disable rematch button
        GameObject.Find("GameUI").transform.Find("ReadyPanel").gameObject.SetActive(false);

        // Enable glyph input & reboot the color cleaning coroutine
        GameObject glyphInput = GameObject.Find("Canvas").transform.Find("Basic Glyph Input").gameObject;
        glyphInput.SetActive(true);
        glyphInput.GetComponent<GlyphRecognition>().InitCleanScreen();

        GameObject.Find("HP1").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);
        GameObject.Find("HP2").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);
        GameObject.Find("HP3").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f);

        shields.Clear();
        lightningCharge = 0;

        //reset momentum
        movingForward = 0;
        movingRight = 0;
        movingUp = 0;
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
            GameObject.FindWithTag("GlyphRecognition").GetComponent<GlyphRecognition>().ClearAll();
            GameObject.FindWithTag("GlyphRecognition").SetActive(false);

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
                hp1.color = new Color(1f, 1f, 1f, 1f);
            }
            if (health < 2) {
                hp2.color = red;
            } else {
                hp2.color = new Color(1f, 1f, 1f, 1f);
            }
            if (health < 1) {
                hp3.color = red;
            } else {
                hp3.color = new Color(1f, 1f, 1f, 1f);
            }
        }

        if (royalBurn > 0f) {
            royalBurn -= royalBurnRecovery * Time.deltaTime; 
        }
        else 
            royalBurn = 0f;
    }

    [Command]
    public void CmdRestoreHealth(int h) {
        health = h;
    }

    IEnumerator Movement() {
        while (true) {
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

            if (!onGround) {
                //fall faster if falling down and no spell has been used -- easier to time reflect pulse
                if (speedUp < 0 && stopMomentumCharges > 0) {
                    if (!comingDown && hasAuthority) {
                        CmdSetAnimTrigger("PulseDown");
                        comingDown = true;
                    }
                    transform.position += transform.up * speedUp * 200f * Time.deltaTime;
                }
                else {
                    transform.position += transform.up * speedUp * 50f * Time.deltaTime;
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
    public void TakeDamage(int dmg) {
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
    public void TargetSetAnimTrigger(NetworkConnection target, string s) {
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
        speedUp = 0.6f;
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

    public void CastRoyalFire() {
        onGround = false;
        speedUp = 0.4f;
        stopMomentumCharges = 0;
        movingRight = 50;
        speedRight = 0.2f;
        CmdSetAnimTrigger("FireballRight");
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
    public void TargetThrowPlayerBack(NetworkConnection target, float horizontal, float vertical, int duration){
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
        newFireball.GetComponent<Fireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient);
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

    IEnumerator DashLeft() {
        float duration = 0.6f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f) {
            transform.position -= transform.right * dashSpeed * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * dashHeight) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DashRight() {
        float duration = 0.6f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f) {
            transform.position += transform.right * dashSpeed * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * dashHeight) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DashBack() {
        float duration = 0.4f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f) {
            transform.position -= transform.forward * dashSpeed * 0.8f * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * dashHeight * 0.5f) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
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

    IEnumerator DashForward() {
        float duration = 0.125f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f) {
            transform.position += transform.forward * dashSpeed * 4f * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            //print(currentTime);

            yield return new WaitForEndOfFrame();
        }

    }
}