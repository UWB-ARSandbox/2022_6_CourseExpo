using UnityEngine;
using UnityEngine.InputSystem;

public class MapNavigation : MonoBehaviour
{
    private float initialZoom;
    private float curZoom = 1f;
    private Camera mapCam;

    public float zoomSpeed = 0.1f;
    public float maxZoom = 10f;
    public float panSpeed = 1f;

    private void Start()
    {
        mapCam = GameObject.Find("Overview Map Camera").GetComponent<Camera>();
        initialZoom = 100f;
    }

    public void Update()
    {
        float scrollAmount = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollAmount) > 0.01) {
            float delta = scrollAmount * zoomSpeed;
            float desiredScale = mapCam.orthographicSize - delta;

            desiredScale = ClampDesiredScale(desiredScale);
            mapCam.orthographicSize = desiredScale;
            curZoom = initialZoom / desiredScale;
        }
    }

    public void ResetMapZoom() {
        mapCam.orthographicSize = initialZoom;
    }

    public void PanCamera(Vector2 input) {
        float horizontalInput = input.x;
        float verticalInput = input.y;
        if (Mathf.Abs(horizontalInput) > 0 || Mathf.Abs(verticalInput) > 0) {
            float panSpeedMultiplier = panSpeed / curZoom;
            Vector3 newPos = new Vector3(mapCam.transform.position.x - verticalInput * panSpeedMultiplier, mapCam.transform.position.y, mapCam.transform.position.z + horizontalInput * panSpeedMultiplier);
            mapCam.transform.position = newPos;
        }
    }

    private float ClampDesiredScale(float desiredScale)
    {
        desiredScale = Mathf.Min(initialZoom, desiredScale);
        desiredScale = Mathf.Max(initialZoom / maxZoom, desiredScale);
        return desiredScale;
    }
}