using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public float Cameraspeed;
    public float minX, maxX;
    public float minY, maxY;

    [Header("Optional: keep the view inside the level art")]
    [Tooltip("When on, the camera never shows past these world-space edges, whatever the zoom.")]
    public bool clampToLevelBounds = false;
    public Vector2 levelBoundsMin;
    public Vector2 levelBoundsMax;

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
            Vector2 newCamPosition = Vector2.Lerp(transform.position, Target.position, Cameraspeed * Time.deltaTime);
            float ClampX;
            float ClampY;

            if (clampToLevelBounds && cam != null && cam.orthographic)
            {
                //level 3: the view never goes past the edges of the level art
                ClampX = Mathf.Clamp(newCamPosition.x, minX, maxX);
                ClampY = Mathf.Clamp(newCamPosition.y, minY, maxY);

                float halfHeight = cam.orthographicSize;
                float halfWidth = halfHeight * cam.aspect;
                ClampX = ClampToBounds(ClampX, levelBoundsMin.x + halfWidth, levelBoundsMax.x - halfWidth);
                ClampY = ClampToBounds(ClampY, levelBoundsMin.y + halfHeight, levelBoundsMax.y - halfHeight);
            }
            else
            {
                //when the camera zooms in (CameraZoom) it sees less, so it can move closer to the edges of the level
                float extraX = (startSize - cam.orthographicSize) * 16f / 9f; //the game screen is 16:9
                float extraY = startSize - cam.orthographicSize;

                ClampX = Mathf.Clamp(newCamPosition.x, minX - extraX, maxX + extraX);
                ClampY = Mathf.Clamp(newCamPosition.y, minY - extraY, maxY + extraY);
            }

            transform.position = new Vector3(ClampX, ClampY, -10f);
        }
    }

    static float ClampToBounds(float value, float low, float high)
    {
        // If the view is wider than the level, centre it.
        if (low > high) return (low + high) * 0.5f;
        return Mathf.Clamp(value, low, high);
    }
}
