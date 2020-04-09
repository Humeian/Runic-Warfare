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
            other.GetComponent<PlayerBehaviour>().royalBurn += royalBurnRate * Time.deltaTime;
            if (other.GetComponent<PlayerBehaviour>().royalBurn >= 1f) {
                other.GetComponent<PlayerBehaviour>().TakeDamage(1);
                other.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
                other.GetComponent<PlayerBehaviour>().royalBurn = 0f;
            }
        }
    }
}
