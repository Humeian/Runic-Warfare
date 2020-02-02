using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    private GameObject otherPlayer;
    public float dashSpeed;
    public float dashHeight;
    private float playerHeight = 1f;

    public int health = 2;
    public GameObject fireball;
    public GameObject shield;
    public GameObject windslash;

    // Start is called before the first frame update
    void Start()
    {
        if (this.gameObject.name == "Player1"){
            otherPlayer = GameObject.Find("Player2");
        } else {
            otherPlayer = GameObject.Find("Player1");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.LookAt(otherPlayer.transform);
        if( health <= 0){
            // set a global death flag to enter finished screen
            Debug.Log(this.gameObject.name +" is dead");
        }
    }

    void Update() {
        
    }

    public void TakeDamage(int dmg=1) {
        health -= dmg;
        Debug.Log(this.gameObject.name+" takes "+dmg+" damage!" );
    }

    public void CastFireballRight() {
        GameObject newFireball = Instantiate(fireball, transform.position, transform.rotation);
        StartCoroutine(DashRight());
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
