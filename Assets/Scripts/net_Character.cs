using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FirstGearGames.Mirrors.Assets.FlexNetworkAnimators;

public class net_Character : NetworkBehaviour
{
    public Animator animator;
    public FlexNetworkAnimator networkAnimator;

    [SyncVar]
    public uint playerId;

    public Transform target;
    public bool isWalking;
    public float resetDelay;

    Vector3 initialPos;

    public GameObject currentCharacter;
    public GameObject cachedOrb;

    public bool first;

    public enum CharacterType 
    {
        Alexander,
        Fassa,
        Xixi,
        Mina
    }

    public CharacterType type;

    public bool chargeAttack;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAuthority == false) return;


        if (chargeAttack == true)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= 5)
            { 
                this.Wait(2, () => { transform.position = initialPos; chargeAttack = false; });
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, target.position, 10 * Time.deltaTime);
            }
        }

        if (GameManager.singleton.decide == true) return;

        if(isWalking == true) 
        {      
            float distance = Vector3.Distance(transform.position, target.position);

            if(distance <= 5) 
            {
                animator.SetBool("Walk", false);

                isWalking = false;

                CmdAnimation(AnimationType.Attack);

                this.Wait(resetDelay, () => {  transform.position = initialPos; });
            }
            else 
            {
                transform.position = Vector3.Lerp(transform.position, target.position, 10 * Time.deltaTime);
            }
        }
    }

    [Command]
    void CmdAnimation(AnimationType type) 
    {
        RpcShowAnimation(type);
    }

    [ClientRpc]
    void RpcShowAnimation(AnimationType type) 
    {
      
        if (type == AnimationType.Attack) 
        {
            animator.Play("Attack");
        }
        else
        if(type == AnimationType.Win)
        {
            animator.Play("Win");
        }
        else
        if (type == AnimationType.Lose)
        {
            animator.Play("Lose");
        }

        if(this.type == CharacterType.Alexander) 
        {
            target.GetComponent<net_Character>().currentCharacter.GetComponent<RuntimeCharacter>().animator.Play("Hurt");
        }
        else
        {
            if (cachedOrb != null) return;
      

            WindPlayerAnimation wpa = currentCharacter.GetComponentInChildren<WindPlayerAnimation>();
            GameObject a = Instantiate(wpa.prefab , wpa.spawnPos.position, Quaternion.identity);

            if(first == false) 
            {
                a.GetComponent<SpriteRenderer>().flipX = true;
              
            }

            a.GetComponent<net_Orb>().target = target;
            a.GetComponent<net_Orb>().mainScene = true;
            cachedOrb = a;
        }
    }

    public void Walk() 
    {
        Debug.Log("calling");
        if (type == CharacterType.Alexander)
        {
            isWalking = true;
            animator.SetBool("Walk", true);
        }
        else 
        {
            CmdAnimation(AnimationType.Attack);
        }
    }

    public void Spawn(int index , Transform t) 
    {
        List<GameObject> chars = net_CharacterManager.Singleton.characters;
        GameObject go = Instantiate(chars[index], transform.position, transform.rotation);
        RuntimeCharacter rc = go.GetComponent<RuntimeCharacter>();
        rc.target = transform;

        currentCharacter = go;

        type = rc.cType;
        animator = rc.animator;
        networkAnimator.SetAnimator(rc.animator);
    }
}

public enum AnimationType 
{
    Idle,
    Attack,
    Win,
    Lose
}