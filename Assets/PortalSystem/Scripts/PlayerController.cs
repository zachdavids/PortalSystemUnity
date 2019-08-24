using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_WalkSpeed = 6.0f;
    [SerializeField] private float m_JumpSpeed = 4.0f;
    [SerializeField] private float m_RotateSpeed = 0.8f;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier; 

    private Vector3 m_MoveDirection = Vector3.zero;
    private Camera m_Camera;
    private CharacterController m_CharacterController;

    // Start is called before the first frame update
    private void Start()
    {
        m_Camera = Camera.main;
        m_CharacterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(0, Input.GetAxis("Mouse X") * m_RotateSpeed, 0);

        m_Camera.transform.Rotate(-Input.GetAxis("Mouse Y") * m_RotateSpeed, 0, 0);
        if (m_Camera.transform.localRotation.eulerAngles.y != 0)
        {
            m_Camera.transform.Rotate(Input.GetAxis("Mouse Y") * m_RotateSpeed, 0, 0);
        }

        m_MoveDirection = new Vector3(Input.GetAxis("Horizontal") * m_WalkSpeed, m_MoveDirection.y, Input.GetAxis("Vertical") * m_WalkSpeed);
        m_MoveDirection = transform.TransformDirection(m_MoveDirection);

        if (m_CharacterController.isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                m_MoveDirection.y = m_JumpSpeed;
            }
            else
            {
                m_MoveDirection.y = 0;
            }
        }

        m_MoveDirection.y += Physics.gravity.y * Time.deltaTime;
        m_CharacterController.Move(m_MoveDirection * Time.deltaTime);
    }
}
