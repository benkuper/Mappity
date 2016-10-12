// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MouseOrbitImproved : OSCControllable
{
    public Transform target;

    [OSCProperty("target")]
    public Vector3 targetOffset;

    [OSCProperty("distance")]
    public float distance = 5.0f;

    public float xSpeed = 20.0f;
    public float ySpeed = 20.0f;

    float yMinLimit = -90;
    float yMaxLimit = 90;

    float distanceMin = 0.1f;
    float distanceMax = 500;

    [OSCProperty("x")]
    public float x = 0.0f;
    [OSCProperty("y")]
    public float y = 0.0f;

    [OSCProperty("smooth")]
    public float smoothTime = 0.2f;

    private float xSmooth = 0.0f;
    private float ySmooth = 0.0f;
    private float distSmooth = 0.0f;
    private float txSmooth = 0.0f;
    private float tySmooth = 0.0f;
    private float tzSmooth = 0.0f;

    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float distVelocity = 0.0f;
    private float txVelocity = 0.0f;
    private float tyVelocity = 0.0f;
    private float tzVelocity = 0.0f;


    public override void Start()
    {
    }

    public override void Update()
    {
        if (target)
        {
            if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {

                x += Input.GetAxis("Mouse X") * xSpeed;
                y -= Input.GetAxis("Mouse Y") * ySpeed;

                y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

            }

            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * distance, distanceMin, distanceMax);
            }

            if (Application.isPlaying && smoothTime > 0)
            {
                xSmooth = Mathf.SmoothDamp(xSmooth, x, ref xVelocity, smoothTime);
                ySmooth = Mathf.SmoothDamp(ySmooth, y, ref yVelocity, smoothTime);
                distSmooth = Mathf.SmoothDamp(distSmooth, distance, ref distVelocity, smoothTime);
                txSmooth = Mathf.SmoothDamp(txSmooth, targetOffset.x, ref txVelocity, smoothTime);
                tySmooth = Mathf.SmoothDamp(tySmooth, targetOffset.y, ref tyVelocity, smoothTime);
                tzSmooth = Mathf.SmoothDamp(tzSmooth, targetOffset.z, ref tzVelocity, smoothTime);
            }
            else
            {
                xSmooth = x;
                ySmooth = y;
                distSmooth = distance;
                txSmooth = targetOffset.x;
                tySmooth = targetOffset.y;
                tzSmooth = targetOffset.z;
            }

            transform.localRotation = Quaternion.Euler(ySmooth, xSmooth, 0);
            transform.position = transform.rotation * new Vector3(0.0f, 0.0f, -distSmooth) + target.position + new Vector3(txSmooth, tySmooth, tzSmooth);

        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(target.transform.position+targetOffset, Vector3.one * 1f);
    }
}