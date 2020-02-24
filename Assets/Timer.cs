using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    public float roundDuration = 60;
    private float timeRemaining;
    public Text timerText;
    //public bool pause = true;
    public bool started = false;

    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        //gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //timerText = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining = gameManager.timer;
        //print("time: " + timeRemaining);

        //string minutes = ((int) timeRemaining / 60).ToString();
        //string seconds = ((int) timeRemaining % 60).ToString();

        timerText.text = Mathf.Ceil(timeRemaining).ToString();

        // When the round has only a quarter left, turn the timer red
        if (timeRemaining <= roundDuration/6){
            timerText.color = new Color(255, 0, 0);
        }
        else {
            timerText.color = new Color(0, 0, 0);
        }


    }

    /*
    public void StartTimer() {
        pause = false;
    }

    public void StopTimer() {
        pause = true;
    }

    public void ResetTimer() {
        timeRemaining = roundDuration;
    }
    */
}
