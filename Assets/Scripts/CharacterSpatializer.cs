using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpatializer : MonoBehaviour
{
    private PlayerBehaviour playerBehaviour;
    private AIBehaviour aIBehaviour;

    public float inRoyalFireDistance, movementTolerance;
    public bool standingInRoyalFire, canMoveLeft, canMoveRight, canMoveForward, canMoveBackward, blockingShield, shieldInFrontOfSelf;

    // Front facing variables
    private float playerDistance, shieldDistance, iceSpikesDistance, royalFireDistance = 99f;

    // Back facing variables
    private float backIceSpikesDistance, backRoyalFireDistance = 99f;

    // Right facing variables
    private float rightShieldDistance, rightIceSpikesDistance, rightRoyalFireDistance = 99f;

    // Left facing variables
    private float leftShieldDistance, leftIceSpikesDistance, leftRoyalFireDistance = 99f;

    private string lastPlayerSpell, playerHeldSpell;
    private int lightningCharges;

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
        } else {
            lightningCharges = aIBehaviour.lightningCharge;
        }

        ///////////////////// RaycastHit hits;
        // FrontFacing Ray
        RaycastHit hit, rightHit, leftHit, backHit;
        if (Physics.SphereCast(transform.position, 0.5f,  transform.forward, out hit, 100.0f)) {
            //Debug.Log("Hit Object: "+hit.collider.tag+"  Distance: "+hit.distance);

            switch (hit.collider.tag) {
                case "Player":
                    playerDistance = hit.distance;
                    if (iceSpikesDistance < 99f) iceSpikesDistance = 99f;
                    if (shieldDistance < 99f) shieldDistance = 99f;
                    if (royalFireDistance < 99f) royalFireDistance = 99f;
                    if (blockingShield) blockingShield = false;
                    if (shieldInFrontOfSelf) shieldInFrontOfSelf = false;
                    break;
                case "IceSpikes":
                    iceSpikesDistance = hit.distance;
                    break;
                case "Shield":
                    shieldDistance = hit.distance;
                    if (shieldDistance <= 4) {
                        shieldInFrontOfSelf = true;
                    }
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


    public void ReactionCast(string spell, Vector3 endPosition) {
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
                StartCoroutine(CounterProjectile(1.35f));
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
            case "windslash":
                StartCoroutine(CounterWindslash());
                break;
            case "royalfire":
                StartCoroutine(CounterProjectile(2.8f));
                break;
            case "icespikes":
                StartCoroutine(CounterIceSpikes(endPosition));
                break;
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

    void CastForward(){
        aIBehaviour.CastWindForward();
    }

    void CastBackward(float chance) {
        float counterSpell = Random.value;
        if (counterSpell > chance) {
            aIBehaviour.CastShieldBack();
        } else {
            aIBehaviour.CastIceSpikes();
        }
    }

    void CastRight (float chance) {
        float counterSpell = Random.value;
        if (counterSpell > chance) {
            aIBehaviour.CastFireball(25, 1f);
        } else {
            aIBehaviour.CastRoyalFire(50, 0.2f);
        }
    }

    void CastLeft(float chance) {
        float counterSpell = Random.value;
        if (counterSpell > chance) {
            aIBehaviour.CastFireball(-25, 1f);
        } else {
            aIBehaviour.CastRoyalFire(-50, 0.2f);
        }
    }

    void CastStationary(bool inDanger) {
        if (inDanger) {
            aIBehaviour.CastArcanePulse();
        } else {
            if (lightningCharges < 2 || !shieldInFrontOfSelf) {
                aIBehaviour.CastLightningNeutral();
            }
        }
    }

    IEnumerator CounterProjectile (float maxReactionTime) {
        yield return new WaitForSeconds(0.2f);

        // Roll to see which counter spell to cast (Set as range [0,1] so that counter spell probabilities can be uneven)
        string moveDir = null;
        try {
            moveDir = safeDirections[Random.Range(0, safeDirections.Count)];
        } catch {
            // no safe directions
        }
        Debug.Log(" Directions count: " +safeDirections.Count+"   moveDir: "+moveDir);

        if (moveDir != null && moveDir != "Stationary") {
            switch (moveDir) {
                case "Forward":
                    yield return new WaitForSeconds(Random.value*maxReactionTime);
                    CastForward();
                    break;
                case "Backward":
                    yield return new WaitForSeconds(Random.value*maxReactionTime);
                    CastBackward(0.33f);
                    break;
                case "Right":
                    yield return new WaitForSeconds(Random.value*maxReactionTime);
                    CastRight(0.4f);
                    break;
                case "Left":
                    yield return new WaitForSeconds(Random.value*maxReactionTime);
                    CastLeft(0.4f);
                    break;
            }
        } else {
            // no safe direction, or stationary is safe
            yield return new WaitForSeconds(1.1f);
            CastStationary(true);
        }
    }

    IEnumerator CounterWindslash() {
        yield return new WaitForSeconds(0.3f);
        if (!shieldInFrontOfSelf) {
            if (canMoveForward && canMoveBackward) {
                float counterSpell = Random.value;
                if (counterSpell > 0.5f && playerDistance < 8f) {
                    CastForward();
                } else {
                    CastBackward(0.45f);
                }
            } else if (canMoveForward) {
                CastForward();
            } else if (canMoveBackward) {
                CastBackward(0.66f);
            } else {
                yield return new WaitForSeconds(Random.Range(0f, 2f));
                CastStationary(playerDistance <= 2f ? true : false);
            }
        } else {
            if (playerDistance < 8f) {
                CastBackward(0f);
            } else {
                yield return new WaitForSeconds(Random.Range(1f, 1.5f));
                CastStationary(playerDistance <= 2f ? true : false);
            }
        }
    }

    IEnumerator CounterIceSpikes(Vector3 endPosition) {
        yield return new WaitForSeconds(1.2f);
    }

    IEnumerator CounterStationary () {
        yield return new WaitForSeconds(1.1f);
    }
}