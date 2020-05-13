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

    public NetworkConnection owner;
    public GameObject ownerGO;
    private bool wasReflected = false;
    public bool playerThrown; // TODO

    private float verticalSpeed;
    private float vertical;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        startPosition = transform.position + Vector3.up;
        startTime = Time.time;
        startHeight = transform.position.y;

        StartCoroutine(TravelToDestination());
    }

    // public void Start () {
    //     startPosition = transform.position + Vector3.up;
    //     startTime = Time.time;
    //     startHeight = transform.position.y;

    //     StartCoroutine(TravelToDestination());
    // }

    public void SetOwner(NetworkConnection conn, GameObject go) { SetOwner(conn, go, false); }
    public void SetOwner(NetworkConnection connection, GameObject go, bool playerOwned) {

        Debug.Log(connection);
        owner = connection;
        ownerGO = go;
        playerThrown = playerOwned;
        if (playerThrown) travelTime -= 0.85f;
        Debug.Log("traveltime "+travelTime);
    }

    public void SetTarget(Vector3 p) {
        endPosition = p;
    }

    IEnumerator TravelToDestination() {
        verticalSpeed = maxHeight;
        vertical = startHeight;
        while (true) {
            float currentTime = (Time.time - startTime) / travelTime;
            Vector3 horizontal = Vector3.Lerp(startPosition, endPosition, currentTime);
            vertical += verticalSpeed * Time.deltaTime;
            verticalSpeed -= maxHeight * (2f / travelTime) * Time.deltaTime;

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
        //print("ownerGO: " + ownerGO.ToString());
        //print("other: " + other.ToString());
        if (other.GetComponent<NetworkIdentity>() == null && other.tag != "BodyPart") {
            //print(other.name);
            //print("explode here1");
            ServerSpawnExplosion();
        }
        else if (other.GetComponent<NetworkIdentity>() != null && other.tag != "RoyalFire") {
            if (other.tag == "ArcanePulse") {
                //doesn't reflect more than once... kind of a cop out
                if (wasReflected == true) {
                    Destroy(gameObject);
                }

                startPosition = transform.position;
                startTime = Time.time;
                //startHeight = transform.position.y;
                verticalSpeed = maxHeight;

                if (owner != null)
                    endPosition = owner.identity.transform.position;
                else
                    endPosition = ownerGO.transform.position;
                SetOwner(other.GetComponent<ArcanePulse>().owner.GetComponent<NetworkIdentity>().connectionToClient, other.gameObject, !playerThrown);
                wasReflected = true;
            }
            else if (other.GetComponent<NetworkIdentity>().connectionToClient == null) {
                
                if (other.GetComponent<AIBehaviour>() == null && other != ownerGO && !other.transform.IsChildOf(ownerGO.transform))
                {
                    ServerSpawnExplosion();
                }
            }
            //if other object has an identity and wasn't the owner, explode
            else if ((other.GetComponent<NetworkIdentity>().connectionToClient.ToString() != (owner == null ? null : owner.ToString()))) {
                //Debug.Log("HERE");
                ServerSpawnExplosion();
            }
            //if fireball was reflected and identity is the owner, explode
            else if (wasReflected == true && other.GetComponent<NetworkIdentity>().connectionToClient.ToString() == owner.ToString()) {
                ServerSpawnExplosion();
            }
                
        }
        
    }

    [Server]
    public void ServerSpawnExplosion() {
        GameObject newExplosion = Instantiate(fireballExplosion, transform.position, Quaternion.identity);
        newExplosion.GetComponent<FireballExplosion>().SetOwner(ownerGO, playerThrown);
        NetworkServer.Spawn(newExplosion);
        Destroy(gameObject);
    }

    public void OfflineSpawnExplosion() {
        GameObject newExplosion = Instantiate(fireballExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
