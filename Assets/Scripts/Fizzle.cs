using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Fizzle : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position += Vector3.up;
        Destroy(gameObject, 2f);
    }
}
