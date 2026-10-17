using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    public Transform target;
public float Cameraspeed ;
public float minX, maxX, minY, maxY;
Vector2 newCamPosition;
    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (target != null)
        {
            newCamPosition = Vector2.Lerp(transform.position, target.position, Cameraspeed * Time.deltaTime);
           float ClampX = Mathf.Clamp(newCamPosition.x, minX, maxX);
           float ClampY = Mathf.Clamp(newCamPosition.y, minY, maxY);
           transform.position = new Vector3(ClampX, ClampY, -10f);
        }



    }
}
