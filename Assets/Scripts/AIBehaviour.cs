using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AIBehaviour : NetworkBehaviour
{
    //public bool isAIPlayer = false;
    public bool tutorialMode = false;
    public bool AIAttacks = true;

    public GameObject AIplayer;

    void Start()
    {
        AIplayer = GetComponent<PlayerBehaviour>().otherPlayer;
        Debug.Log(AIplayer);
        //
    }

    public void activateAI()
    {
        StartCoroutine(CastRandom());
    }

    IEnumerator CastRandom()
    {
        while(true)
        {
            Debug.Log("CastRandom");
            if (!tutorialMode && AIAttacks && player != null)
            {
                switch (Random.Range(0, 7))
                {
                    case 0:
                        int direction = Random.Range(0, 3);
                        if (direction == 0)
                            player.CastFireball(25, 1f);
                        if (direction == 1)
                            player.CastFireball(-25, 1f);
                        else
                            player.CastFireball(0, 0f);
                        break;
                    case 1:
                        player.CastShieldBack();
                        break;
                    case 2:
                        player.CastWindForward();
                        break;
                    case 3:
                        player.CastLightningNeutral();
                        break;
                    case 4:
                        player.CastArcanePulse();
                        break;
                    case 5:
                        player.CastIceSpikes();
                        break;
                    case 6:
                        // Cast Royal Fire
                        break;
                    default:
                        Debug.Log("CastFailed");
                        break;

                }
            }
            yield return new WaitForSeconds(3);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!tutorialMode)
        {
             
        }
    }
}
