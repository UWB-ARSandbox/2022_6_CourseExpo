using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnouncementScreen : MonoBehaviour
{
    public GameObject List;
    public GameObject AnnounceObject;
    public List<string> Announcements;
    public AnnouncementManager manager;
    private bool initialized = false;

    public void createList()
    {
        while(Announcements.Count > 0)
        {
            var Announcement = Instantiate(AnnounceObject, List.transform, false) as GameObject;
            Announcement.GetComponent<SinglelineContainer>().setText(Announcements[0]);
            Announcements.RemoveAt(0);
        }
        /*foreach(string var in Announcements)
        {
            var Announcement = Instantiate(AnnounceObject, List.transform, false) as GameObject;
            Announcement.GetComponent<SinglelineContainer>().setText(var);
        }*/
    }

    public void addToList(string var)
    {
        Announcements.Add(var);
    }

    public void resetList()
    {
        foreach (Transform child in List.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void flipScreen()
    {
        if (gameObject.activeSelf == false)
        {
            Cursor.lockState = CursorLockMode.None;
            gameObject.SetActive(true);
            //resetList();
            createList();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.SetActive(false);
        }
    }
}
