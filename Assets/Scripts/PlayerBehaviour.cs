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

    public float dashSpeed;
    public float dashHeight;
    private float playerHeight = 1f;
    
    public GameObject fireball;
    public GameObject shield;
    public GameObject windslash;

    int movingRight = 0;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }

    //called by NewNetworkManager
    public void SetOtherPlayer(GameObject op) {
        otherPlayer = op;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (otherPlayer != null) {
            transform.LookAt(otherPlayer.transform);
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }

        if( health <= 0){
            // set a global death flag to enter finished screen
            Debug.Log(this.gameObject.name +" is dead");
        }

        if (movingRight > 0) {
            transform.position += transform.right * Time.deltaTime * (movingRight * 1f);
            movingRight--;
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
    public void TargetRedScreen(NetworkConnection target) {
        UnityEngine.UI.Image red = GameObject.FindGameObjectWithTag("GlyphRecognition").GetComponent<UnityEngine.UI.Image>();
        red.color = new Color(1f, 0f, 0f, 0.8f);

        Camera.main.GetComponent<PlayerCamera>().Shake(5f);
    }

    public void CastFireballRight() {
        //StartCoroutine(DashRight());
        //transform.position += transform.TransformDirection(Vector3.right);
        movingRight += 30;
        CmdCastFireballRight();
    }

    //Command: Client sends a message to the server; server executes the function.
    [Command]
    public void CmdCastFireballRight() {
        GameObject newFireball = Instantiate(fireball, transform.position, transform.rotation);
        NetworkServer.Spawn(newFireball);
        newFireball.GetComponent<Fireball>().SetTarget(otherPlayer.transform.position);
        //StartCoroutine(DashRight());
    }

    public void CastShieldBack() {
        GameObject newShield = Instantiate(shield, transform.position, transform.rotation * Quaternion.Euler(90f, 0f, 90f));
        StartCoroutine(DashBack());
    }

    public void CastWindForward() {
        GameObject newWindSlash = Instantiate(windslash, transform.position + (transform.forward * 2f), transform.rotation, transform);
        StartCoroutine(DashForward());
    }

    public void ThrowPlayerBack(float horizontal, float vertical, float duration){
        StartCoroutine(ThrowBack(horizontal, vertical, duration));
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