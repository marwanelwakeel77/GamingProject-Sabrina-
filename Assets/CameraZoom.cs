using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Camera Cam;
    private float TargetZoom = 3;
    private float ScrollData;
    public float ZoomSpeed=3;
    public float minZoom = 3; //the closest the camera can zoom in
    public float maxZoom = 6; //the farthest the camera can zoom out
    public float scrollAmount = 1; //how much one scroll of the mouse wheel zooms


    void Start()
    {
        Cam = GetComponent<Camera>();
        TargetZoom = Cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        ScrollData = Input.GetAxis("Mouse ScrollWheel");
        TargetZoom=TargetZoom-ScrollData*scrollAmount;
        TargetZoom = Mathf.Clamp(TargetZoom, minZoom, maxZoom);
        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, TargetZoom, ZoomSpeed * Time.deltaTime);
    }
}
