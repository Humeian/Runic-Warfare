using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    public float roundDuration = 120;
    private float timeRemaining;
    Text timerText;
    public bool pause = true;
    public bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        timeRemaining = roundDuration;
        timerText = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!pause) {
            timeRemaining -= Time.deltaTime;
        }

        string minutes = ((int) timeRemaining / 60).ToString();
        string seconds = ((int) timeRemaining % 60).ToString();

        timerText.text = minutes + ": " + seconds;

        // When the round has only a quarter left, turn the timer red
        if (timeRemaining <= roundDuration/4){
            timerText.color = new Color(255, 0, 0);
        }
    }

    public void StartTimer() {
        pause = false;
    }

    public void StopTimer() {
        pause = true;
    }

    public void ResetTimer() {
        timeRemaining = roundDuration;
    }
}
