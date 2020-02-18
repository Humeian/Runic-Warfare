using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Fireball : NetworkBehaviour
{

    private Vector3 startPosition;
    private Vector3 endPosition;
    public float travelTime;
    private float startHeight;
    public float maxHeight;

    private float startTime;
    private Vector3 target;

    public GameObject fireballExplosion;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        startPosition = transform.position;
        startTime = Time.time;
        startHeight = transform.position.y;

        StartCoroutine(TravelToDestination());
    }

    public void SetTarget(Vector3 p) {
        endPosition = p;
    }

    IEnumerator TravelToDestination() {
        while (true) {
            float currentTime = (Time.time - startTime) / travelTime;
            Vector3 horizontal = Vector3.Lerp(startPosition, endPosition, currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * maxHeight) + startHeight;

            transform.position = new Vector3(horizontal.x, vertical, horizontal.z);

            if (currentTime > 1f) {
                GameObject newExplosion = Instantiate(fireballExplosion, transform.position, Quaternion.identity);
                NetworkServer.Spawn(newExplosion);
                Destroy(gameObject);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    /*
    void OnTriggerEnter(Collider other) {
        // Should not hit the caster
        if (GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>().currentPlayer.GetComponent<Collider>() != other ){
            Instantiate(fireballExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
    */


}
