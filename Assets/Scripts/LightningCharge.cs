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
}
