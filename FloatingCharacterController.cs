using UnityEngine;

namespace Me.Mato
{
    public class FloatingCharacterController : MonoBehaviour
    {
        [Header("References")]
        public LayerMask whatIsGround;
        public Transform origin;
        public Transform playerCamera;

        [Header("Movement Settings")]
        public float moveSpeed = 5.0f;
        public float sprintSpeed = 1.25f;
        public float maxMoveSpeed = 5.0f;
    
        [Header("Look Settings")]
        public float lookSpeed = 2f;

        [Header("Jump Settings")]
        public float jumpForce = 10.0f;
        public int maxJumps = 2;
        [field: SerializeField] 
        public float GravityScale { get; private set; } = 1.5f;
        private int jumpsLeft;

        [Header("Float Settings")]    
        public float rideHeight = 2.0f;
        public float raycastSize = 5.0f;
        public float springStrength = 50.0f;
        public float springDamper = 5f;

        private Rigidbody rb;
        public bool canFloat;
        private float rotationX;
        private float rotationY;
        public bool IsGrounded { get; private set;  }

        private void Start()
        {
            canFloat = true;
            rb = GetComponent<Rigidbody>();
        
            jumpsLeft = maxJumps;
        
            rb.freezeRotation = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            Look();

            if (Input.GetButtonDown("Jump"))
            {
                if (IsGrounded)
                {
                    Jump();
                }
                else if (jumpsLeft >= 0)
                {
                    jumpsLeft--;
                    Jump();
                }
            }

            if (IsGrounded)
                jumpsLeft = maxJumps;

            Physics.Raycast(origin.position, -origin.up, out RaycastHit rideHit, rideHeight, whatIsGround);
            IsGrounded = rideHit.transform;
        }

        private void Jump()
        {
            canFloat = false;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            
            Invoke(nameof(ResetFloat), 1f);
        }

        private void FixedUpdate()
        {
            rb.AddForce(Vector3.up * (Physics.gravity.y * GravityScale));
        
            Float();
            Move();
        }

        private void Move()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            float currentMoveSpeed = moveSpeed;

            if (Input.GetKey(KeyCode.LeftShift))
                currentMoveSpeed = sprintSpeed;

            Vector3 currentVelocity = rb.velocity;
            Vector3 targetVelocity = new Vector3(horizontalInput, 0, verticalInput);
            targetVelocity *= currentMoveSpeed;

            targetVelocity = transform.TransformDirection(targetVelocity);

            Vector3 velocityChange = targetVelocity - currentVelocity;
            velocityChange.y = 0;

            Vector3.ClampMagnitude(velocityChange, maxMoveSpeed);
        
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    
        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            rotationX += mouseX * (lookSpeed * Time.deltaTime);
            rotationY -= mouseY * (lookSpeed * Time.deltaTime);

            rotationY = Mathf.Clamp(rotationY, -75, 75);
        
            transform.rotation = Quaternion.Euler(0, rotationX, 0);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationY, 0, 0);
        }

        private void Float()
        {
            if (!canFloat)
                return;
            
            Debug.DrawRay(origin.position, -origin.up * raycastSize, Color.red);
            Debug.DrawRay(origin.position, -origin.up * rideHeight, Color.yellow);
            Physics.Raycast(origin.position, -origin.up, out RaycastHit hit, raycastSize, whatIsGround);

            if (!hit.transform)
                return;

            Vector3 vel = rb.velocity;
            Vector3 rayDir = origin.TransformDirection(Vector3.down);
            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = hit.rigidbody;

            if (hitBody)
                otherVel = hitBody.velocity;

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float x = hit.distance - rideHeight;
            
            float springForce = (x * springStrength) - (relVel * springDamper);
            
            Debug.DrawLine(origin.position, (origin.position + rayDir * (springForce)), Color.cyan);
            rb.AddForce(rayDir * springForce);

            if (hitBody)
                hitBody.AddForceAtPosition(rayDir * -springForce, hit.point);
        }

        private void ResetFloat()
        {
            canFloat = true;
        }
    
        public void ScaleGravity(float newScale)
        {   
            // For per-object gravity areas.
            GravityScale = newScale;
        }
    }
}