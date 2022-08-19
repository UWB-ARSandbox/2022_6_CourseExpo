using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Unused, implementation is now within CanvasInput
public class GraphicRaycastFix : MonoBehaviour
{
    GraphicRaycaster mRaycaster;
    // Start is called before the first frame update
    void Start()
    {
        mRaycaster = GetComponent<GraphicRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
            {
                List<RaycastResult> results = new List<RaycastResult>();
                PointerEventData data = new PointerEventData(EventSystem.current);
                data.position = new Vector2(Screen.width / 2, Screen.height / 2);
                data.pressPosition = new Vector2(Screen.width / 2, Screen.height / 2);
                
                
                EventSystem.current.RaycastAll(data, results);
                
                foreach(RaycastResult result in results)
                {
                    //Debug.Log(result.gameObject.name);
                    if(result.gameObject.name == "Background")
                    {
                        
                        result.gameObject.transform.parent.GetComponent<Slider>().OnDrag(data);
                        
                    }
                    
                }
            }
    }
}
