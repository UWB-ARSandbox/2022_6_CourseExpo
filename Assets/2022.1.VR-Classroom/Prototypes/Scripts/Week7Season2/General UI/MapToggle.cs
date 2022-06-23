using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapToggle : MonoBehaviour
{
    public GameObject Minimap;
    public GameObject OverviewMap;
    
    public enum MapState {
        Hidden,
        Minimap,
        FullMap
    }
    public MapState mapState = MapState.Hidden;

    public void ToggleMap() {
        switch (mapState) {
            case MapState.Hidden:
                Minimap.SetActive(true);
                mapState = MapState.Minimap;
                break;
            case MapState.Minimap:
                mapState = MapState.FullMap;
                Minimap.SetActive(false);
                OverviewMap.SetActive(true);
                break;
            case MapState.FullMap:
                OverviewMap.SetActive(false);
                mapState = MapState.Hidden;
                break;
        }
    }
}
