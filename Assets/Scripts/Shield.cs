using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Shield : NetworkBehaviour
{
    public int health = 1;
    public bool reflectsSpells = false;
    private Vector3 finalPosition;
    private Vector3 startPosition;
    private MaterialPropertyBlock mpb;
    private MeshRenderer renderer;

    public CharacterBehaviour owner;

    public AudioClip emergeClip;
    public AudioClip breakClip;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        finalPosition = transform.position;
        transform.position += Vector3.down * 4f;
        startPosition = transform.position;
        StartCoroutine(RiseUp());
    }

    void Start() {
        GetComponent<AudioSource>().clip = emergeClip;
        GetComponent<AudioSource>().Play();
        mpb = new MaterialPropertyBlock();
        renderer = GetComponent<MeshRenderer>();
        StartCoroutine(FadeIn());
    }

    IEnumerator RiseUp() {
        float duration = 0.3f;
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
            Color newColor = new Color(0.125f, 0.25f, 0.75f, (currentTime / duration) * 3f);
            mpb.SetColor("_Color", newColor);
            renderer.SetPropertyBlock(mpb, 0);
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        
    }

    IEnumerator FadeOut() {
        Destroy(GetComponent<MeshCollider>());
        GetComponent<AudioSource>().Play();
        float duration = 0.3f;
        float currentTime = 0f;
        while (currentTime < duration) {
            float remainingTime = (duration - currentTime);
            Color newColor = new Color(0.125f, 0.25f, 0.75f, (duration - currentTime) * 6f);
            Vector3 emission = new Vector3(remainingTime * 15f, remainingTime * 30f, remainingTime * 90f);
            mpb.SetColor("_Color", newColor);
            mpb.SetVector("_EmissionColor", emission);
            renderer.SetPropertyBlock(mpb, 0);
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        // Destroy(transform.GetChild(2).gameObject);
        // Destroy(transform.GetChild(1).gameObject);
        // Destroy(transform.GetChild(0).gameObject);
        Destroy(GetComponent<MeshRenderer>());
        Destroy(gameObject, 2f);
    }

/*
    IEnumerator ClientFadeOut() {
        Destroy(GetComponent<BoxCollider>());
        float duration = 0.3f;
        float currentTime = 0f;
        while (currentTime < duration) {
            float remainingTime = (duration - currentTime);
            Color newColor = new Color(0.75f, 0.5f, 0.75f, (duration - currentTime) * 6f);
            Vector3 emission = new Vector3(remainingTime * 2f, remainingTime * 2f, remainingTime * 2f);
            mpb.SetColor("_Color", newColor);
            renderer.SetPropertyBlock(mpb, 0);
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        // Shield will be destroyed on server-side.
    }
*/

    public void Break() {
        ServerBreak();
        RpcBreak();
    }

    [Server] 
    public void ServerBreak() {
        StartCoroutine(FadeOut());
    }

    [ClientRpc]
    public void RpcBreak() {
        StartCoroutine(FadeOut());
        GetComponent<AudioSource>().clip = breakClip;
        GetComponent<AudioSource>().Play();
    }
}
