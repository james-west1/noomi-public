using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    Vector3 offset;

    void Start()
    {
        // get difference between target and camera at the start
        offset = transform.position - target.position;
    }

    void Update()
    {
        // move camera to the calculated difference plus the player position, so whenever the player moves the camera will move with it
        transform.position = target.position + offset;
    }
}
