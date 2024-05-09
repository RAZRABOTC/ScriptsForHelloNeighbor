using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    Vector3 oldPos;//new
    [SerializeField] private PostProcessing postProccesing;
    [SerializeField] private float fearDistance = 5;
    public Transform trapSpawn;
    public static EnemyController Instance;
    public GameObject trapObj;
    public Transform startPos,enemyStartPos;
    private Animator anim;
    private GameObject player;
    public bool isStay = false;
    public NavMeshAgent agent;
    public NeighborPlaceToVisit[] places;
    public int maxChanse;
    public int[] chanses;
    public float distanceToCurrentPlace;
    public NeighborPlaceToVisit currentPlace,targetPlace;
    public enum Behaviour {stayOnPlace,goingToPlace,chasingPlayer,openDoor,lookingForPlayer,goToTrap}
    public Behaviour currentBehaviour;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        anim = GetComponentInChildren<Animator>();
        currentBehaviour = Behaviour.goingToPlace;
        SelectRandomPlace();
        CalcuateChanses();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(SetAnimation());
    }

    // Update is called once per frame
    void Update()
    {
        float distToPlayer = (transform.position - player.transform.position).magnitude;//ew
        if (distToPlayer < fearDistance)//new if
        {
            postProccesing.FearEffect(1 - distToPlayer/fearDistance);
        }
        else//new else
        {
            postProccesing.FearEffect(0);
        }
        if (currentBehaviour == Behaviour.goToTrap)
        {
            agent.SetDestination(player.transform.position);
        }
        distanceToCurrentPlace = (transform.position - currentPlace.transform.position).magnitude;
        if(currentBehaviour == Behaviour.goingToPlace && currentPlace != null &&currentBehaviour != Behaviour.chasingPlayer && currentBehaviour != Behaviour.goToTrap)
        {
            agent.speed = 3.5f;
            //anim.SetInteger("StateOfEnemy", 1);
            agent.SetDestination(currentPlace.transform.position);
        }
        if(distanceToCurrentPlace < 1.05f && !isStay && currentBehaviour != Behaviour.chasingPlayer && currentBehaviour != Behaviour.goToTrap)
        {
            isStay = true;
            currentBehaviour = Behaviour.stayOnPlace;
            StartCoroutine(Stay());
        }
        if(currentBehaviour == Behaviour.lookingForPlayer)
        {
            agent.SetDestination(transform.position);
        }
        float d = Vector3.Dot((player.transform.position - transform.position).normalized, transform.forward);
        RaycastHit hit;
        if (d > 0.2f)
        {
            if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, 100f))
            {
                if (hit.transform.gameObject == player)
                {
                    if (currentBehaviour == Behaviour.stayOnPlace || currentBehaviour == Behaviour.goingToPlace)
                    {
                        Invoke("DisableWalkToPlayer", 5);
                    }
                    currentBehaviour = Behaviour.chasingPlayer;
                }
            }
        }
        if (currentBehaviour == Behaviour.chasingPlayer)
        {
            agent.speed = 6;
            anim.SetInteger("StateOfEnemy", 2);
            agent.SetDestination(player.transform.position);
        }
    }
    NeighborPlaceToVisit PlaceAtChanse(int  chanse)
    {
        NeighborPlaceToVisit place = places[0];
        for(int i = 0; i < chanses.Length; i++)
        {
            if (chanses[i] > chanse)
            {
                place = places[i];
                return place;
            }
        }
        return place;
    }
    void SelectRandomPlace()
    {
        int rand = Random.Range(0, maxChanse);
        currentPlace = PlaceAtChanse(rand);     
    }
    void CalcuateChanses()
    {
        chanses = new int [places.Length];
        for(int i = 0; i < chanses.Length; i++)
        {
            chanses[i] = maxChanse;
            maxChanse += places[i].visitChanse;
        }
    }
    IEnumerator Stay()
    {
        //anim.SetInteger("StateOfEnemy", 0);
        yield return new WaitForSeconds(5);
        isStay = false;
        currentBehaviour = Behaviour.goingToPlace;
        SelectRandomPlace();
    }
    void DisableWalkToPlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, 100f))
        {
            if (hit.transform != player)
            {
                agent.speed = 3.5f;
                currentBehaviour = Behaviour.lookingForPlayer;
                StartCoroutine(LookingFor());
            }
            else
            {
                Invoke("DisableWalkToPlayer", 5);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            StartCoroutine(CatchPlayer());
        }
        //if(other.CompareTag())
    }
    IEnumerator LookingFor()
    {
        //anim.SetInteger("StateOfEnemy", 0);
        yield return new WaitForSeconds (5);
        Instantiate(trapObj, trapSpawn.position,Quaternion.Euler(-90,0,0));
        SelectRandomPlace();
        currentBehaviour = Behaviour.goingToPlace;
    }
    IEnumerator CatchPlayer()
    {
        StartCoroutine(postProccesing.DieEffect());//new
        player.GetComponent<FirstPersonMovement>().enabled = false;
        yield return new WaitForSeconds(2);
        transform.position = enemyStartPos.position;
        postProccesing.FearEffect(0);//new
        player.GetComponent<FirstPersonMovement>().enabled = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        player.GetComponent<Jump>().enabled = true;
        agent.speed = 3.5f;
        player.transform.position = startPos.position;
        currentBehaviour = Behaviour.goingToPlace;
    }
    public void GoToTrap()
    {
        //anim.SetInteger("StateOfEnemy", 2);
        currentBehaviour = Behaviour.goToTrap;
        agent.SetDestination(player.transform.position);
    }
    IEnumerator SetAnimation() {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            float speed = Vector3.Distance(transform.position, oldPos);
            oldPos = transform.position;
            if(speed < 0.1f)
            {
                anim.SetInteger("StateOfEnemy",0);
            }
            else if (speed < 0.5f)
            {
                {
                    anim.SetInteger("StateOfEnemy",1);
                }
            }
            else
            {
                anim.SetInteger("StateOfEnemy", 2);
            }
        }
    }
}
