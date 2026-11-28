using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public float Cameraspeed;
    public float minX, maxX;
    public float minY, maxY;

    private Camera cam;
    private float startSize; //the size of the camera when the level starts

    void Start()
    {
        cam = GetComponent<Camera>();
        startSize = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        if (Target != null)
        {
            Vector2 newCamPosition = Vector2.Lerp(transform.position,Target.position, Cameraspeed * Time.deltaTime);

            //minX and maxX are set for a 16:9 screen at the start size.
            //when the camera zooms in, or the screen is narrower, it sees less, so it can move closer to the edges.
            //when the screen is wider (like Free Aspect), it sees more, so it must stay further from the edges.
            float extraX = startSize * 16f / 9f - cam.orthographicSize * cam.aspect;
            float extraY = startSize - cam.orthographicSize;

            float ClampX = Mathf.Clamp(newCamPosition.x, minX - extraX, maxX + extraX);
            float ClampY = Mathf.Clamp(newCamPosition.y, minY - extraY, maxY + extraY);
            transform.position = new Vector3(ClampX, ClampY, -10f);
        }
    }
    
}
