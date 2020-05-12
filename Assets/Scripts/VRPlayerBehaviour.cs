using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VRPlayerBehaviour : PlayerBehaviour
{
    public GameObject castTarget;
    public NewNetworkManager networkManager;
    // Start is called before the first frame update
    // public void Start() {
    //     gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    //     red = new Color(1f, 0f, 0f, 1f);
    //     shields = new List<GameObject>();
    // }

    // Update is called once per frame
    // void Update()
    // {
        
    // }

    // public override void CastFireball(int horizontal, float horizSpeed) {
    //     GameObject newFireball = Instantiate(fireball, transform.position + Vector3.up, transform.rotation);
    //     newFireball.GetComponent<Fireball>().SetTarget(castTarget.transform.position);
    //     //newFireball.GetComponent<Fireball>().OfflineSetOwner(gameObject);
    //     //NetworkServer.Spawn(newFireball);
    //     // StopAirMomentum();
    //     // //transform.position += transform.TransformDirection(Vector3.right);
    //     // movingRight = horizontal;
    //     // speedRight = horizSpeed;
    //     // movingForward = 25;
    //     // speedForward = 0.4f;
    //     // if (horizontal > 0f)
    //     //     CmdSetAnimTrigger("FireballRight");
    //     // else 
    //     //     CmdSetAnimTrigger("FireballLeft");
    //     // CmdCastFireball(castTarget);
    // }

    // [Command]
    // public void CmdCastFireball(GameObject target) {
    //     GameObject newFireball = Instantiate(fireball, transform.position + Vector3.up, transform.rotation);
    //     newFireball.GetComponent<Fireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient, gameObject);
    //     newFireball.GetComponent<Fireball>().SetTarget(target.transform.position);
    //     NetworkServer.Spawn(newFireball);
    //     //StartCoroutine(DashRight());
    // }
}
