using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LightningCharge : NetworkBehaviour
{
    [SyncVar]
    public GameObject owner;

    // Start is called before the first frame update
    void Start()
    {
        transform.position += Vector3.up;
        //StartCoroutine(PaintScreenYellow());
        Destroy(gameObject, 2f);
    }

    public void SetOwner(GameObject o) {
        owner = o;
    }
    
    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator PaintScreenYellow() {
        yield return new WaitForSeconds(0.45f);
        Color yellow = new Color(1f, 1f, 0.5f, 0.6f);
        if (owner.GetComponent<PlayerBehaviour>() != null)
            owner.GetComponent<PlayerBehaviour>().TargetPaintScreen(owner.GetComponent<NetworkIdentity>().connectionToClient, yellow);
    }
}
