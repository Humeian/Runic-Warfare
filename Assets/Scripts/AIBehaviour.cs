using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AIBehaviour : CharacterBehaviour
{

    private int lightningChargesNeeded = 3;

    public Animator animator;

    private float playerHeight = 1f;

    private bool onGround = true;

    int movingRight = 0;
    int movingForward = 0;
    int movingUp = 0;
    float speedRight = 0f;
    float speedForward = 0f;
    float speedUp = 0f;

    private bool firstHit = true;

    bool comingDown = false;

    public bool tutorialMode = false;
    public bool AIAttacks = true;

    // After using Pulse, number of times spells casted in the air will stop air momentum.
    private int stopMomentumCharges = 0;

    private string lastPlayerSpell;
    private float counterChance;
    private float counterSpell;

    // Start with hard AI
    public string difficulty = "Hard";

    // Percentage chance that the AI will counter the last spell cast
    public float chanceToCounter = 0.66f;
    private int minCastTime = 2; // minimum seconds between casts
    private int maxCastTime = 7; // maximum seconds between casts

    // OnServerAuthority: called when GameObject is created on the client with authority.
    // By default, clients only have authority over their Player object and nothing else. 
    void Start()
    {
        StartCoroutine(Movement());
        StartCoroutine(CastRandom());
        shields = new List<GameObject>();
        animator = GetComponent<Animator>();
    }

    public override void RpcResetUI()
    {
        shields.Clear();

        //reset momentum
        movingForward = 0;
        movingRight = 0;
        movingUp = 0;
    }

    void FixedUpdate()
    {
        if (otherPlayer != null)
        {
            transform.LookAt(otherPlayer.transform);
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

            lastPlayerSpell = otherPlayer.GetComponent<PlayerBehaviour>().lastSpellCast;
        }

        if (health > 0 && GetComponent<Animator>().runtimeAnimatorController == null)
        {
            GetComponent<Animator>().runtimeAnimatorController = controller;
            //GetComponent<CapsuleCollider>().enabled = !GetComponent<CapsuleCollider>().enabled;
        }

        else if (health <= 0 && GetComponent<Animator>().runtimeAnimatorController != null)
        {
            // set a global death flag to enter finished screen
            //Debug.Log(this.gameObject.name +" is dead");
            //GetComponent<CapsuleCollider>().enabled = !GetComponent<CapsuleCollider>().enabled;
            GetComponent<Animator>().runtimeAnimatorController = null;

            // Stop the timer
            //timer.StopTimer();
        }

        if ( otherPlayer == null ) {
            Debug.Log("Null player");
            otherPlayer = GameObject.Find("TestPlayer(Clone)");
        }
    }

    IEnumerator Movement()
    {
        while (true)
        {
            float distanceFromCenter = DistanceToCenter();

            if (distanceFromCenter < 24)
            {
                if (movingRight != 0)
                {
                    transform.position += transform.right * Time.deltaTime * (movingRight * speedRight);

                    if (movingRight < 0) movingRight++;
                    else if (movingRight > 0) movingRight--;
                }

                if (movingForward != 0)
                {
                    transform.position += transform.forward * Time.deltaTime * (movingForward * speedForward);

                    if (movingForward < 0) movingForward++;
                    else if (movingForward > 0) movingForward--;
                }
            }
            else
            {
                movingRight = 0;
                movingForward = 0;
                transform.position -= (transform.position - GameObject.Find("CenterMark").transform.position).normalized;
            }

            if (!onGround)
            {
                //fall faster if falling down and no spell has been used -- easier to time reflect pulse
                if (speedUp < 0 && stopMomentumCharges > 0)
                {
                    if (!comingDown && hasAuthority)
                    {
                        SetAnimTrigger("PulseDown");
                        comingDown = true;
                    }
                    transform.position += transform.up * speedUp * 250f * Time.deltaTime;
                }
                else
                {
                    transform.position += transform.up * speedUp * 60f * Time.deltaTime;
                }

                speedUp -= Time.deltaTime;

                if (transform.position.y <= 0f)
                {
                    transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
                    onGround = true;
                    if (stopMomentumCharges > 0)
                    {
                        GameObject newPulse = Instantiate(arcanePulse, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
                        newPulse.GetComponent<ArcanePulse>().SetOwner(gameObject);
                        NetworkServer.Spawn(newPulse);
                    }
                    stopMomentumCharges = 0;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public void ResetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void CastFireball(int horizontal, float horizSpeed)
    {
        print("AI Cast Fireball");
        StopAirMomentum();
        //transform.position += transform.TransformDirection(Vector3.right);
        movingRight = horizontal;
        speedRight = horizSpeed;
        movingForward = 25;
        speedForward = 0.5f;
        if (horizontal > 0f)
            SetAnimTrigger("FireballRight");
        else
            SetAnimTrigger("FireballLeft");
            
        GameObject newFireball = Instantiate(fireball, transform.position + Vector3.up, transform.rotation);
        newFireball.GetComponent<Fireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient, gameObject);
        newFireball.GetComponent<Fireball>().SetTarget(otherPlayer.transform.position);
        NetworkServer.Spawn(newFireball);
    }


    public void SetAnimTrigger(string s)
    {
        animator.SetTrigger(s);
    }

    public void CastWindForward()
    {
        StopAirMomentum();
        movingForward = 20;
        speedForward = 2f;
        this.SetAnimTrigger("WindSlash");
        GameObject newWindSlash = Instantiate(windslash, transform.position + (transform.forward * 2f) + Vector3.up, transform.rotation);
        newWindSlash.GetComponent<WindSlash>().SetOwner(gameObject);
        newWindSlash.GetComponent<WindSlash>().SetTarget(otherPlayer);
        NetworkServer.Spawn(newWindSlash);
    }

    public void CastShieldBack()
    {
        StopAirMomentum();
        movingForward = -30;
        speedForward = 0.4f;
        SetAnimTrigger("ShieldBack");
        GameObject newShield = Instantiate(shield, transform.position + Vector3.up, transform.rotation * Quaternion.Euler(90f, 0f, 90f));
        NetworkServer.Spawn(newShield);

        shields.Add(newShield);
        if (shields.Count > maxShields)
        {
            GameObject oldShield = shields[0];
            shields.RemoveAt(0);
            Destroy(oldShield);
        }
    }

    public void CastLightningNeutral()
    {
        StopAirMomentum();
        GameObject newChargeEffect = Instantiate(lightningChargeObj, transform.position + Vector3.up, transform.rotation);
        newChargeEffect.GetComponent<LightningCharge>().SetOwner(gameObject);
        NetworkServer.Spawn(newChargeEffect);
        lightningCharge++;
        if (lightningCharge == lightningChargesNeeded)
        {
            StartCoroutine(WaitForLightning());
            lightningCharge = 0;
        }
    }

    public void CastArcanePulse()
    {
        onGround = false;
        speedUp = 0.6f;
        stopMomentumCharges = 1;
        SetAnimTrigger("ArcanePulse");
        GameObject newPulse = Instantiate(arcanePulse, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        newPulse.GetComponent<ArcanePulse>().SetOwner(gameObject);
        NetworkServer.Spawn(newPulse);
    }

    public void CastIceSpikes()
    {
        onGround = false;
        speedUp = 0.6f;
        movingForward = -80;
        speedForward = 0.1f;
        //do not fall faster
        stopMomentumCharges = 0;
        SetAnimTrigger("ShieldBack");
        GameObject newIceSpikes = Instantiate(iceSpikeProjectile, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        newIceSpikes.GetComponent<IceSpikeProjectile>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient);
        NetworkServer.Spawn(newIceSpikes);
    }

    public void CastRoyalFire(int horizontal, float horizSpeed)
    {
        onGround = false;
        speedUp = 0.4f;
        stopMomentumCharges = 0;
        movingRight = horizontal;
        speedRight = horizSpeed;
        if (horizontal > 0f)
            SetAnimTrigger("FireballRight");
        else
            SetAnimTrigger("FireballLeft");
        GameObject newRoyalFireball = Instantiate(royalFireball, transform.position + Vector3.up, transform.rotation);
        newRoyalFireball.GetComponent<Royalfireball>().SetOwner(GetComponent<NetworkIdentity>().connectionToClient);
        newRoyalFireball.GetComponent<Royalfireball>().SetTarget(otherPlayer.transform.position);
        NetworkServer.Spawn(newRoyalFireball);
    }

    public void CastFizzle()
    {
        GameObject newFizzle = Instantiate(fizzle, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        NetworkServer.Spawn(newFizzle);
    }

    private void StopAirMomentum()
    {
        if (stopMomentumCharges > 0)
        {
            stopMomentumCharges--;
            if (speedUp < 0)
            {
                speedUp = 0.2f;
            }
        }
    }

    IEnumerator WaitForLightning()
    {
        yield return new WaitForSeconds(0.45f);
        GameObject newLightning = Instantiate(lightning, transform.position + Vector3.up, transform.rotation);
        newLightning.GetComponent<Lightning>().SetOwner(gameObject);
        newLightning.GetComponent<Lightning>().SetTarget(otherPlayer);
        NetworkServer.Spawn(newLightning);
    }

    public override void TargetThrowPlayerBack(NetworkConnection target, float horizontal, float vertical, int duration)
    {
        movingForward = -duration;
        speedForward = horizontal;
        //speedUp = vertical;
        //StartCoroutine(ThrowBack(horizontal, vertical, duration));
    }

    public override void TargetSetAnimTrigger(NetworkConnection target, string s)
    {
        animator.SetTrigger(s);
    }

    IEnumerator DashLeft()
    {
        float duration = 0.6f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f)
        {
            transform.position -= transform.right * dashSpeed * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * dashHeight) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DashRight()
    {
        float duration = 0.6f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f)
        {
            transform.position += transform.right * dashSpeed * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * dashHeight) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DashBack()
    {
        float duration = 0.4f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f)
        {
            transform.position -= transform.forward * dashSpeed * 0.8f * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * dashHeight * 0.5f) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ThrowBack(float throwHorizontal, float throwVertical, float duration = 0.4f)
    {
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 3 * duration)
        {
            transform.position -= transform.forward * throwHorizontal * 2 * duration * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            // print(currentTime);
            float vertical = (Mathf.Sin(currentTime * Mathf.PI) * throwVertical * 0.5f) + playerHeight;

            transform.position = new Vector3(transform.position.x, vertical, transform.position.z);

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DashForward()
    {
        float duration = 0.125f;
        float startTime = Time.time;
        float currentTime = (Time.time - startTime) / duration;
        while (currentTime < 1f)
        {
            transform.position += transform.forward * dashSpeed * 4f * Time.deltaTime;

            currentTime = (Time.time - startTime) / duration;
            //print(currentTime);

            yield return new WaitForEndOfFrame();
        }

    }

    public void ToggleDifficulty(string newDifficulty) {
        difficulty = newDifficulty;

        if (difficulty == "Hard") {
            chanceToCounter = 0.66f; // 2 in 3  chance to counter 
            minCastTime = 2;
            maxCastTime = 7;
        } else if (difficulty == "Medium") {
            chanceToCounter = 0.50f; // 1 in 3 chance to counter
            minCastTime = 4;
            maxCastTime = 6;
        } else if (difficulty == "Easy") {
            chanceToCounter = 0.33f; // 1 in 3 chance to counter
            minCastTime = 6;
            maxCastTime = 8;
        } else {
            // Expert Difficulty
            chanceToCounter = 0.80f; // 80% chance to counter
            minCastTime = 2;
            maxCastTime = 4;
        }

        Debug.Log("AI Difficulty changed to "+difficulty+" -> counter %: "+chanceToCounter+"    minCastTime: "+minCastTime+"   maxCastTime: "+maxCastTime);
    }

    void CastRandomFireball(){
        int direction = Random.Range(0, 2);
        if (direction == 0)
            CastFireball(25, 1f);
        else if (direction == 1)
            CastFireball(-25, 1f);
        else
            CastFireball(0, 0f);
    }

    void CastRandomRoyalFire(){
        int dir = Random.Range(0, 2);
        if (dir == 0)
            CastRoyalFire(50, 0.2f);
        else if (dir == 1)
            CastRoyalFire(-50, 0.2f);
        else
            CastRoyalFire(0, 0f);
    }
    //public bool isAIPlayer = false;
    IEnumerator CastRandom()
    {
        yield return new WaitForSeconds(5);
        while (true)
        {
            // Chance that AI counters the last player spell cast
            counterChance = Random.value;

            // Roll to see which counter spell to cast (Set as range [0,1] so that counter spell probabilities can be uneven)
            counterSpell = Random.value;

            if (AIAttacks && health > 0)
            {
                if (counterChance >= (1f - chanceToCounter) ) {
                    // counter spell a "chanceToCounter" percentage of the time
                    Debug.Log("CounterSpell");
                    switch (lastPlayerSpell) {
                        case "fireball":
                            if (counterSpell >= 0.5) {
                                CastShieldBack();
                            } else if (counterSpell >= 0.25) {
                                CastRandomFireball();
                            } else {
                                CastArcanePulse();
                            }
                            break;
                        case "shield":
                            if (counterSpell >= 0.66) {
                                CastLightningNeutral();
                            } else if (counterSpell >= 0.3) {
                                CastRandomFireball();
                            } else {
                                CastWindForward();
                            }
                            break;
                        case "lightning":
                            if (counterSpell >= 0.33) {
                                CastRandomRoyalFire();
                            } else {
                                CastShieldBack();
                            }
                            break;
                        case "windslash":
                            if (counterSpell >= 0.5) {
                                CastShieldBack();
                            } else if (counterSpell >= 0.25) {
                                CastIceSpikes();
                            } else {
                                CastWindForward();
                            }
                            break;
                        case "royalfire":
                            if (counterSpell >= 0.66) {
                                CastWindForward();
                            } else if (counterSpell >= 0.33) {
                                CastRandomFireball();
                            } else {
                                CastRandomRoyalFire();
                            }
                            break;
                        case "icespikes":
                            if (counterSpell >= 0.5) {
                                CastShieldBack();
                            } else {
                                CastRandomFireball();
                            }
                            break;
                        case "arcanopulse":
                            if (counterSpell >= 0.66) {
                                CastShieldBack();
                            } else if (counterSpell >= 0.33) {
                                CastRandomRoyalFire();
                            } else {
                                CastArcanePulse();
                            }
                            break;
                        default:
                            CastRandomFireball();
                            break;
                    }
                } else {
                    // cast random spell
                    Debug.Log("CastRandom");
                    switch (Random.Range(0, 9)) {
                        case 0:
                        case 8:
                            CastRandomFireball();
                            break;
                        case 1:
                            CastShieldBack();
                            break;
                        case 2:
                        case 7:
                            CastWindForward();
                            break;
                        case 3:
                        case 9:
                            CastLightningNeutral();
                            break;
                        case 4:
                            CastArcanePulse();
                            break;
                        case 5:
                            CastIceSpikes();
                            break;
                        case 6:
                            CastRandomRoyalFire();
                            break;
                        default:
                            Debug.Log("CastFailed");
                            break;

                    }
                }
            }
            yield return new WaitForSeconds(Random.Range(minCastTime,maxCastTime));
        }
    }
}
