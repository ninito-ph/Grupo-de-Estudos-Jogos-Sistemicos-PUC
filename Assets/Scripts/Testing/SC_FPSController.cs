using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class SC_FPSController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    public GameObject pointer;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public Material mat;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        //Cerate pointer
        ProjetoAbelhas.WorldMeshBuilder builder = new ProjetoAbelhas.WorldMeshBuilder(16,10,10);
        builder.AddFluidHexagon(0,0,0,Color.red);
        pointer = new GameObject();
        pointer.AddComponent<MeshFilter>().mesh = builder.Create();
        pointer.AddComponent<MeshRenderer>().material = mat;
        pointer.transform.position = new Vector3(0,0,0);
        
        builder.Clear();


        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        var ray = playerCamera.ScreenPointToRay( new Vector2( Screen.width / 2, Screen.height / 2 ));
        RaycastHit hitPoint;
 
        if( Physics.Raycast( ray, out hitPoint, 100.0f ))
        {
            Vector3 hit = hitPoint.point + hitPoint.normal/16f;

            Vector2 pos = ProjetoAbelhas.WorldGeneration.WorldUtils.WorldPosToHex(hit.x,hit.z);

            pointer.transform.position = new Vector3(pos.x,TestWorldGenerator.world.GetHeightAtPoint(pos.x,pos.y) + 0.025f,pos.y);
        }
        else
            pointer.transform.position = Vector3.zero;
    }
}