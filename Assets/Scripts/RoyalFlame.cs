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
        Destroy(GetComponent<CapsuleCollider>(), 10f);
        Destroy(gameObject, 14f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ServerCallback]
    void OnTriggerStay(Collider other) {
        if (other.tag == "Player" && other.GetComponent<CharacterBehaviour>().health > 0) {
            other.GetComponent<CharacterBehaviour>().royalBurn += royalBurnRate * Time.deltaTime;
            if (other.GetComponent<CharacterBehaviour>().royalBurn >= 1f) {
                RpcPlaySound();
                other.GetComponent<CharacterBehaviour>().TakeDamage(1);
                other.GetComponent<CharacterBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
                other.GetComponent<CharacterBehaviour>().royalBurn = 0f;
            }
        }
    }

    [ClientRpc]
    void RpcPlaySound() {
        GetComponents<AudioSource>()[1].Play();
    }
}
