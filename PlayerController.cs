using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private CharacterController controller;
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


    private void Awake()
    {
        controller = GetComponent<CharacterController>();

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
        //forward
        Vector3 forward = transform.forward * speed;

        //horizontal sliding
        currentX = Mathf.Lerp(currentX, targetX, smoothingFactor/* * Time.deltaTime*/);
        float offset = currentX - transform.position.x;
        Vector3 horizontal = Vector3.right * offset;

        Vector3 totalMovement = forward + horizontal;
        controller.Move(totalMovement * Time.deltaTime);       

    }

    private void HandleSwipeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            lastPosition = startPosition;

        }

        if (Input.GetMouseButton(0))
        {
            currentPosition = Input.mousePosition;
            deltaX = currentPosition.x - lastPosition.x;

            float horizontalMovement = deltaX * sensitivity;
            
            targetX += horizontalMovement;

            targetX = Mathf.Clamp(targetX, minX, maxX);

            lastPosition = currentPosition;


        }
    }
}
