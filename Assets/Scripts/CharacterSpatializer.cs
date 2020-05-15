using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpatializer : MonoBehaviour
{
    private PlayerBehaviour playerBehaviour;
    private AIBehaviour aIBehaviour;

    public float inRoyalFireDistance, movementTolerance;
    public bool standingInRoyalFire, canMoveLeft, canMoveRight, canMoveForward, canMoveBackward, blockingShield;

    // Front facing variables
    private float playerDistance, shieldDistance, iceSpikesDistance, royalFireDistance = 99f;

    // Back facing variables
    private float backIceSpikesDistance, backRoyalFireDistance = 99f;

    // Right facing variables
    private float rightShieldDistance, rightIceSpikesDistance, rightRoyalFireDistance = 99f;

    // Left facing variables
    private float leftShieldDistance, leftIceSpikesDistance, leftRoyalFireDistance = 99f;

    private string lastPlayerSpell, playerHeldSpell;

    private List<string> safeDirections;

    // Start is called before the first frame update
    void Start()
    {
        safeDirections = new List<string>( );
    }

    // Update is called every 20ms
    void FixedUpdate()
    {
        if (playerBehaviour == null) {
            playerBehaviour = GetComponent<AIBehaviour>().otherPlayer.GetComponent<PlayerBehaviour>();
        }
        if (aIBehaviour == null) {
            aIBehaviour = GetComponent<AIBehaviour>();
        }

        ///////////////////// RaycastHit hits;
        // FrontFacing Ray
        RaycastHit hit, rightHit, leftHit, backHit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 100.0f)) {
            //Debug.Log("Hit Object: "+hit.collider.tag+"  Distance: "+hit.distance);

            switch (hit.collider.tag) {
                case "Player":
                    playerDistance = hit.distance;
                    if (iceSpikesDistance < 99f) iceSpikesDistance = 99f;
                    if (shieldDistance < 99f) shieldDistance = 99f;
                    if (royalFireDistance < 99f) royalFireDistance = 99f;
                    if (blockingShield) blockingShield = false;
                    break;
                case "IceSpikes":
                    iceSpikesDistance = hit.distance;
                    break;
                case "Shield":
                    shieldDistance = hit.distance;
                    blockingShield = true;
                    break;
                case "RoyalFire":
                    royalFireDistance = hit.distance;
                    break;
            }
        }

        // BackFacing Ray
        if (Physics.Raycast(transform.position, transform.forward * -1f, out backHit, 100.0f)) {
            //Debug.Log("backHit Object: "+backHit.collider.tag+"  Distance: "+backHit.distance);

            switch (backHit.collider.tag) {
                case "IceSpikes":
                    backIceSpikesDistance = backHit.distance;
                    break;
                case "RoyalFire":
                    backRoyalFireDistance = backHit.distance;
                    break;
            }
        } else {
            if (backIceSpikesDistance < 99f) backIceSpikesDistance = 99f;
            if (backRoyalFireDistance < 99f) backRoyalFireDistance = 99f;
        }

        // RightSide Ray
        if (Physics.Raycast(transform.position, (Quaternion.Euler(0, 50, 0) * transform.forward), out rightHit, 100.0f)) {
            //Debug.Log("rightHit Object: "+rightHit.collider.tag+"  Distance: "+rightHit.distance);

            switch (rightHit.collider.tag) {
                case "IceSpikes":
                    rightIceSpikesDistance = rightHit.distance;
                    break;
                case "Shield":
                    rightShieldDistance = rightHit.distance;
                    break;
                case "RoyalFire":
                    rightRoyalFireDistance = rightHit.distance;
                    break;
            }
        } else {
            if (rightIceSpikesDistance < 99f) rightIceSpikesDistance = 99f;
            if (rightRoyalFireDistance < 99f) rightRoyalFireDistance = 99f;
            if (rightShieldDistance < 99f) rightShieldDistance = 99f;
        }

        // LeftSide Ray
        if (Physics.Raycast(transform.position, (Quaternion.Euler(0, -50, 0) * transform.forward), out leftHit, 100.0f)) {
            //Debug.Log("leftHit Object: "+leftHit.collider.tag+"  Distance: "+leftHit.distance);

            switch (leftHit.collider.tag) {
                case "IceSpikes":
                    leftIceSpikesDistance = leftHit.distance;
                    break;
                case "Shield":
                    leftShieldDistance = leftHit.distance;
                    break;
                case "RoyalFire":
                    leftRoyalFireDistance = leftHit.distance;
                    break;
            }
        } else {
            if (leftIceSpikesDistance < 99f) leftIceSpikesDistance = 99f;
            if (leftRoyalFireDistance < 99f) leftRoyalFireDistance = 99f;
            if (leftShieldDistance < 99f) leftShieldDistance = 99f;
        }

        // Parse Info
        standingInRoyalFire = aIBehaviour.royalBurn > 0;
        canMoveLeft = (standingInRoyalFire || leftRoyalFireDistance > movementTolerance) && (leftIceSpikesDistance > movementTolerance);
        canMoveRight = (standingInRoyalFire || rightRoyalFireDistance > movementTolerance) && (rightIceSpikesDistance > movementTolerance);
        canMoveForward = (standingInRoyalFire || royalFireDistance > movementTolerance) && iceSpikesDistance > movementTolerance && shieldDistance > movementTolerance;
        canMoveBackward = (standingInRoyalFire || backRoyalFireDistance > movementTolerance) && (backIceSpikesDistance > movementTolerance) && aIBehaviour.DistanceToCenter() < 27;
        safeDirections.Clear();
        if (canMoveLeft) safeDirections.Add("Left");
        if (canMoveRight) safeDirections.Add("Right");
        if (canMoveForward) safeDirections.Add("Forward");
        if (canMoveBackward) safeDirections.Add("Backward");
        if (!standingInRoyalFire) safeDirections.Add("Stationary");
    }

    // Works with all knowledge 
    public string SmartestSpellCast() {

        return "arcanopulse";
    }

    // Considers most factors, but is not reactionary.
    public string SmartSpellCast() {

        return "icespikes";
    }

    // Only considers movement factors
    public string DecentSpellCast() {

        return "windslash";
    }


    public void ReactionCast(string spell) {
        // 1.2 seconds to react to a royal fireball
        // 1.65 seconds to react to a standard fireball
        // Ice spikes depends on time
        // Windslash should not be reacted to
        // Arcanopulse might be difficult
        // Shield has no reaction
        // Lightning reaction depends on how many charges the player has
        Debug.Log("Reacting");

        switch (spell) {
            case "fireball":
                StartCoroutine(CounterFireball());
                break;
            // case "shield":
            //     if (counterSpell >= 0.66) {
            //         aIBehaviour.CastLightningNeutral();
            //     } else if (counterSpell >= 0.3) {
            //         aIBehaviour.CastRandomFireball();
            //     } else {
            //         aIBehaviour.CastWindForward();
            //     }
            //     break;
            // case "lightning":
            //     if (counterSpell >= 0.33) {
            //         aIBehaviour.CastRandomRoyalFire();
            //     } else {
            //        aIBehaviour.CastShieldBack();
            //     }
            //     break;
            // case "windslash":
            //     if (counterSpell >= 0.5) {
            //         aIBehaviour.CastShieldBack();
            //     } else if (counterSpell >= 0.25) {
            //         aIBehaviour.CastIceSpikes();
            //     } else {
            //         aIBehaviour.CastWindForward();
            //     }
            //     break;
            // case "royalfire":
            //     if (counterSpell >= 0.66) {
            //         aIBehaviour.CastWindForward();
            //     } else if (counterSpell >= 0.33) {
            //         aIBehaviour.CastRandomFireball();
            //     } else {
            //         aIBehaviour.CastRandomRoyalFire();
            //     }
            //     break;
            // case "icespikes":
            //     if (counterSpell >= 0.5) {
            //         aIBehaviour.CastShieldBack();
            //     } else {
            //         aIBehaviour.CastRandomFireball();
            //     }
            //     break;
            // case "arcanopulse":
            //     if (counterSpell >= 0.66) {
            //         aIBehaviour.CastShieldBack();
            //     } else if (counterSpell >= 0.33) {
            //         aIBehaviour.CastRandomRoyalFire();
            //     } else {
            //         aIBehaviour.CastArcanePulse();
            //     }
            //     break;
            // default:
            //     aIBehaviour.CastRandomFireball();
            //     break;
        }
    }

    IEnumerator CounterFireball () {
        // Roll to see which counter spell to cast (Set as range [0,1] so that counter spell probabilities can be uneven)
        float counterSpell = Random.value;

        string moveDir = null;
        try {
            moveDir = safeDirections[Random.Range(0, safeDirections.Count-1)];
        } catch {
            // no safe directions
        }
        Debug.Log(" Directions count: " +safeDirections.Count+"   moveDir: "+moveDir);

        if (moveDir != null && moveDir != "Stationary") {
            switch (moveDir) {
                case "Forward":
                    yield return new WaitForSeconds(Random.value*1.55f);
                    aIBehaviour.CastWindForward();
                    break;
                case "Backward":
                    yield return new WaitForSeconds(Random.value*1.55f);
                    if (counterSpell > 0.33f) {
                        aIBehaviour.CastShieldBack();
                    } else {
                        aIBehaviour.CastIceSpikes();
                    }
                    break;
                case "Right":
                    yield return new WaitForSeconds(Random.value*1.55f);
                    if (counterSpell > 0.4f) {
                        aIBehaviour.CastFireball(25, 1f);
                    } else {
                        aIBehaviour.CastRoyalFire(50, 0.2f);
                    }
                    break;
                case "Left":
                    yield return new WaitForSeconds(Random.value*1.55f);
                    if (counterSpell > 0.4f) {
                        aIBehaviour.CastFireball(-25, 1f);
                    } else {
                        aIBehaviour.CastRoyalFire(-50, 0.2f);
                    }
                    break;
            }
        } else {
            // no safe direction, or stationary is safe
            yield return new WaitForSeconds(1.2f);
            aIBehaviour.CastArcanePulse();
        }
        
        //yield return new WaitForSeconds(0.1f);
    }
}