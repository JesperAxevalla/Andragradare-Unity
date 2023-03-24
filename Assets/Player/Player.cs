using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{   
    [Header("Scene References")]
    [SerializeField]
    private Transform cam;
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private Transform releasePosition;

    [Header("Basic Controls")]
    [SerializeField]
    private float groundMovementSpeed = 40f;
    [SerializeField]
    private float sprintSpeed = 10f;
    [SerializeField]
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Camera")]
    [SerializeField]
    private GameObject cameraGameObject;
    [SerializeField]
    private float minZoomOut = -10f;
    [SerializeField]
    private float maxZoomOut = 10f;
    [SerializeField]
    [Range(0f,10f)]
    private float zoomPerScroll = 2f;
    [SerializeField]
    private float zoomSmoothTime = 0.1f;
    private float zoomOut = 0f;
    private float camTopRigDefaultHeight;
    private float camTopRigDefaultRadius;
    private float camMidRigDefault;

    [Header("Jumping")]
    [SerializeField]
    private float mass = 2;
    [Range(1f, 50f)]
    [SerializeField]
    private float jumpSpeed = 25f;
    [SerializeField]
    [Range(10, 100)]
    private int linePoints = 25;
    [SerializeField]
    [Range(0.01f, 0.25f)]
    private float timeBetweenPoints = 0.1f;

    private CinemachineFreeLook cinemachineFreeLook;

    private float vSpeed = 0;
    private Vector3 jumpVelocity;

    private void Awake()
    {
        Camera.main.gameObject.TryGetComponent<CinemachineBrain>(out var brain);
        if (brain == null)
        {
            brain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
        }
        brain.m_DefaultBlend.m_Time = 1;
        brain.m_ShowDebugText = true;

        cinemachineFreeLook = cameraGameObject.GetComponent<CinemachineFreeLook>();

        camTopRigDefaultHeight = cinemachineFreeLook.m_Orbits[0].m_Height;
        camTopRigDefaultRadius = cinemachineFreeLook.m_Orbits[0].m_Radius;
        camMidRigDefault = cinemachineFreeLook.m_Orbits[1].m_Radius;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {



        if (!characterController.isGrounded)
        {
            vSpeed += Physics.gravity.y * Time.deltaTime;
            jumpVelocity = jumpVelocity * Time.deltaTime + (Physics.gravity / 2f * Time.deltaTime * Time.deltaTime);
        } else 
            GroundMovement();

        MouseInputHandler();

        // Gravity
        characterController.Move(new Vector3(0, vSpeed + jumpVelocity.y, 0));

    }

    private void GroundMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") * groundMovementSpeed;
        float vertical = Input.GetAxisRaw("Vertical") * groundMovementSpeed;
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f && characterController.isGrounded)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            characterController.Move(moveDir.normalized * groundMovementSpeed * Time.deltaTime);
        }
    }

    private void MouseInputHandler()
    {
        MouseScrollHandler();

        // Left mouse button
        if (Input.GetMouseButton(0))
            DrawJump();

        if (Input.GetMouseButtonUp(0))
            Jump();
    }

    private void MouseScrollHandler()
    {
        var scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll != 0f)
        {

            if (scroll > 0f && zoomOut >= minZoomOut)
                zoomOut -= zoomPerScroll;
            if (scroll < 0f && zoomOut <= maxZoomOut)
                zoomOut += zoomPerScroll;
            cinemachineFreeLook.m_Orbits[1].m_Radius = camMidRigDefault + zoomOut;
        }
    }

    private void DrawJump()
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        Vector3 startPosition = releasePosition.position;
        Vector3 startVelocity = jumpSpeed * (cam.transform.forward + cam.transform.up) / mass;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < linePoints; time += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            lineRenderer.SetPosition(i, point);
        }
    }

    private void Jump()
    {
        jumpVelocity = jumpSpeed * (cam.transform.forward + cam.transform.up) / mass;
        lineRenderer.enabled = false;
    }

}
