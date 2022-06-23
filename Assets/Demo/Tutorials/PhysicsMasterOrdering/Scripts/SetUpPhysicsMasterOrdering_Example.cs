using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class SetUpPhysicsMasterOrdering_Example : MonoBehaviour
{
    /// <summary>Displays the mode information</summary>
    public Text m_DisplayInformation;

    /// <summary>Displays the player's peer id</summary>
    public Text m_DisplayPlayerInformation;

    /// <summary>Determines if the player is the phhysics master at this moment</summary>
    public Text m_DisplayPhysicsMasterInformation;

    /// <summary>Determines which mode the example is in: Defualt/LowestPeer/HighestPeer/Custom</summary>
    public int m_ModeIndex = 0;

    /// <summary>
    /// Used to make adjustment on the information and the mode according to user-selected mode.
    /// </summary>
    void Start()
    {
        UserSelection(m_ModeIndex);
        DisplayModeInformation(m_ModeIndex);
        m_DisplayPlayerInformation.text = "My id is: " + ASL.GameLiftManager.GetInstance().m_PeerId.ToString();
    }

    /// <summary>
    /// Used to make updates on the player's physics master status.
    /// </summary>
    void Update()
    {
        bool isPhysicsMaster = ASL_PhysicsMasterSingleton.Instance.IsPhysicsMaster;
        m_DisplayPhysicsMasterInformation.text = "I am the Physics Master: " + (isPhysicsMaster? "YES" : "NO");
    }

    /// <summary>
    /// Used to make physics master decision base on selected mode.
    /// </summary>
    /// <param name="index">The integer value received for determining mode</param>
    void UserSelection(int index)
    {
        switch (index)
        {
            case 0:
                ASL_PhysicsMasterSingleton.Instance.SetUpPhysicsMaster();
                return;
            case 1:
                ASL_PhysicsMasterSingleton.Instance.SetUpPhysicsMasterByLowestPeer();
                return;
            case 2:
                ASL_PhysicsMasterSingleton.Instance.SetUpPhysicsMasterByHighestPeer();
                return;
            case 3:
                ASL_PhysicsMasterSingleton.Instance.SetUpPhysicsMasterByCustomFunction(DefinePhysicsMasterCallback, DefinePhysicsMasterIdCallback);
                return;
            default:
                return;
        }
    }

    /// <summary>
    /// Used to display information about the selected mode.
    /// </summary>
    /// <param name="index">The integer value received for determining mode</param>
    void DisplayModeInformation(int index)
    {
        switch (index)
        {
            case 0:
                m_DisplayInformation.text = "Default: the player who entered the game earliest is the Physics Master.";
                return;
            case 1:
                m_DisplayInformation.text = "By lowest peer: the player who entered the game earliest is the Physics Master.";
                return;
            case 2:
                m_DisplayInformation.text = "By highest peer: the player who entered the game latest is the Physics Master.";
                return;
            case 3:
                m_DisplayInformation.text = "By custom function (by highest peer): a custom function is used for selecting the Physics Master.";
                return;
            default:
                return;
        }
    }

    /// <summary>
    /// Used to define a call back function which contains the algorithm of the physics master decision making.
    /// </summary>
    void DefinePhysicsMasterCallback()
    {
        ASL_PhysicsMasterSingleton.Instance.SetPhysicsMaster(ASL.GameLiftManager.GetInstance().AmHighestPeer());
    }

    /// <summary>
    /// Used to define a call back function which contains the same/similar algorithm of the physics master decision making.
    /// But in this case, it needs to return the player's id who is the physics master.
    /// </summary>
    /// <returns>Selected physics master's peer id which is determined by the algorithm</returns>
    int DefinePhysicsMasterIdCallback()
    {
        return ASL.GameLiftManager.GetInstance().GetHighestPeerId();
    }
}
