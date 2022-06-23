using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    [Range(1, 50)]
    public int MaxGrabbableObjects = 20;

    [Range(1.0f, 3.0f)]
    public float GrabRange = 1.5f;

    private static Queue<GrabbableObject> _grabbableObjects = new Queue<GrabbableObject>();

    private Vector3 _initialScale = Vector3.zero;

    private Rigidbody _rb = null;

    private GameObject player = null;

    private void Awake()
    {
        _rb = transform.GetComponent<Rigidbody>();
    }

    public void Start()
    {
        _initialScale = transform.localScale;
        _grabbableObjects.Enqueue(this);
        if (_grabbableObjects.Count > MaxGrabbableObjects)
        {
            var toDelete = _grabbableObjects.Dequeue();
            Destroy(toDelete.gameObject);
        }
    }

    public void Update() {
        if (player != null) {
            Vector3 initialGrabPoint = Vector3.Lerp(player.transform.position, Camera.main.transform.position, 0.3f);
            transform.parent.transform.position = initialGrabPoint + (player.transform.forward * GrabRange);
            transform.localPosition = Vector3.zero;
            transform.parent.transform.LookAt(Camera.main.transform);
        }
    }

    public void Grab(GameObject grabPlayer)
    {
        _rb.useGravity = false;
        _rb.detectCollisions = false;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        player = grabPlayer;
        //transform.SetParent(grabPlayer.transform);
        //transform.localPosition = Vector3.forward * GrabRange;
    }

    public void LetGo()
    {
        _rb.useGravity = true;
        _rb.detectCollisions = true;

        player = null;
        //transform.SetParent(null);
        //transform.localScale = _initialScale;
    }
}
