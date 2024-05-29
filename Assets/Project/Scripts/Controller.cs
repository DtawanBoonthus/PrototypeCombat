using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [SerializeField] private bool isUseMovementA;
    
    [SerializeField] private InputActionReference lookDirectionInput;
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private InputActionReference attackIntervalInput;
    
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Camera cameraRay;
    [SerializeField] private Rigidbody rb;

    private Vector2 lookDirection = Vector2.zero;
    private Vector2 movement = Vector2.zero;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        
        lookDirectionInput.action.performed += OnLookDirectionPerformed;
        
        moveInput.action.performed += OnMovePerformed;
        moveInput.action.canceled += OnMoveCanceled;
        
        attackIntervalInput.action.performed += OnAttackIntervalPerformed;
        attackIntervalInput.action.canceled += OnAttackIntervalCanceled;
    }

    private void OnAttackIntervalPerformed(InputAction.CallbackContext callback)
    {
        Debug.Log($"Attack interval Performed: {callback.action.IsPressed()}");
    }
    
    private void OnAttackIntervalCanceled(InputAction.CallbackContext callback)
    {
        Debug.Log($"Attack interval Canceled: {callback.action.IsPressed()}");
    }

    private void OnDisable()
    {
        lookDirectionInput.action.performed -= OnLookDirectionPerformed;
        moveInput.action.performed -= OnMovePerformed;
        moveInput.action.canceled -= OnMoveCanceled;
    }
    
    private void OnMovePerformed(InputAction.CallbackContext callback)
    {
        movement = callback.ReadValue<Vector2>();
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext callback)
    {
        movement = Vector2.zero;
    }

    private void OnLookDirectionPerformed(InputAction.CallbackContext callback)
    {
        lookDirection = callback.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (isUseMovementA)
        {
            MovementA();
        }
        else
        {
            MovementB();
        }
       
        RotateToLookDirection();
    }

    private void MovementA()
    {
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y);
        
        moveDirection = transform.TransformDirection(moveDirection);
        
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }
    
    private void MovementB()
    {
        Vector3 moveDirection = new Vector3(movement.x, 0, movement.y);
        
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }
    
    private void RotateToLookDirection()
    {
        Ray ray = cameraRay.ScreenPointToRay(lookDirection);
        
        Plane plane = new Plane(Vector3.up, transform.position);
        
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            transform.rotation =
                Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }
}