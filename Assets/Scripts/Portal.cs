using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    #region Attributes

    private GameObject _target = null;
    public GameObject target
    {
        get { return _target; }
        set { _target = value; }
    }

    private RenderTexture _renderTexture = null;
    public RenderTexture renderTexture
    {
        get { return _renderTexture; }
        set { _renderTexture = value; }
    }

    private Color _color;
    public Color color
    {
        get { return _color; }
        set { _color = value; }
    }

    private MaterialPropertyBlock _propertyBlock = null;

    #endregion

    #region Teleportation

    private void Teleport(Transform other)
    {
        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(other.position);
        localPosition = new Vector3(-localPosition.x, localPosition.y, -localPosition.z);
        other.position = _target.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);

        Quaternion difference = _target.transform.rotation * Quaternion.Inverse(transform.rotation);
        other.rotation = difference * other.rotation;
    }

    #endregion

    #region Monobehaviour Functions

    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        renderTexture.autoGenerateMips = true;
        renderTexture.wrapMode = TextureWrapMode.Clamp;

        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetColor("color", color);
        _propertyBlock.SetTexture("_MainTex", renderTexture);
        GetComponentInChildren<MeshRenderer>().SetPropertyBlock(_propertyBlock);
    }

    private void OnTriggerStay(Collider other)
    {
        float zPos = transform.worldToLocalMatrix.MultiplyPoint3x4(other.transform.position).z;

        if (target && zPos < 0)
        {
            Teleport(other.transform);
        }
    }

    #endregion
}
