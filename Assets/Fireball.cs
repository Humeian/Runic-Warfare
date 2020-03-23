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
    private GameObject owner;

    public GameObject fireballExplosion;

    public NetworkConnection owner;

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

    public void SetOwner(GameObject o) {
        owner = o;
    }

    IEnumerator TravelToDestination() {
        while (true) {
            float currentTime = (Time.time - startTime) / travelTime;
            Vector3 horizontal = Vector3.Lerp(startPosition, endPosition, currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * maxHeight) + startHeight;

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
        // Should not hit the caster
        // This is pretty messy - reminder to clean up afterwards
        // print("owner: " + owner.ToString());
        if (other.GetComponent<NetworkIdentity>() == null && other.tag != "BodyPart") {
            ServerSpawnExplosion();
        }
        else if (other.GetComponent<NetworkIdentity>() != null) {
            if (other.GetComponent<NetworkIdentity>().connectionToClient == null) {
                ServerSpawnExplosion();
            }
            else if (other.GetComponent<NetworkIdentity>().connectionToClient.ToString() != owner.ToString())
                ServerSpawnExplosion();
        }
        if (other.name == "ArcanePulse") {
            startPosition = transform.position;
            startTime = Time.time;
            startHeight = transform.position.y;

            endPosition = owner.transform.position;
        }
    }

    [Server]
    public void ServerSpawnExplosion() {
        GameObject newExplosion = Instantiate(fireballExplosion, transform.position, Quaternion.identity);
        NetworkServer.Spawn(newExplosion);
        Destroy(gameObject);
    }
}
