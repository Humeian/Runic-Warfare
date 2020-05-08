using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class ScreenShakeVREffect : MonoBehaviour
{
public static ScreenShakeVREffect instance;

public static float Factor;

public float shakeMagnitude = 0.1f;
public float shakeFrequency = 20f;

private float shakeVal;
float shakeCumulation;

public bool debug = false;

public XRController test;

public class ShakeEvent
{
    public float magnitude;
    public float length;

    private float exponent;

    public bool finished { get { return time >= length; } }
    public float currentStrength { get { return magnitude * Mathf.Clamp01(1 - time / length); } }

    public ShakeEvent(float mag, float len, float exp = 2)
    {
        magnitude = mag;
        length = len;
        exponent = exp;
    }

    private float time;

    public void Update(float deltaTime)
    {
        time += deltaTime;
    }
}

public List <ShakeEvent> activeShakes = new List<ShakeEvent>();

void Awake()
{
    instance = this;
}

private void OnEnable()
{
    Awake();
}

public void Shake(float magnitude, float length, float exponent = 2)
{
    //print("Shake");
    activeShakes.Add(new ShakeEvent(magnitude, length, exponent));
}

public static void TriggerShake(float magnitude, float length, float exponent = 2)
{
    if(instance == null)
    {
        Debug.LogWarning("No ScreenShakeVR Component in scene. Add one to a camera.");
    }
    else
    {
        instance.Shake(magnitude, length, exponent);
    }
}

private void Update()
{
    if (debug) {
        test.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool goDown);
        if (goDown) Shake(0.7f, 1.0f);
    }

    shakeCumulation = 0;
    for (int i = activeShakes.Count - 1; i >= 0; i--)
    {
        activeShakes[i].Update(Time.deltaTime);
        shakeCumulation += activeShakes[i].currentStrength;
        if (activeShakes[i].finished)
        {
            activeShakes.RemoveAt(i);
        }
    }

    if (shakeCumulation > 0)
    {
        shakeVal = Mathf.PerlinNoise(Time.time * shakeFrequency, 10.234896f) * shakeCumulation * shakeMagnitude;
    }
    else
    {
        shakeVal = 0;
    }

    Factor = shakeVal;
}
}