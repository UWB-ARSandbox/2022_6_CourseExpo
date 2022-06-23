using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platformer_CameraMove : MonoBehaviour
{
    public float LeftBound;
    public float RightBound;
    public float TopBound;
    public float BottomBound;
    public float CameraMoveSpeed = 10f;

    Platformer_Player player;
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        //offset = transform.position - player.transform.position;
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = player.transform.position;

            endPos.x += offset.x;
            endPos.y += offset.y;
            endPos.z = -10;
            transform.position = Vector3.Lerp(startPos, endPos, CameraMoveSpeed * Time.fixedDeltaTime);

            transform.position = new Vector3
                (
                    Mathf.Clamp(transform.position.x, LeftBound, RightBound),
                    Mathf.Clamp(transform.position.y, BottomBound, TopBound),
                    transform.position.z
                );
        }
    }

    public void SetUpCamera(Platformer_Player _player)
    {
        player = _player;
        offset = transform.position - player.transform.position;
    }
}
