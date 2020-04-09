using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Royalfireball : NetworkBehaviour
{

    private Vector3 startPosition;
    private Vector3 endPosition;
    public float travelTime;
    private float startHeight;
    public float maxHeight;

    private float startTime;
    private Vector3 target;

    public GameObject royalFire;

    public NetworkConnection owner;
    private bool wasReflected = false;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        startPosition = transform.position + Vector3.up;
        startTime = Time.time;
        startHeight = transform.position.y;

        StartCoroutine(TravelToDestination());
    }

    public void SetOwner(NetworkConnection connection) {
        owner = connection;
    }

    public void SetTarget(Vector3 p) {
        endPosition = p;
    }

    IEnumerator TravelToDestination() {
        float verticalSpeed = maxHeight;
        float vertical = startHeight;
        while (true) {
            float currentTime = (Time.time - startTime) / travelTime;
            Vector3 horizontal = Vector3.Lerp(startPosition, endPosition, currentTime);
            vertical += verticalSpeed * Time.deltaTime;
            verticalSpeed -= maxHeight * (2.5f / travelTime) * Time.deltaTime;

            transform.position = new Vector3(horizontal.x, vertical, horizontal.z);

            /*
            if (currentTime > 1f) {
                GameObject newExplosion = Instantiate(fireballExplosion, transform.position, Quaternion.identity);
                NetworkServer.Spawn(newExplosion);
                Destroy(gameObject);
            }
            */

            yield return new WaitForEndOfFrame();
        }
    }

    void OnTriggerEnter(Collider other) {
        // Only hits the arena
        if (other.tag == "Arena") {
            ServerSpawnExplosion();
        } else if (other.tag == "Shield") {
            other.GetComponent<Shield>().Break();
            Destroy(gameObject);
        }
    }

    [Server]
    public void ServerSpawnExplosion() {
        GameObject newExplosion = Instantiate(royalFire, new Vector3(transform.position.x, 0f, transform.position.z), Quaternion.identity);
        NetworkServer.Spawn(newExplosion);
        Destroy(gameObject);
    }
}
