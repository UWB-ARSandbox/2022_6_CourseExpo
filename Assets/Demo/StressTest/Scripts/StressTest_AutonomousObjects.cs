using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

namespace StressTesting
{
    /// <summary>
    /// Stress test for moving ASL_AutonomousObjects. Instantiates 20 opjects per player, up to a maximum of 3 players.
    /// Objects move back and forth on screen.
    /// </summary>
    public class StressTest_AutonomousObjects : MonoBehaviour
    {
        public GameObject AutonomousObjectPrefab;
        const int OBJECTS_PER_PLAYER = 20;

        // Start is called before the first frame update
        void Start()
        {
            switch (ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                case 1:
                    createAutonomousObjects(new Vector3(-6, 4, 0));
                    break;
                case 2:
                    createAutonomousObjects(new Vector3(-6, 0, 0));
                    break;
                case 3:
                    createAutonomousObjects(new Vector3(-6, -4, 0));
                    break;
                default:
                    break;
            }
        }

        void createAutonomousObjects(Vector3 startPos)
        {
            Vector3 pos = startPos;
            //ASL_AutonomousObjectHandler.Instance.InstantiateAutonomousObject(AutonomousObjectPrefab, pos, AutonomousObjectPrefab.transform.rotation);
            for (int i = 0; i < OBJECTS_PER_PLAYER; i++)
            {
                ASL_AutonomousObjectHandler.Instance.InstantiateAutonomousObject(AutonomousObjectPrefab, pos, AutonomousObjectPrefab.transform.rotation);
                if (i == (OBJECTS_PER_PLAYER / 2) - 1)
                {
                    pos = startPos;
                    pos.y -= 1;
                }
                else
                {
                    pos.x += 1.3f;
                }
            }
        }
    }
}