using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCamera : MonoBehaviour
{
    public Camera CameraToCopy;
    Camera thisCamera;
    // Start is called before the first frame update
    void Start()
    {
        thisCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        thisCamera.depth = CameraToCopy.depth;
        thisCamera.orthographic = CameraToCopy.orthographic;
        thisCamera.fieldOfView = CameraToCopy.fieldOfView;
        thisCamera.transform.SetPositionAndRotation(CameraToCopy.transform.position, CameraToCopy.transform.rotation);
        thisCamera.transform.localScale = CameraToCopy.transform.localScale;
    }
}
