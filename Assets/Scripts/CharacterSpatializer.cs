using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpatializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // RaycastHit hit;
            // if (Physics.Raycast(transform.position, transform.forward, out hit, 100.0f)) {
            //     Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow);
            //     Debug.Log("Hit Object: "+hit.collider.name+"  Tag: "+hit.collider.tag);

            //     switch (hit.collider.tag) {
            //         case "Player":
            //             playerDistance = hit.distance;
            //             iceSpikesDistance = 0f;
            //             shieldDistance = 0f;
            //             break;
            //         case "IceSpikes":
            //             iceSpikesDistance = hit.distance;
            //             shieldDistance = 0f;
            //             break;
            //         case "Shield":
            //             shieldDistance = hit.distance;
            //             iceSpikesDistance = 0f;
            //             break;
            //     }
            // }
    }
}
