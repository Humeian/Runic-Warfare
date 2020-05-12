using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLink : MonoBehaviour
{
    public GameObject menuCube;
    public bool isGripped = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isGripped)
            menuCube.transform.rotation = transform.rotation * Quaternion.Euler(0, 90, 0);
    }

    // public void setStartRotation(Transform t) {
    //     startRotation = t.rotation;
    //     transform.rotation = Quaternion.Euler(0,0,0);
    // }

    public void toggleGripped(bool isgr) {
        isGripped = isgr;
    }
}
