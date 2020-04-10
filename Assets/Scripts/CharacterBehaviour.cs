using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class CharacterBehaviour : NetworkBehaviour
{
    [SyncVar]
    public GameObject otherPlayer;

    [SyncVar]
    public int health = 3;

    [SyncVar]
    public int lightningCharge = 0;

    [SyncVar]
    //Royal Fire will deal damage if royalBurn reaches 1
    public float royalBurn = 0f;

    //royalBurn decreases by this every second
    public float royalBurnRecovery = 0.2f;

    public float dashSpeed;
    public float dashHeight;

    public GameObject fireball;
    public GameObject shield;
    public GameObject windslash;
    public GameObject lightningChargeObj;
    public GameObject lightning;
    public GameObject arcanePulse;
    public GameObject iceSpikeProjectile;
    public GameObject fizzle;
    public GameObject royalFireball;
    public RuntimeAnimatorController controller;

    public List<GameObject> shields;
    public int maxShields = 2;

    //called by NewNetworkManager
    public void SetOtherPlayer(GameObject op)
    {
        Debug.Log("Passed other player");
        Debug.Log(op);
        otherPlayer = op;
        Debug.Log("Set other player");
        Debug.Log(otherPlayer);
    }
    
    [TargetRpc]
    public void TargetResetPosition(NetworkConnection connection, Vector3 pos)
    {
        transform.position = pos;
    }

    [ClientRpc]
    public abstract void RpcResetUI();

    public void RestoreHealth(int h) {
        health = h;
    }

    public float DistanceToCenter()
    {
        GameObject centerMark = GameObject.Find("CenterMark");
        return Vector3.Distance(this.gameObject.transform.position, centerMark.transform.position);
    }

    //Server: Only the server executes the function. 
    //(However, because the variable is synced, clients will also see the HP decrease.)
    public void TakeDamage(int dmg)
    {
        health -= dmg;
    }

    public abstract void TargetThrowPlayerBack(NetworkConnection target, float horizontal, float vertical, int duration);

    public abstract void TargetSetAnimTrigger(NetworkConnection target, string s);

}
