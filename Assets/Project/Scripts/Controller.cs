using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("Combat Attack")]
    [SerializeField] private bool useAttackFormatA;
    [SerializeField][Min(0.3f)] private float delayAttack;
    [SerializeField] private float timerResetCombat;
    [SerializeField] private float timerDelayEndAttack;
    [SerializeField] private int maxCombat;

    [Header("Interval Attack")] 
    private bool isAttack = false; 
    private bool isCanAttack = true;
    [SerializeField] private float attackInterval;
    [SerializeField] private float attackIntervalEnd;

    [SerializeField] private TextMeshProUGUI countAttackText;
    [SerializeField] private GameObject attackObj;
    [SerializeField] private bool isUseMovementA;

    [SerializeField] private InputActionReference lookDirectionInput;
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private InputActionReference attackIntervalInput;

    [SerializeField] private float rotateSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Camera cameraRay;
    [SerializeField] private Rigidbody rb;

    [Header("Debug")] [SerializeField] private int countAttack;

    private Vector2 lookDirection = Vector2.zero;
    private Vector2 movement = Vector2.zero;
    private bool canAttack = true;
    private bool isAttackEnd = false;
    private CancellationTokenSource cancellationTokenSource;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;

        lookDirectionInput.action.performed += OnLookDirectionPerformed;

        moveInput.action.performed += OnMovePerformed;
        moveInput.action.canceled += OnMoveCanceled;

        attackIntervalInput.action.performed += OnAttackIntervalPerformed;
        attackIntervalInput.action.canceled += OnAttackIntervalCanceled;
    }

    private void OnDisable()
    {
        lookDirectionInput.action.performed -= OnLookDirectionPerformed;

        moveInput.action.performed -= OnMovePerformed;
        moveInput.action.canceled -= OnMoveCanceled;

        attackIntervalInput.action.performed -= OnAttackIntervalPerformed;
        attackIntervalInput.action.canceled -= OnAttackIntervalCanceled;
    }

    private void OnAttackIntervalPerformed(InputAction.CallbackContext callback)
    {
        if (useAttackFormatA)
        {
            CountAttack();
        }
        else
        {
            if (isCanAttack)
            {
                IntervalAttack();
            }
        }
    }

    private async void OnAttackIntervalCanceled(InputAction.CallbackContext callback)
    {
        isAttack = false;
        
        if (!useAttackFormatA)
        {
            if (!isCanAttack) return;

            cancellationTokenSource.Cancel();
            countAttack = 0;
            isCanAttack = false;
            await UniTask.WaitForSeconds(attackIntervalEnd);
            isCanAttack = true;
        }
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

    private async void IntervalAttack()
    {
        isAttack = true;
        
        cancellationTokenSource = new CancellationTokenSource();
        
        while (isAttack)
        {
            if (countAttack >= maxCombat)
            {
                countAttack = 1;
            }
            else
            {
                countAttack++;
            }
            
            ShowAttack();

            bool isCancel = await UniTask
                .WaitForSeconds(attackInterval + 0.3f, cancellationToken: cancellationTokenSource.Token)
                .SuppressCancellationThrow();

            if (isCancel) break;
        }
    }
    
    private async void CountAttack()
    {
        if (isAttackEnd) return;
        
        if (!canAttack) return;

        cancellationTokenSource?.Cancel();
        ResetCombat();
        DelayCanAttack();
        
        if (countAttack >= maxCombat)
        {
            countAttack = 1;
        }
        else
        {
            countAttack++;
        }
        
        ShowAttack();

        if (countAttack == maxCombat)
        {
             AttackEnd();
        }
    }

    private async void AttackEnd()
    {
        isAttackEnd = true;
        await UniTask.WaitForSeconds(timerDelayEndAttack);
        isAttackEnd = false;
    }

    private void ShowAttack()
    {
        attackObj.SetActive(true);
        countAttackText.SetText(countAttack.ToString());
    }

    private async void ResetCombat()
    {
        cancellationTokenSource = new CancellationTokenSource();
        
        bool isCancel = await UniTask.WaitForSeconds(timerResetCombat, cancellationToken: cancellationTokenSource.Token)
            .SuppressCancellationThrow();
         
        if (!isCancel)
        {
            countAttack = 0;
        }
    }
    
    private async void DelayCanAttack()
    {
        canAttack = false;
        
        await UniTask.WaitForSeconds(delayAttack);

        canAttack = true;
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