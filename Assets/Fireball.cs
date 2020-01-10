using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{

    private Vector3 startPosition;
    private Vector3 endPosition;
    public float travelTime;
    private float startHeight;
    public float maxHeight;

    private float startTime;

    public GameObject fireballExplosion;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startTime = Time.time;
        startHeight = transform.position.y;
        endPosition = GameObject.Find("Player2").transform.position;
        StartCoroutine(TravelToDestination());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    IEnumerator TravelToDestination() {
        while (true) {
            float currentTime = (Time.time - startTime) / travelTime;
            Vector3 horizontal = Vector3.Lerp(startPosition, endPosition, currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * maxHeight) + startHeight;

            transform.position = new Vector3(horizontal.x, vertical, horizontal.z);

            if (currentTime > 1f) {
                Instantiate(fireballExplosion, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
