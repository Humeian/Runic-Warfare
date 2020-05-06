using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WindSlash : NetworkBehaviour
{
    public int damage = 1;

    [SyncVar]
    public GameObject otherPlayer;

    [SyncVar]
    public GameObject owner;

    public GameObject hitParticle;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        Destroy(GetComponent<BoxCollider>(), 0.6f);
        Destroy(gameObject, 1f);

        //otherPlayer = GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>().otherPlayer.GetComponent<Collider>();
    }

    void Start() {
        Color green = new Color(0.5f, 1f, 0.5f, 0.6f);
        if (owner.GetComponent<PlayerBehaviour>() != null)
            owner.GetComponent<PlayerBehaviour>().TargetPaintScreen(owner.GetComponent<NetworkIdentity>().connectionToClient, green);

        int random = Random.Range(0, 5);
        //print(random);
        GetComponents<AudioSource>()[random].Play();
    }

    public void SetTarget(GameObject g) {
        otherPlayer = g;
    }

    public void SetOwner(GameObject g) {
        owner = g;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = owner.transform.position + (transform.forward * 2f) + Vector3.up;
    }

    [ServerCallback]
    void OnTriggerStay(Collider other) {
        //Debug.Log("hit");
        if (owner != null && otherPlayer != null) {
            if (other.gameObject == otherPlayer) {
                //Debug.Log("testing");
                other.GetComponent<CharacterBehaviour>().TakeDamage(damage);
                if (owner.GetComponent<PlayerBehaviour>() != null)
                    other.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
                owner.GetComponent<CharacterBehaviour>().TargetThrowPlayerBack(owner.GetComponent<NetworkIdentity>().connectionToClient, 0.8f, 2, 40);
                owner.GetComponent<CharacterBehaviour>().TargetSetAnimTrigger(owner.GetComponent<NetworkIdentity>().connectionToClient, "WindSlashRecoil");
                ServerSpawnHit();
                Destroy(gameObject);
            } else if (other.tag == "Shield") {
                other.GetComponent<Shield>().Break();
                owner.GetComponent<CharacterBehaviour>().TargetThrowPlayerBack(owner.GetComponent<NetworkIdentity>().connectionToClient, 0.4f, 2, 40);
                owner.GetComponent<CharacterBehaviour>().TargetSetAnimTrigger(owner.GetComponent<NetworkIdentity>().connectionToClient, "WindSlashRecoil");
                ServerSpawnHit();
                Destroy(gameObject);
            }
        }
    }

    [Server]
    public void ServerSpawnHit() {
        GameObject newExplosion = Instantiate(hitParticle, transform.position, transform.rotation);
        NetworkServer.Spawn(newExplosion);
    }
}
