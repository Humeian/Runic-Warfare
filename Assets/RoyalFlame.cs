using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoyalFlame : NetworkBehaviour
{
    public float royalBurnRate = 0.1f;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        Destroy(GetComponent<CapsuleCollider>(), 8f);
        Destroy(gameObject, 12f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ServerCallback]
    void OnTriggerStay(Collider other) {
        if (other.tag == "Player" && other.GetComponent<PlayerBehaviour>().health > 0) {
            other.GetComponent<CharacterBehaviour>().royalBurn += royalBurnRate * Time.deltaTime;
            if (other.GetComponent<CharacterBehaviour>().royalBurn >= 1f) {
                other.GetComponent<CharacterBehaviour>().TakeDamage(1);
                if (other.GetComponent<PlayerBehaviour>() != null)
                    other.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
                other.GetComponent<CharacterBehaviour>().royalBurn = 0f;
            }
        }
    }
}
