using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public int health = 1;
    public bool reflectsSpells = false;
    private Vector3 finalPosition;
    private Vector3 startPosition;
    private MaterialPropertyBlock mpb;
    private MeshRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        finalPosition = transform.position;
        transform.position += Vector3.down * 4f;
        startPosition = transform.position;
        StartCoroutine(RiseUp());

        mpb = new MaterialPropertyBlock();
        renderer = GetComponent<MeshRenderer>();
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    IEnumerator RiseUp() {
        float duration = 0.4f;
        float currentTime = 0f;
        while (currentTime < duration) {
            float lerp = currentTime / duration;
            float vertical = Mathf.Sin(lerp * Mathf.PI * 0.5f) * 4f;
            transform.position = startPosition + Vector3.up * vertical;
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        transform.position = finalPosition;
    }

    IEnumerator FadeIn() {
        float duration = 0.3f;
        float currentTime = 0f;
        while (currentTime < duration) {
            Color newColor = new Color(0.75f, 0.5f, 0.75f, (currentTime / duration) * 3f);
            mpb.SetColor("_Color", newColor);
            renderer.SetPropertyBlock(mpb, 0);
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        
    }

    IEnumerator FadeOut() {
        float duration = 0.3f;
        float currentTime = 0f;
        while (currentTime < duration) {
            Color newColor = new Color(0.75f, 0.5f, 0.75f, (duration / currentTime) * 3f);
            mpb.SetColor("_Color", newColor);
            renderer.SetPropertyBlock(mpb, 0);
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        
    }

    public void Break() {
        StartCoroutine(FadeOut());
    }
}
