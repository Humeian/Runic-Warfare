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
        startPos = owner.transform.position + Vector3.up;
        Vector3 targetPos = target.transform.position + Vector3.up;
        Vector3 direction = targetPos - startPos;

        Debug.DrawRay(startPos, direction, Color.red, 5f);

        RaycastHit[] rayhits = Physics.RaycastAll(startPos, direction, 100f);
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
            else if (rh.collider.tag == "ArcanePulse") {
                startPos = rh.point;
                endPos = owner.transform.position + Vector3.up;
                owner.GetComponent<CharacterBehaviour>().TakeDamage(damage);
                owner.GetComponent<CharacterBehaviour>().TargetShowDamageEffects(owner.GetComponent<NetworkIdentity>().connectionToClient);
                break;
            }
            else if (rh.collider.tag == "Player") {
                rh.collider.GetComponent<CharacterBehaviour>().TakeDamage(damage);
                rh.collider.GetComponent<CharacterBehaviour>().TargetShowDamageEffects(rh.collider.GetComponent<NetworkIdentity>().connectionToClient);
                endPos = rh.point;
                //lineRenderer.SetPosition(1, rh.point);
                break;
            }
        }
    }

    void Start() {
        // Set line positions for client
        lineRenderer.positionCount = 9;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(8, endPos);

        // Add zigzags to line
        lineRenderer.SetPosition(1, Vector3.Lerp(startPos, endPos, 0.125f) + (Random.insideUnitSphere * 2f));
        lineRenderer.SetPosition(2, Vector3.Lerp(startPos, endPos, 0.25f) + (Random.insideUnitSphere * 2f));
        lineRenderer.SetPosition(3, Vector3.Lerp(startPos, endPos, 0.375f) + (Random.insideUnitSphere * 2f));
        lineRenderer.SetPosition(4, Vector3.Lerp(startPos, endPos, 0.5f) + (Random.insideUnitSphere * 2f));
        lineRenderer.SetPosition(5, Vector3.Lerp(startPos, endPos, 0.625f) + (Random.insideUnitSphere * 2f));
        lineRenderer.SetPosition(6, Vector3.Lerp(startPos, endPos, 0.75f) + (Random.insideUnitSphere * 2f));
        lineRenderer.SetPosition(7, Vector3.Lerp(startPos, endPos, 0.875f) + (Random.insideUnitSphere * 2f));

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
            if (lineRenderer.widthMultiplier <= 0.001f) {
                lineRenderer.widthMultiplier = 0f;
                Destroy(gameObject, 1.5f);
            }
            else {
                lineRenderer.widthMultiplier *= 0.9f;
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
