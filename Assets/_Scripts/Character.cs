using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [SerializeField] private CircleCollider2D Collider2D;
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private float jumpStrength;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float dampingSpeed = 1;
    
    [SerializeField] private LayerMask layerMask;
    
    [SerializeField] private Button buttonToLeft;
    [SerializeField] private Button buttonToRight;
    [SerializeField] private Button buttonToJump;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    private Vector2 playerInput;
    public Vector2 Gravity;
    private Vector2 playerVelocity;
    private Vector2 playerJumpVelocity;
    private const float playerAngularSpeed = 120f;
    private Quaternion targerPlayerAngler;
    private const float jumpTime = 1;
    private float jumpTimer = jumpTime;
    private bool isCanJump;
    private RaycastHit2D hit;
    private bool isGrounded;

    private InputActionAsset action;
    public GravityObject GravityObject;
    
    private void Awake()
    {
        action = InputSystem.actions;
        EnhancedTouchSupport.Enable();
        action.FindAction("Move").started += InputMove;
        action.FindAction("Move").canceled += InputMove;
        action.FindAction("Jump").performed += InputJump;
        
        buttonToLeft.onClick.AddListener(() => playerInput.x = -1);
        buttonToRight.onClick.AddListener(() => playerInput.x = 1);
        buttonToJump.onClick.AddListener(() => playerInput.y = 1);
    }

    private void InputJump(InputAction.CallbackContext obj)
    {
        playerInput.y = 1;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        isCanJump = true;
        Rigidbody2D.linearVelocity = Vector2.zero;
        playerVelocity = Vector2.zero;
        playerJumpVelocity = Vector2.zero;
        jumpTimer = jumpTime;
    }

    private void InputMove(InputAction.CallbackContext obj)
    {
        if (obj.canceled)
        {
            //Debug.Log("Canceled");
            playerInput.x = 0;
            return;
        }
        
        float moveDirection = obj.ReadValue<Vector2>().x;
        playerInput.x = moveDirection;
    }

    private void Update()
    {
        if (playerInput.x > 0 && _spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = false;
        }
        if (playerInput.x < 0 && !_spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = true;
        }
        
        //Debug.DrawLine(transform.position, transform.position - transform.up * raycastDistance);
        hit = Physics2D.CircleCast(transform.position,
            Collider2D.radius + 0.02f,
            -transform.up,
            0,
            layerMask);
        if (hit.collider != null)
        {
            isGrounded = true;
            isCanJump = true;
            targerPlayerAngler = Quaternion.FromToRotation(Vector3.up,hit.normal);
        }
        else
        {
            isGrounded = false;
            if (GravityObject != null)
            {
                Vector2 normal = transform.position - GravityObject.transform.position;
                targerPlayerAngler = Quaternion.FromToRotation(Vector3.up,normal);
            }
        }
        
        if (playerInput.x != 0)
        {
            playerVelocity = transform.right * playerInput.x * walkSpeed;
        }
        
        if (isGrounded && isCanJump && playerInput.y > 0)
        {
            isCanJump = false;
            jumpTimer = 0;
            playerJumpVelocity = hit.normal * jumpStrength;
        }
        playerInput.y = 0;
        playerVelocity = Vector2.Lerp(playerVelocity, Vector2.zero, dampingSpeed * Time.deltaTime);

        if (isGrounded)
        {
            transform.rotation = targerPlayerAngler;
        }
        else
        {
            float currentAngle = transform.eulerAngles.z;
            float targetAngleFloat = targerPlayerAngler.eulerAngles.z;
            
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngleFloat, 
                playerAngularSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
            
            /*transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targerPlayerAngler, 
                playerAngularSpeed * Time.deltaTime);*/
        }
        
        RigidbodiUpdate();
    }

    private void RigidbodiUpdate()
    {
        Vector3 nextPosition = (Vector3)playerVelocity * Time.deltaTime;

        float jumpStrenght = jumpCurve.Evaluate(jumpTimer);
        if (jumpTimer < jumpTime)
        {
            nextPosition += (Vector3)playerJumpVelocity * jumpStrenght * Time.deltaTime;
            jumpTimer += Time.deltaTime;
        }

        if (!isGrounded)
        {
            float gravityStrenght = 1 - jumpStrenght;
            nextPosition += (Vector3)Gravity * gravityStrenght * Time.deltaTime;
        }

        Rigidbody2D.MovePosition(transform.position + nextPosition);
        Rigidbody2D.linearVelocity = Vector2.zero;
    }

    private void OnDestroy()
    {
        action.FindAction("Move").started -= InputMove;
        action.FindAction("Move").canceled -= InputMove;
        action.FindAction("Jump").performed -= InputJump;
        
        buttonToLeft.onClick.RemoveAllListeners();
        buttonToRight.onClick.RemoveAllListeners();
        buttonToJump.onClick.RemoveAllListeners();
    }
}
