using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionDemoHandler : MonoBehaviour
{
    public Image Chart;
    bool displayChart = false;
    ObjectWithCollider selectedObject;

    private void Update()
    {
        ObjectWithCollider demoObject = null;
        ObjectWithCollider previousObject = null;
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f))
            {
                demoObject = hit.transform.GetComponent<ObjectWithCollider>();
            }
            //clicking on the selected object
            if (selectedObject == demoObject && selectedObject != null)
            {
                selectedObject.SelectObject(false);
                selectedObject.GetComponent<MeshRenderer>().material.color = selectedObject.DefaultColor;
                selectedObject = null;
            }
            //clicking on nothing
            else if (demoObject == null)
            {
                if (selectedObject != null)
                {
                    selectedObject.SelectObject(false);
                    selectedObject.GetComponent<MeshRenderer>().material.color = selectedObject.DefaultColor;
                    selectedObject = null;
                }
            }
            //clicking on an object while no object is selected
            else if (selectedObject == null)
            {
                selectedObject = demoObject;
                selectedObject.SelectObject(true);
                selectedObject.GetComponent<MeshRenderer>().material.color = selectedObject.SelectedColor;
            }
            //clicking on another object while one is selected
            else
            {
                previousObject = selectedObject;
                selectedObject = demoObject;
                selectedObject.SelectObject(true);
                selectedObject.GetComponent<MeshRenderer>().material.color = selectedObject.SelectedColor;

                previousObject.SelectObject(false);
                previousObject.GetComponent<MeshRenderer>().material.color = previousObject.DefaultColor;
                previousObject = null;
            }
        }
    }

    public void ToggleChart()
    {
        displayChart = !displayChart;
        Chart.enabled = displayChart;
    }
}
