using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.XR;
using System.Text;
using System.Linq;

public class SaveStats : MonoBehaviour
{
    string path = "";
    public GameObject Player = null;
    public PersonalStats PlayerStats;
    public StatsManager statsManager;

    public List<string> StudentNames;
    public Dictionary<string, PersonalStats.BoothStats> checker;
    public int pos = 0;
    public int length = 0;

    // Update is called once per frame
    void Update()
    {
        if (Player == null)
        {
            Player = GameObject.Find("FirstPersonPlayer(Clone)");
            PlayerStats = Player.GetComponent<PersonalStats>();
        }
    }

    public void StatsSave()
    {
        var extension = new[]
        {
            //new SFB.ExtensionFilter("Text", "txt"),
            new SFB.ExtensionFilter("CSV", "csv")
        };
        if (XRSettings.isDeviceActive) Cursor.lockState = CursorLockMode.None;
        SFB.StandaloneFileBrowser.SaveFilePanelAsync("Save File", Application.dataPath, "Stats", extension, writeToSave);
    }

    public void writeToSave(string strs)
    {        
        StringBuilder writer = new StringBuilder();

        if (GameManager.AmTeacher)
        {
            foreach (var item in statsManager.studentStats)
            {
                //top line: name, stats categories
                writer.AppendLine($"{item.Key},Time in booth,Time taken to complete,Questions timed out,Score,Completed");
                PersonalStats studentStats = item.Value;

                //sort the names before exporting
                studentStats.boothStats = studentStats.boothStats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                foreach (var boothStats in studentStats.boothStats)
                {
                    //each booth line: Name, score, completed
                    writer.AppendLine($"{boothStats.Key},{boothStats.Value.OutputStats()}");
                }
                writer.AppendLine();
            }
        }
        else
        {
            //sort the names before exporting
            PlayerStats.boothStats = PlayerStats.boothStats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            writer.AppendLine($",Time in booth,Time taken to complete,Questions timed out,Score,Completed");
            foreach (var item in PlayerStats.boothStats)
            {
                writer.AppendLine($"{item.Key},{item.Value.OutputStats()}");
            }
        }

        File.WriteAllText(strs, writer.ToString());
    }
}
