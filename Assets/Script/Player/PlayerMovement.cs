using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

namespace MagesnShadows.PlayerSystems
{
    public class PlayerMovement : NetworkBehaviour
    {
        #region Vars

        [SerializeField] private float walkSpeed = 0.01f;
        [SerializeField] private float runSpeed = 0.02f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -4.2f;
        [SerializeField] private bool isGrounded;
        [SerializeField] private float groundCheckDis;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private GameObject groundChecker;
        [SerializeField] private bool isAlive = true;

        private Vector3 moveDirection;
        private Vector3 velocity;

        

        #endregion

        #region References
        [SerializeField] private CharacterController controller;
        [SerializeField] private Rigidbody rb;

        #endregion





        // Start is called before the first frame update
        void Start()
        {

            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }



        }
        //public override void OnStartNetwork()
        //{
        //    base.OnStartNetwork();
        //    if (!base.IsOwner)
        //    {
        //        this.enabled = false;
        //    }
        //}
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!base.IsOwner)
            {
                this.enabled = false;
                return;
            }
            else if (base.IsOwner)
            {
                this.enabled = true;
            }
            isAlive = true;
        }


        // Update is called once per frame
        void Update()
        {
            if (isAlive)
            {
                Move();
            }
            

        }
        public bool IsAlive => isAlive;
        [ServerRpc]
        public void AliveToggle()
        {
            isAlive = !isAlive;

            
        }
        private void Move()
        {
            isGrounded = Physics.CheckSphere(groundChecker.transform.position, groundCheckDis, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = gravity;
            }


            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            

            //moveDirection = new Vector3(x, 0, z);
            //moveDirection *= walkSpeed;
            moveDirection = (transform.right * x + transform.forward * z);
            
            moveDirection.Normalize();
            if (moveDirection != Vector3.zero && !Input.GetKey(KeyCode.LeftShift))
            {
                //Walk
                Walk();
            }
            else if (moveDirection != Vector3.zero && Input.GetKey(KeyCode.LeftShift))
            {
                //Run
                Run();
            }
            else
            {
                Idle();
            }
            if (isGrounded && Input.GetKeyDown(KeyCode.F))
            {
                Jump();
            }



            controller.Move(moveDirection * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

        }

        private void Walk()
        {
            moveDirection = moveDirection * walkSpeed;
            print("walking");
        }

        private void Run()
        {
            moveDirection = moveDirection * runSpeed;
            print("running");
        }
        private void Idle()
        {

        }
        private void Jump()
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            print("Jump calculated");
        }

    }


}


//controller.Move(velocity * Time.deltaTime);
public struct SyncMoveData
{

}