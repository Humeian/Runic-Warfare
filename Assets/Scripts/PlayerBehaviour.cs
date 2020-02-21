using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerBehaviour : NetworkBehaviour
{
    [SyncVar]
    public GameObject otherPlayer;

    [SyncVar]
    public int health = 2;

    [SyncVar]
    public int lightningCharge = 0;
    

    private int lightningChargesNeeded = 3;

    public NetworkAnimator animator;

    public float dashSpeed;
    public float dashHeight;
    private float playerHeight = 1f;
    
    public GameObject fireball;
    public GameObject shield;
    public GameObject windslash;
    public GameObject lightningChargeObj;
    public GameObject lightning;
    public RuntimeAnimatorController controller;

    int movingRight = 0;
    int movingForward = 0;
    float speedRight = 0f;
    float speedForward = 0f;

    private bool firstHit = true;

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

    IEnumerator testmove() {
        while (true) {
            transform.position += transform.forward * Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    //called by NewNetworkManager
    public void SetOtherPlayer(GameObject op) {
        otherPlayer = op;
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
        }

        if (Input.GetKey(KeyCode.H)) {
            CmdRestoreHealth(2);
        }
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
        UnityEngine.UI.Image red = GameObject.FindGameObjectWithTag("GlyphRecognition").GetComponent<UnityEngine.UI.Image>();
        red.color = new Color(1f, 0f, 0f, 0.8f);

        Camera.main.GetComponent<PlayerCamera>().Shake(5f);

        if (firstHit){
            firstHit = false;
            GameObject.Find("First").GetComponent<UnityEngine.UI.Image>().color = new Color(255, 0, 0);
        } else {
            GameObject.Find("Last").GetComponent<UnityEngine.UI.Image>().color = new Color(255, 0, 0);
        }
    }

    [TargetRpc]
    public void TargetPaintScreen(NetworkConnection target, Color c) {
        UnityEngine.UI.Image screen = GameObject.FindGameObjectWithTag("GlyphRecognition").GetComponent<UnityEngine.UI.Image>();
        screen.color = c;
    }

    public void SetAnimTrigger(string s) {
        animator.SetTrigger(s);
    }

    // For outside animation triggers such as WindSlashRecoil.
    [TargetRpc]
    public void TargetSetAnimTrigger(NetworkConnection target, string s) {
        animator.SetTrigger(s);
    }

    public void CastFireballRight() {
        //transform.position += transform.TransformDirection(Vector3.right);
        movingRight = 25;
        speedRight = 1f;
        SetAnimTrigger("FireballRight");
        CmdCastFireballRight();
    }

    public void CastWindForward() {
        movingForward = 20;
        speedForward = 2f;
        SetAnimTrigger("WindSlash");
        CmdCastWindForward();
    }

    public void CastShieldBack() {
        movingForward = -30;
        speedForward = 0.4f;
        SetAnimTrigger("ShieldBack");
        CmdCastShieldBack();
    }

    public void CastLightningNeutral() {
        CmdCastLightningCharge();
        lightningCharge++;
        if (lightningCharge == lightningChargesNeeded) {
            StartCoroutine(WaitForLightning());
            lightningCharge = 0;
        }
        
    }

    IEnumerator WaitForLightning() {
        yield return new WaitForSeconds(0.45f);
        CmdCastLightning();
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
    public void CmdCastFireballRight() {
        GameObject newFireball = Instantiate(fireball, transform.position + Vector3.up, transform.rotation);
        newFireball.GetComponent<Fireball>().SetTarget(otherPlayer.transform.position);
        NetworkServer.Spawn(newFireball);
        //StartCoroutine(DashRight());
    }

    [Command]
    public void CmdCastShieldBack() {
        GameObject newShield = Instantiate(shield, transform.position + Vector3.up, transform.rotation * Quaternion.Euler(90f, 0f, 90f));
        NetworkServer.Spawn(newShield);
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