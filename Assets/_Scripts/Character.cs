using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [SerializeField] private CircleCollider2D Collider2D;
    [SerializeField] private float ColliderOffset = 0.02f;
    [SerializeField] private float jumpStrength;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float dampingSpeed = 1;
    
    [SerializeField] private LayerMask layerMask;
    
    [SerializeField] private LongPressButton buttonToLeft;
    [SerializeField] private LongPressButton buttonToRight;
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
        
        buttonToLeft.OnButtonPressed.AddListener(() => playerInput.x = -1);
        buttonToLeft.OnButtonReleased.AddListener(() => playerInput.x = 0);
        
        buttonToRight.OnButtonPressed.AddListener(() => playerInput.x = 1);
        buttonToRight.OnButtonReleased.AddListener(() => playerInput.x = 0);
        
        buttonToJump.onClick.AddListener(() => playerInput.y = 1);
    }

    private void InputJump(InputAction.CallbackContext obj)
    {
        playerInput.y = 1;
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
        //Loock direction
        if (playerInput.x > 0 && _spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = false;
        }
        if (playerInput.x < 0 && !_spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = true;
        }
        
        //Ground check
        hit = Physics2D.CircleCast(transform.position,
            Collider2D.radius + ColliderOffset,
            -transform.up,
            0,
            layerMask);
        if (hit.collider != null)
        {
            isGrounded = true;
            isCanJump = true;
            targerPlayerAngler = Quaternion.LookRotation(Vector3.forward,hit.normal);
        }
        else
        {
            isGrounded = false;
            if (GravityObject != null)
            {
                hit = Physics2D.Raycast(transform.position,
                    GravityObject.transform.position - transform.position,
                    Vector2.Distance(GravityObject.transform.position, transform.position),
                    layerMask);
                targerPlayerAngler = Quaternion.LookRotation(Vector3.forward,hit.normal);
            }
        }
        
        //Input check
        if (playerInput.x != 0)
        {
            Vector2 right = new Vector2(hit.normal.y, -hit.normal.x);
            playerVelocity = right * playerInput.x * walkSpeed;
        }
        
        if (isGrounded && isCanJump && playerInput.y > 0)
        {
            isCanJump = false;
            jumpTimer = 0;
            playerJumpVelocity = hit.normal * jumpStrength;
        }
        playerVelocity = Vector2.Lerp(playerVelocity, Vector2.zero, dampingSpeed * Time.deltaTime);

        //Move rotation
        if (isGrounded)
        {
            transform.rotation = targerPlayerAngler;
        }
        else
        {
            float currentAngle = transform.eulerAngles.z;
            float targetAngleFloat = targerPlayerAngler.eulerAngles.z;

            float delta = targetAngleFloat - currentAngle;
            while (delta > 180) {delta -= 360; targetAngleFloat -= 360; }
            while (delta <= -180) {delta += 360; targetAngleFloat += 360; }
            
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngleFloat, 
                playerAngularSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        
        //Move position
        Vector3 nextPosition = (Vector3)playerVelocity * Time.deltaTime;
        playerVelocity = Vector2.zero;
        
        float jumpStrenght = jumpCurve.Evaluate(jumpTimer);
        if (jumpTimer < jumpTime)
        {
            nextPosition += (Vector3)playerJumpVelocity * jumpStrenght * Time.deltaTime;
            jumpTimer += Time.deltaTime;
            isGrounded = false;
        }

        if (!isGrounded)
        {
            float gravityStrenght = 1 - jumpStrenght;
            nextPosition += (Vector3)Gravity * gravityStrenght * Time.deltaTime;
        }

        transform.position += nextPosition;
        
        //Correction position
        hit = Physics2D.CircleCast(transform.position,
            Collider2D.radius + ColliderOffset,
            -transform.up,
            0,
            layerMask);
        
        if (isGrounded)
        {
            if (hit.collider == null)
            {
                hit = Physics2D.Raycast(transform.position,
                    GravityObject.transform.position - transform.position,
                    Vector2.Distance(GravityObject.transform.position, transform.position),
                    layerMask);
                
                transform.position = hit.point + hit.normal * Collider2D.radius;
            }
            else
            {
                transform.position = hit.point + hit.normal * Collider2D.radius;
            }
        }
        else if (playerInput.y == 0 && hit.collider != null)
        {
            playerVelocity = Vector2.zero;
            playerJumpVelocity = Vector2.zero;
            jumpTimer = jumpTime;
            transform.position = hit.point + hit.normal * Collider2D.radius;
        }
        playerInput.y = 0;
    }

    private void OnDestroy()
    {
        action.FindAction("Move").started -= InputMove;
        action.FindAction("Move").canceled -= InputMove;
        action.FindAction("Jump").performed -= InputJump;
        
        buttonToLeft.OnButtonPressed.RemoveAllListeners();
        buttonToLeft.OnButtonReleased.RemoveAllListeners();
        buttonToRight.OnButtonPressed.RemoveAllListeners();
        buttonToRight.OnButtonReleased.RemoveAllListeners();
        buttonToJump.onClick.RemoveAllListeners();
    }
}
