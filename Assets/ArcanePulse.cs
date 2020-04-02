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
        Destroy(gameObject, 1f);
    }

    public void SetOwner(GameObject o) {
    	owner = o;
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
