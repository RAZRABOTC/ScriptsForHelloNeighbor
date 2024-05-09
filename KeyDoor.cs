using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KeyDoor : MonoBehaviour
{
    private NavMeshObstacle doorObstacle;
    private Camera m_Camera;//new
    [SerializeField] private float distance;
    [SerializeField] private LayerMask mask;
    [SerializeField] private AudioClip openClip,closeClip;
    [SerializeField] private AudioSource source;
    [SerializeField] private int doorID;
    private Animator anim;
    private GameObject player;
    bool locked = true;
    public GameObject lockDoor;
    // Start is called before the first frame update
    void Start()
    {
        doorObstacle = GetComponentInParent<NavMeshObstacle>();
        m_Camera = Camera.main;//new
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && locked)
        {
            if(!player.activeSelf)//new if
            {
                return;
            }
            RaycastHit hit;
            Ray ray = new Ray(m_Camera.transform.position, m_Camera.transform.forward);//new
            TakeItem takeItem = player.GetComponentInChildren<TakeItem>();//new
            if (Physics.Raycast(ray, out hit, distance, mask) && takeItem != null && takeItem.hasKey && takeItem.keyID == doorID)//new
            {
                StartCoroutine(DestroyLock());
                if(doorObstacle)
                {
                    doorObstacle.enabled = false;
                }
            }
        }
        if (Input.GetMouseButtonDown(0) && !locked)
        {
            RaycastHit hit;
            Ray ray = new Ray(m_Camera.transform.position, m_Camera.transform.forward);//new
            if (Physics.Raycast(ray, out hit, distance, mask))
            {
                if (!anim.GetBool("IsOpen"))
                {
                    anim.SetBool("IsOpen", true);
                    source.PlayOneShot(openClip);
                    return;
                }
                if (anim.GetBool("IsOpen"))
                {
                    anim.SetBool("IsOpen", false);
                    source.PlayOneShot(closeClip);
                }
            }
        }
    }

    IEnumerator DestroyLock()
    {
        lockDoor.GetComponent<Rigidbody>().isKinematic = false;
        yield return new WaitForSeconds(2);
        Destroy(lockDoor.gameObject);
        locked = false;
    }
    private void OnTriggerEnter(Collider other)//neweqw
    {
        if (other.tag == "EnemyNeighbor" && !locked)
        {
            StartCoroutine(OpenDoor());
        }
    }
    IEnumerator OpenDoor()//new fwefw
    {
        bool open = anim.GetBool("IsOpen");
        if (open)
        {
            yield return null;
        }
        if (!open)
        {
            anim.SetBool("IsOpen", true);
        }
        yield return new WaitForSeconds(1);
        anim.SetBool("IsOpen", false);
    }
}
