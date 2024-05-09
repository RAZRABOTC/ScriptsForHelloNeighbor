using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeItem : MonoBehaviour
{
    [SerializeField] private float distance;
    [SerializeField] private int initialLayer;
    [SerializeField] private float throwForce;
    [SerializeField] private LayerMask mask;
    [SerializeField] private AudioClip TakeClip;
    [SerializeField] private AudioClip ThrowClip;
    [SerializeField] private AudioSource source; 
    [SerializeField] private Transform handPos; 
    public Sprite itemSprite;
    public int keyID;
    public bool hasKey;
    private Rigidbody rb;
    Transform camTr;

    void Start()
    {
        camTr = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        initialLayer = gameObject.layer;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = new Ray(camTr.position, camTr.forward);
            if(Physics.Raycast(ray, out hit ,distance,mask))
            {
                rb.isKinematic = true;
                source.PlayOneShot(TakeClip);
                Take();
            }
        }
        if(Input.GetKeyDown(KeyCode.Q) && rb.isKinematic)
        {
            transform.position = camTr.position + camTr.forward * 0.2f;
            hasKey = false;
            rb.isKinematic = false;
            StartCoroutine(CheckIfItemUnderGround());
            this.gameObject.transform.SetParent(null);
            source.PlayOneShot(ThrowClip);
            gameObject.layer = initialLayer;
            rb.AddForce(camTr.forward * throwForce,ForceMode.Impulse);
            this.gameObject.GetComponent<Collider>().enabled = true;
            Inventory.Instance.RemoveItem(this);
        }
    }
    private void Take()
    {
        if (rb.isKinematic)
        {
            if (Inventory.Instance.InventoryIsFull())
            {
                return;
            }
            hasKey = true;
            this.gameObject.transform.SetParent(handPos.transform);
            this.gameObject.transform.position = handPos.position;
            this.gameObject.transform.rotation = handPos.rotation;
            this.gameObject.GetComponent<Collider>().enabled = false;
            if (!Inventory.Instance.InventoryIsEmpty())
            {
                gameObject.SetActive(false);
            }

                Inventory.Instance.PutItemInSlot(this);
            gameObject.layer = 11;
        }
    }
    IEnumerator CheckIfItemUnderGround()
    {
        Vector3 pos = transform.position;
        yield return new WaitForSeconds(1.5f);
        if (!rb.isKinematic)
        {
            if(transform.position.y < -6.6f)
            {
                transform.position = pos;
            }
        }
    }
}
