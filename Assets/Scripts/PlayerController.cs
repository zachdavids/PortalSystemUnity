using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Attributes

    [SerializeField] private float _walkSpeed = 6.0f;
    [SerializeField] private float _jumpSpeed = 4.0f;
    [SerializeField] private float _rotateSpeed = 0.8f;

    private Vector3 _moveDirection = Vector3.zero;
    private Transform _cameraTransform = null;
    private PortalManager _portalManager = null;
    private CharacterController _characterController = null;

    #endregion

    #region Movement

    private void Move()
    {
        transform.Rotate(0, Input.GetAxis("Mouse X") * _rotateSpeed, 0);

        _cameraTransform.Rotate(-Input.GetAxis("Mouse Y") * _rotateSpeed, 0, 0);
        if (_cameraTransform.localRotation.eulerAngles.y != 0)
        {
            _cameraTransform.Rotate(Input.GetAxis("Mouse Y") * _rotateSpeed, 0, 0);
        }

        _moveDirection = new Vector3(Input.GetAxis("Horizontal") * _walkSpeed, _moveDirection.y, Input.GetAxis("Vertical") * _walkSpeed);
        _moveDirection = transform.TransformDirection(_moveDirection);

        if (_characterController.isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                _moveDirection.y = _jumpSpeed;
            }
            else
            {
                _moveDirection.y = 0;
            }
        }

        _moveDirection.y += Physics.gravity.y * Time.deltaTime;
        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    #endregion

    #region Portals

    private void FireBlue()
    {
        Vector3 start = _cameraTransform.position;
        Vector3 end = _cameraTransform.forward;

        _portalManager.SpawnBluePortal(start, end);
    }

    private void FireRed()
    {
        Vector3 start = _cameraTransform.position;
        Vector3 end = _cameraTransform.forward;

        _portalManager.SpawnRedPortal(start, end);
    }

    #endregion

    #region Monobehaviour Functions

    // Start is called before the first frame update
    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        _portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
        _characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    private void Update()
    {
        Move();

        if (Input.GetMouseButtonDown(0))
        {
            FireBlue();
        }

        if (Input.GetMouseButtonDown(1))
        {
            FireRed();
        }
    }

    #endregion
}
