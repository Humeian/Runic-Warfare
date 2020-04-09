using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LightningExplosion : NetworkBehaviour
{
    // Doesn't deal damage; damage is dealt directly by Lightning.

    void Start() {
        Destroy(gameObject, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


