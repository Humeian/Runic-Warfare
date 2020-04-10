using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ArcanePulse : NetworkBehaviour
{
	public float MaxDiameter = 2.5f;
	public float expansionTime;
	public GameObject owner;

	private Vector3 startScale;
	private float startTime;

    public override void OnStartServer()
    {
        //startScale = transform.localScale;
        //startTime = Time.time;

        //StartCoroutine(Expand());
        Destroy(GetComponent<SphereCollider>(), 1.4f);
        Destroy(gameObject, 1.7f);
    }

    public void SetOwner(GameObject o) {
    	owner = o;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        //don't collide with ragdolls
        if (other.tag == "BodyPart") {

        } else if (other.tag == "Player" && (other.GetComponent<NetworkIdentity>().connectionToClient != owner.GetComponent<NetworkIdentity>().connectionToClient)) {
            print("hit");
            print(other.GetComponent<NetworkIdentity>().connectionToClient.ToString());
            print(owner.ToString());
            other.GetComponent<PlayerBehaviour>().TakeDamage(1);
            other.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(other.GetComponent<NetworkIdentity>().connectionToClient);
            other.GetComponent<PlayerBehaviour>().TargetThrowPlayerBack(other.GetComponent<NetworkIdentity>().connectionToClient, 0.6f, 0, 40);
        } else if (other.tag == "Shield") {
            other.GetComponent<Shield>().Break();
        }
    }

    /*
    IEnumerator Expand() {
    	while (true) {
    		// transform.scale = new Vector3(tranform.scale.x + expansionSpeed);
            float currentTime = (Time.time - startTime) / expansionTime;
            transform.localScale = Vector3.Lerp(startScale, new Vector3(MaxDiameter,MaxDiameter,MaxDiameter), currentTime);;
            Debug.Log(transform.localScale);
            if (transform.localScale.x >= MaxDiameter) {
                // GameObject newExplosion = Instantiate(fireballExplosion, transform.position, Quaternion.identity);
                // NetworkServer.Spawn(newExplosion);
                Destroy(gameObject);
            }

            yield return new WaitForEndOfFrame();
        }
    }
    */
}
