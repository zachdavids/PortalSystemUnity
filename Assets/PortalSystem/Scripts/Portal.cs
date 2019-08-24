using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [SerializeField] private Portal m_Target;

    private MaterialPropertyBlock m_PropertyBlock;
    private RenderTexture m_RenderTexture;

    public Portal Target { get; set; }
    public RenderTexture renderTexture { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        m_RenderTexture = new RenderTexture(Screen.width, Screen.height, 24);

      //  m_PropertyBlock = new MaterialPropertyBlock();
        //m_PropertyBlock.SetTexture("_MainTex", m_Target.m_RenderTexture);
        //GetComponentInChildren<MeshRenderer>().SetPropertyBlock(m_PropertyBlock);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerStay(Collider other)
    {
        float zPos = transform.worldToLocalMatrix.MultiplyPoint3x4(other.transform.position).z;

        if (zPos < 0)
        {
            Teleport(other.transform);
        }
    }

    private void Teleport(Transform other)
    {
        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(other.position);
        localPosition = new Vector3(-localPosition.x, localPosition.y, -localPosition.z);
        other.position = m_Target.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);

        Quaternion difference = m_Target.transform.rotation * Quaternion.Inverse(transform.rotation * Quaternion.Euler(0, 180, 0));
        other.rotation = difference * other.rotation;
    }
}
