using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí la Main Camera (el casco VR)")]
    public Transform vrCamera;

    void LateUpdate()
    {
        if (vrCamera == null) return;

        transform.LookAt(transform.position + vrCamera.forward);
    }
}