using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed = 5f;

    private Vector2 startPosition;
    private Vector2 lastPosition;
    private Vector2 currentPosition;

    private float currentX;
    private float targetX;
    private float deltaX;

    public float minX = -2.8f;
    public float maxX = 2.8f;
    public float sensitivity = 0.01f;

    public float smoothingFactor = 0.1f;

    private bool isRunning = false;
    private bool isSliding = false;

    private bool isRecovering = false;
    public float recoveryTime = 0.2f;
    public float backDistance = 3f;
    public LayerMask obstacleLayer;

    private List<Collectable> collectedList = new List<Collectable>();

    public float moneyAmount = 0;


    private void Awake()
    {

        ////Cursor.visible = false;
        ////Cursor.lockState = CursorLockMode.Locked;
    }
    void Start()
    {
        currentX = transform.position.x;
        targetX = transform.position.x;
    }

    private void Update()
    {
        HandleSwipeInput();
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!isRunning || isRecovering ) { return; }

        //forward
        Vector3 forward = transform.forward * speed;

        //horizontal sliding
        if (isSliding)
        {
            currentX = Mathf.Lerp(currentX, targetX, smoothingFactor/* * Time.deltaTime*/);
        }

        else
        {
            currentX = transform.position.x;
            targetX = transform.position.x;
        }

        
        float offset = currentX - transform.position.x;
        Vector3 horizontal = Vector3.right * offset;

        Vector3 totalMovement = forward + horizontal;
        transform.position += totalMovement * Time.deltaTime;

    }

    private void HandleSwipeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            lastPosition = startPosition;
            isSliding = true;

        }

        if (Input.GetMouseButton(0) && isSliding)
        {
            currentPosition = Input.mousePosition;
            deltaX = currentPosition.x - lastPosition.x;

            float horizontalMovement = deltaX * sensitivity;
            
            targetX += horizontalMovement;

            targetX = Mathf.Clamp(targetX, minX, maxX);

            lastPosition = currentPosition;

        }

        if (Input.GetMouseButtonUp(0))
        {
            isSliding = false;
        }
    }

    private IEnumerator Recover()
    {
        isRecovering = true;

        Vector3 target = transform.position - transform.forward * backDistance;
        Vector3 start = transform.position;

        float bounceTime = 0.5f;
        float timer = 0f;

        while (timer < bounceTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, timer / bounceTime);
            yield return null;
        }

        transform.position = target;

        yield return new WaitForSeconds(recoveryTime);

        isRecovering = false;
    }

    public void Run()
    {
        isRunning = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRecovering)  return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") && !other.CompareTag("ATM") && !other.CompareTag("Transformer"))
        {
            StartCoroutine(Recover());
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            Collectable collectable = other.GetComponent<Collectable>();
            if (collectable != null && !collectable.isCollected)
            {
                collectable.Collect(this);
            }
        }

    }


    public void Collect(Collectable newCollectable)
    {
        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(false);
        }

        collectedList.Add(newCollectable);

        Transform target;

        if (collectedList.Count == 1)
        {
            target = this.transform;
            newCollectable.collectDistance = 2f;
        }
        else
        {
            target = collectedList[collectedList.Count - 2].transform;
            newCollectable.collectDistance = 1f;

        }

        newCollectable.followTarget = target;
        newCollectable.SetTip(true);
    }

    public void DropCollectable(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;
        
        collectable.isCollected = false;
        collectable.followTarget = null;
        collectable.SetTip(false);
        collectable.gameObject.layer = LayerMask.NameToLayer("Collectable");

        //buna bakkkkkkkkkkkkkkkkkkkkk cirkin oldu
        Vector3 dropOffset = transform.position + Vector3.forward * Random.Range(6f, 9f) +
                    Vector3.right * Random.Range(-2f, 2f);
        collectable.transform.position = dropOffset;

        collectedList.Remove(collectable);

        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(true);
        }
        
    }

    public void DestroyCollectable(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;

        collectable.SetTip(false);
        collectedList.Remove(collectable);
        Destroy(collectable.gameObject);

        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(true);
        }
    }

    public void Deposit(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;

        collectable.SetTip(false);
        collectedList.Remove(collectable);
        Destroy(collectable.gameObject);

        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(true);
        }
        moneyAmount += collectable.value;
        Debug.Log("Current Money: " + moneyAmount);
    }

    //public void ResetPlayer()
    //{
    //    isRunning = false;
    //    isSliding = false;
    //    isRecovering = false;
    //    currentX = transform.position.x;
    //    targetX = transform.position.x;
    //    collectedList.Clear();
    //    moneyAmount = 0;
    //    transform.position = Vector3.zero;
    //}   



}

