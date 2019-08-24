using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    public Portal target { get; set; }
    public RenderTexture renderTexture { get; set; }
    public Color color { get; set; }
    public GameObject surface { get; set; }

    private MaterialPropertyBlock m_PropertyBlock;

    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        renderTexture.autoGenerateMips = true;
        renderTexture.wrapMode = TextureWrapMode.Clamp;

        m_PropertyBlock = new MaterialPropertyBlock();
        m_PropertyBlock.SetColor("color", color);
        m_PropertyBlock.SetTexture("_MainTex", renderTexture);
        GetComponentInChildren<MeshRenderer>().SetPropertyBlock(m_PropertyBlock);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerStay(Collider other)
    {
        float zPos = transform.worldToLocalMatrix.MultiplyPoint3x4(other.transform.position).z;

        if (target && zPos < 0)
        {
            Teleport(other.transform);
        }
    }

    private void Teleport(Transform other)
    {
        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(other.position);
        localPosition = new Vector3(-localPosition.x, localPosition.y, -localPosition.z);
        other.position = target.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);

        Quaternion difference = target.transform.rotation * Quaternion.Inverse(transform.rotation * Quaternion.Euler(0, 180, 0));
        other.rotation = difference * other.rotation;
    }
}
