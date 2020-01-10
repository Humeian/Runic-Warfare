using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    private GameObject otherPlayer;
    public float dashSpeed;
    public float dashHeight;
    private float playerHeight = 1f;

    public GameObject fireball;

    // Start is called before the first frame update
    void Start()
    {
        otherPlayer = GameObject.Find("Player2");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.LookAt(otherPlayer.transform);
    }

    public void CastFireballRight() {
        GameObject newFireball = Instantiate(fireball, transform.position, transform.rotation);
        StartCoroutine(DashRight());
    }

    void DashLeft() {

    }

    IEnumerator DashRight() {
        float duration = 0.6f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f) {
            transform.position += transform.right * dashSpeed * Time.deltaTime;
            
            currentTime = (Time.time - startTime) / duration;
            print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * dashHeight) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
        
    }
}
