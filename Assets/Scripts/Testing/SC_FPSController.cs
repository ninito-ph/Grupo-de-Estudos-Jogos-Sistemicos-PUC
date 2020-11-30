using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjetoAbelhas.WorldData;

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

    public GameObject tree_prefab;

    public GameObject[,] blocks_objects;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public Material mat;

    [HideInInspector]
    public bool canMove = true;

  

    void Start()
    {
        //Create pointer
        ProjetoAbelhas.WorldMeshBuilder builder = new ProjetoAbelhas.WorldMeshBuilder(16,10,10);
        builder.AddFluidHexagon(0,0,0,Color.red);
        pointer = new GameObject();
        pointer.AddComponent<MeshFilter>().mesh = builder.Create();
        pointer.AddComponent<MeshRenderer>().material = mat;
        pointer.transform.position = new Vector3(0,0,0);
        
        builder.Clear();

        blocks_objects = new GameObject[256,256];

        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        
    }

    bool init = false;

    void Update()
    {
        if(!init && TestWorldGenerator.world != null)
        {
            for(int i = 0; i < 1600; i ++)
            {
                int x = UnityEngine.Random.Range(0,128);
                int z = UnityEngine.Random.Range(0,128);
                
                if(TestWorldGenerator.world.CanWalkAboveTile(new TilePos(x,z),HexFace.LowerL,false))
                    SwitchStatusOfBlock(new TilePos(x,z),WorldUtils.TilePosToHex(new TilePos(x,z)));
            }
            init = true;
        }
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

            Vector2 pos = WorldUtils.WorldPosToHex(hit.x,hit.z);

            pointer.transform.position = new Vector3(pos.x,TestWorldGenerator.world.GetHeightAtPoint(pos.x,pos.y) + 0.025f,pos.y);

            if(Input.GetMouseButtonDown(0))
            {
                NavigationTest.CURRENT_TEST.CalculatePathTo(pos);
            }
            else if(Input.GetMouseButtonDown(1))
            {
                SwitchStatusOfBlock(WorldUtils.HexPosToGrid(pos.x,pos.y),pos);
            }
        }
        else
            pointer.transform.position = Vector3.zero;
    }

    public void SwitchStatusOfBlock(TilePos pos,Vector2 world)  
    {
        if(TestWorldGenerator.world.block_data[pos[0],pos[1]] == 0)
        {
            GameObject tree = GameObject.Instantiate(tree_prefab);
            tree.transform.position = new Vector3(world.x,TestWorldGenerator.world.GetHeightAtPoint(world.x,world.y),world.y);
            tree.transform.eulerAngles = new Vector3(-90f,UnityEngine.Random.Range(0,360),0);
            tree.transform.localScale *= 3f;

            TestWorldGenerator.world.block_data[pos[0],pos[1]] = 1;
            blocks_objects[pos[0],pos[1]] = tree;
        }
        else
        {

            TestWorldGenerator.world.block_data[pos[0],pos[1]] = 0;
            GameObject.Destroy(blocks_objects[pos[0],pos[1]]);
        }
    }
}