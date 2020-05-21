using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceSpikeProjectile : NetworkBehaviour
{

    public GameObject iceSpike;
    public float speed;
    public float spikeDistance;

    public float maxDistanceFromCenter = 28f;

    public NetworkConnection owner;
    private Vector3 startPosition;
    private float distanceTravelled;
    private float nextSpikeDistance;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        distanceTravelled = 0f;
        nextSpikeDistance = spikeDistance;
        startPosition = transform.position;
        Destroy(gameObject, 4f);
    }

    public void SetOwner(NetworkConnection connection) {
        owner = connection;
    }

    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
        float newDistance = speed * Time.deltaTime;
        transform.position += transform.forward * newDistance;
        distanceTravelled += newDistance;

        //transform.rotation *= Quaternion.Euler(transform.forward * 90f * Time.deltaTime);

        if (distanceTravelled >= nextSpikeDistance) {
            GameObject newIceSpike = Instantiate(iceSpike, startPosition + (transform.forward * (nextSpikeDistance - 1f)) - Vector3.up, transform.rotation);
            NetworkServer.Spawn(newIceSpike);
            nextSpikeDistance += spikeDistance;
        }

        //check distance from center if it's out of bounds
        if (Vector3.Distance(transform.position, new Vector3(0f, transform.position.y, 0f)) > maxDistanceFromCenter) {
            Destroy(gameObject);
        }
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" &&
            (other.GetComponent<NetworkIdentity>().connectionToClient == null ? null : other.GetComponent<NetworkIdentity>().connectionToClient.ToString())
            != (owner == null ? null : owner.ToString())
        ) {
            other.GetComponent<CharacterBehaviour>().TakeDamage(1);
            other.GetComponent<CharacterBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
            other.GetComponent<CharacterBehaviour>().TargetThrowPlayerBack(other.GetComponent<NetworkIdentity>().connectionToClient, 0.6f, 0, 40);
            Destroy(gameObject);
        }
        else if (other.tag == "Shield") {
            other.GetComponent<Shield>().Break();
            Destroy(gameObject);
        }
        else if (other.tag == "ArcanePulse") {
            startPosition = transform.position;
            transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
            distanceTravelled = 0f;
            nextSpikeDistance = spikeDistance;
        }
    }
}
