using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class NameControls : MonoBehaviour
{
    public Transform camera;
    GameLiftManager manager;
    
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.transform;
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();

        if(manager.m_PeerId == manager.GetLowestPeerId() && this.transform.parent.parent.gameObject.name == ("Teacher2(Clone)"))
        {
            GetComponent<TextMesh>().color = new Vector4(0, 0, 0, 0);
        }    
        else if(manager.m_PeerId == transform.parent.parent.GetComponent<StudentEnable>().studentID)
        {
            GetComponent<TextMesh>().color = new Vector4(0, 0, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.rotation = Quaternion.LookRotation((transform.position - camera.position).normalized);
    }
}
