using UnityEngine;

public class PivotRotation : MonoBehaviour
{
    private Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.rotation = cameraTransform.rotation;
    }
}
