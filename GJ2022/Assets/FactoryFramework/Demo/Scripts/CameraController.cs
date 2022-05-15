using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public KeyCode fwd = KeyCode.W;
    public KeyCode back = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode CW = KeyCode.E;
    public KeyCode CCW = KeyCode.Q;

    public float moveSpeed = 15f;
    public float rotateSpeed = 90f;
    public float zoomSpeed = 15f;
    public float dampening = 5f;


    private Vector3 _desiredPosition;
    private Vector3 _desiredRotation;

    private float minCameraDist = 5f;
    private float maxCameraDist = 50f;

    private Camera camera;

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Axis shmaxis I say
        float forward = 0f + (Input.GetKey(fwd) ? 1f : 0f) - (Input.GetKey(back) ? 1f : 0f);
        float sideways = 0f + (Input.GetKey(right) ? 1f : 0f) - (Input.GetKey(left) ? 1f : 0f);
        float roll = 0f + (Input.GetKey(CW) ? 1f : 0f) - (Input.GetKey(CCW) ? 1f : 0f);

        Vector3 moveDelta = new Vector3(sideways, 0f, forward) * moveSpeed * Time.deltaTime;
        Vector3 rotDelta = new Vector3(0f, roll, 0f) * rotateSpeed * Time.deltaTime;

        _desiredPosition += transform.TransformVector(moveDelta);
        _desiredRotation += rotDelta;

        transform.position = Vector3.Lerp(transform.position, _desiredPosition, dampening * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_desiredRotation), dampening * Time.deltaTime);

        Vector3 dir = camera.transform.localPosition.normalized;
        float zoomDelta = Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
        float dist = camera.transform.localPosition.magnitude;
        float zoomLevel = Mathf.Clamp(dist - zoomDelta, minCameraDist, maxCameraDist);
        camera.transform.localPosition = dir * zoomLevel;

    }
}
