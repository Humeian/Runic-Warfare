using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceSpike : NetworkBehaviour
{
    public int damage = 1;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        Destroy(GetComponent<CapsuleCollider>(), 6f);
        Destroy(gameObject, 7f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            other.GetComponent<CharacterBehaviour>().TakeDamage(damage);
            if (other.GetComponent<PlayerBehaviour>() != null)
                other.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
            other.GetComponent<CharacterBehaviour>().TargetThrowPlayerBack(other.GetComponent<NetworkIdentity>().connectionToClient, 0.6f, 0, 40);
        }
    }
}
