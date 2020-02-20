using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class Lightning : NetworkBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject lightningExplosion;
    public GameObject owner;
    public GameObject target;
    public int damage = 1;

    [SyncVar]
    Vector3 startPos;

    [SyncVar]
    Vector3 endPos;

    public override void OnStartServer()
    {
        startPos = owner.transform.position;

        Debug.DrawRay(owner.transform.position, target.transform.position - owner.transform.position, Color.red, 5f);

        RaycastHit[] rayhits = Physics.RaycastAll(owner.transform.position, target.transform.position - owner.transform.position, 100f);
        IEnumerable<RaycastHit> rayhitsOrdered = rayhits.OrderBy(rh => rh.distance);

        foreach (RaycastHit rh in rayhitsOrdered) {
            Debug.DrawLine(rh.point, rh.point + Vector3.up * 50f, Color.green, 5f);
        }

        foreach (RaycastHit rh in rayhitsOrdered) {
            if (rh.collider.tag == "Shield") {
                rh.collider.GetComponent<Shield>().Break();
                endPos = rh.point;
                //lineRenderer.SetPosition(1, rh.point);
                break;
            }
            else if (rh.collider.tag == "Player") {
                rh.collider.GetComponent<PlayerBehaviour>().TakeDamage(damage);
                rh.collider.GetComponent<PlayerBehaviour>().TargetShowDamageEffects(rh.collider.GetComponent<NetworkIdentity>().connectionToClient);
                endPos = rh.point;
                //lineRenderer.SetPosition(1, rh.point);
                break;
            }
        }
    }

    void Start() {
        // Set line positions for client
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(4, endPos);

        // Add zigzags to line
        lineRenderer.SetPosition(1, Vector3.Lerp(startPos, endPos, 0.25f) + (Random.insideUnitSphere * 1f));
        lineRenderer.SetPosition(2, Vector3.Lerp(startPos, endPos, 0.5f) + (Random.insideUnitSphere * 1f));
        lineRenderer.SetPosition(3, Vector3.Lerp(startPos, endPos, 0.75f) + (Random.insideUnitSphere * 1f));

        GameObject newExplosion = Instantiate(lightningExplosion, endPos, transform.rotation);

        StartCoroutine(FadeOut());
    }

    public void SetOwner(GameObject o) {
        owner = o;
    }

    public void SetTarget(GameObject t) {
        target = t;
    }

    public IEnumerator FadeOut() {
        float alpha = 1f;
        while (true) {
            lineRenderer.widthMultiplier -= 0.05f;
            if (lineRenderer.widthMultiplier <= 0f) {
                Destroy(gameObject);
            }

            alpha -= 0.08f;
            lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, alpha);
            yield return new WaitForFixedUpdate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
