using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FireballExplosion : NetworkBehaviour
{
    public int damage = 0;

    

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        Destroy(GetComponent<SphereCollider>(), 0.05f);
        Destroy(gameObject, 1f);
    }

    public void Start() {
        Destroy(GetComponent<SphereCollider>(), 0.05f);
        Destroy(gameObject, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Server call only, no call on client.
    //ServerCallback is similar to Server but doesn't generate a warning when called on client.
    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            
            other.GetComponent<CharacterBehaviour>().TakeDamage(damage);
            // if (other.GetComponent<PlayerBehaviour>() != null) {
            //     other.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
            // }
            Destroy(GetComponent<SphereCollider>(), 0);
        } else if (other.tag == "Shield") {
            other.GetComponent<Shield>().Break();
            Destroy(GetComponent<SphereCollider>(), 0);
        }
    }
}


