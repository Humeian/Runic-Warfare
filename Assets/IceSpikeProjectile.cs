using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceSpikeProjectile : NetworkBehaviour
{

    public GameObject iceSpike;
    public float speed;
    public float spikeDistance;

    public float maxDistanceFromCenter = 25f;

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

    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
        float newDistance = speed * Time.deltaTime;
        transform.position += transform.forward * newDistance;
        distanceTravelled += newDistance;

        if (distanceTravelled >= nextSpikeDistance) {
            GameObject newIceSpike = Instantiate(iceSpike, startPosition + (transform.forward * nextSpikeDistance) - Vector3.up, transform.rotation);
            NetworkServer.Spawn(newIceSpike);
            nextSpikeDistance += spikeDistance;
        }

        //check distance from center if it's out of bounds
        if (Vector3.Distance(transform.position, new Vector3(0f, transform.position.y, 0f)) > maxDistanceFromCenter) {
            Destroy(gameObject);
        }
    }
}
