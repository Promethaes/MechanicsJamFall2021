using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour
{
    [Range(0.1f, 1.0f)]
    [SerializeField] float movementFalloffPercent = 0.1f;
    [SerializeField] float movementFalloffThreshold = 1.0f;
    [SerializeField] float movementSpeed = 1.0f;
    [SerializeField] AnimationCurve jumpCurve = null;
    [SerializeField] float jumpScalar = 1.0f;
    [SerializeField] float jumpDuration = 1.0f;
    [Range(1.0f, 10.0f)]
    [SerializeField] float mouseSensitivity = 1.0f;
    [Tooltip("The upper and lower limits of the camera's rotation on the local x axis.")]
    [SerializeField] float upDownLimit = 90.0f;

    [Header("References")]
    [SerializeField] new Transform cameraTransform = null;
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] GroundCollider groundCollider = null;


    Coroutine _jumpRoutine = null;

    Vector2 _moveVec = new Vector2();
    [HideInInspector] public bool doMovementFalloff = true;
    Vector3 _jumpVec = new Vector3();
    bool _canJump = false;

    Vector2 _lookVec = new Vector2();
    float _verticalAngle = 0.0f;
    [HideInInspector] public bool canLook = true;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        void EnableJump()
        {
            _canJump = true;
        }
        void DisableJump()
        {
            _canJump = false;
        }
        groundCollider.OnTouchGround.AddListener(EnableJump);
        groundCollider.OnLeaveGround.AddListener(DisableJump);
    }

    private void Update()
    {
        _verticalAngle -= _lookVec.y * Time.deltaTime * mouseSensitivity;
        _verticalAngle = Mathf.Clamp(_verticalAngle, -upDownLimit, upDownLimit);
        cameraTransform.localRotation = Quaternion.Euler(_verticalAngle, 0.0f, 0.0f);
        transform.localRotation = Quaternion.AngleAxis(_lookVec.x * Time.deltaTime * mouseSensitivity, Vector3.up) * transform.localRotation;
    }

    private void FixedUpdate()
    {
        rigidbody.AddForce(transform.forward * _moveVec.y * movementSpeed, ForceMode.Impulse);
        rigidbody.AddForce(transform.right * _moveVec.x * movementSpeed, ForceMode.Impulse);
        rigidbody.AddForce(_jumpVec, ForceMode.Impulse);

        if (!doMovementFalloff)
            return;
        //ignore y component
        var vel = rigidbody.velocity;
        vel.y = 0.0f;

        if (vel.magnitude > movementFalloffThreshold)
            rigidbody.AddForce(-vel * movementFalloffPercent, ForceMode.Impulse);
    }

    public void OnMove(CallbackContext ctx)
    {
        _moveVec = ctx.ReadValue<Vector2>();
    }

    public void OnJump(CallbackContext ctx)
    {
        if (!_canJump)
            return;
        IEnumerator Jump()
        {
            float x = 0.0f;
            while (x < 1.0f)
            {
                yield return new WaitForEndOfFrame();

                x += Time.deltaTime / jumpDuration;
                _jumpVec = Vector3.up * jumpScalar * jumpCurve.Evaluate(x);
            }
            _jumpVec = Vector3.zero;
        }
        if (ctx.performed)
            _jumpRoutine = StartCoroutine(Jump());
        else if (ctx.canceled)
        {
            StopCoroutine(_jumpRoutine);
            _jumpVec = Vector3.zero;
        }
    }

    public void OnLook(CallbackContext ctx)
    {
        if (!canLook)
            return;
        _lookVec = ctx.ReadValue<Vector2>();
    }

}
