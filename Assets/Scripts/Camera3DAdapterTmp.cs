using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Camera3DAdapterTmp : MonoBehaviour
{
    Camera cam;
    public LayerMask groundMask;
    public float baseDis;
    public float targetHeight;
    public float targetWidth;
    private Vector3 hitPoint;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetWidth = Screen.width;
        targetHeight = Screen.height;

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f,groundMask))
        {
            baseDis = hit.distance;
            hitPoint = hit.point;
        }else
        {
            Debug.Log("Raycast not right ground ground");
        }
        Apply();
    }

    void Apply()
    {
        float currentAspect = targetWidth / targetHeight; 
        float targetAspect = (float)Screen.width / Screen.height;
        float ratio = targetAspect / currentAspect;

        transform.position = hitPoint - transform.forward * (baseDis * ratio);

    }
}
