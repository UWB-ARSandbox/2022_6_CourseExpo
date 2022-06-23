using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleDemos
{
    /// <summary>Example of how to set a OpFunction callback</summary>
    public class SetCallback_Example : MonoBehaviour
    {
        /// <summary>Pre-defined value for number of iterations for the process (create-move-delete). </summary>
        public int m_RepeatTimes = 10;
        /// <summary>Pre-defined value for how many cubes to create. </summary>
        public int m_NumOfCubes = 4;
        /// <summary>Pre-derfined max value for a cube to be deleted after this amount of moving time. </summary>
        public float m_MaxRunningLengthInSecond = 3;
        /// <summary>Pre-derfined min value for a cube to be deleted after this amount of moving time. </summary>
        public float m_MinRunningLengthInSecond = 1;
        /// <summary>Pre-derfined max value for movement. </summary>
        public float m_MaxAdditiveMovementAmount = 0.1f;
        /// <summary>Pre-derfined min value for movement. </summary>
        public float m_MinAdditiveMovementAmount = -0.1f;

        /// <summary>Boolean value for determining if a new set of cubes should be spawned. </summary>
        private bool _shouldSpawn = true;
        /// <summary>Boolean value for determining if a new set of cubes should be spawned. </summary>
        private bool _shouldRun = false;
        /// <summary>Value for number of the iterations have done. </summary>
        private int m_CurrentCount = 0;
        /// <summary>Value for the running length in second for this current iteration. </summary>
        private float m_CurrentRunningLengthInSecond;
        /// <summary>Value for the addtive movement amount for this current iteration. </summary>
        private Vector3 m_CurrentAdditiveMovementAmount;
        /// <summary>Set for storing a cube's last existing position when it is deleted. </summary>
        private HashSet<Vector3> m_CurrentDeletionPositions = new HashSet<Vector3>();
        /// <summary>Dictinary for storing cubes and its moving time </summary>
        private static Dictionary<GameObject, float> m_MyCubesAndTimerDictionary = new Dictionary<GameObject, float>();

        /// <summary>Game Logic</summary>

        /// <summary>Initialize the very first set of cubes and settings.
        /// Each iteration will generate a new set of settings with randomness.
        /// </summary>
        void Start()
        {
            Debug.Log("Spawning...");
            m_CurrentCount++;
            SetSettings();
            SpawnObjects();
        }

        /// <summary> Spawns a specific amount of objects(cubes) within specific positions.
        /// If spawning the very first set of objects, the positions are fixed,
        /// Otherwise, the positions where spawning objects are read from m_CurrentDeletionPositions and plus randomness.
        /// </summary>
        private void SpawnObjects()
        {
            if (m_CurrentDeletionPositions.Count == 0)
            {
                for (int i = 0; i < m_NumOfCubes; i++)
                {
                    ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Cube,
                        new Vector3(Random.Range(-4f, 1f), Random.Range(0f, 0.5f), Random.Range(-2f, 2f)),
                        Quaternion.identity, "", "",
                        CubeCreatedCallback);
                }
            }
            else
            {
                foreach (Vector3 _deletePosition in m_CurrentDeletionPositions)
                {
                    ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Cube,
                       _deletePosition + new Vector3(Random.Range(0.01f, 0.05f), Random.Range(0.01f, 0.05f), Random.Range(0.01f, 0.05f)),
                       Quaternion.identity, "", "",
                       CubeCreatedCallback);
                }
                m_CurrentDeletionPositions.Clear();
            }
            _shouldRun = true;
            _shouldSpawn = false;
        }

        /// <summary> Generates a new set of settings with the given ranges. </summary>
        private void SetSettings()
        {
            m_CurrentRunningLengthInSecond = Random.Range(m_MinRunningLengthInSecond, m_MaxRunningLengthInSecond);
            m_CurrentAdditiveMovementAmount = new Vector3(
                Random.Range(m_MinAdditiveMovementAmount, m_MaxAdditiveMovementAmount),
                Random.Range(m_MinAdditiveMovementAmount, m_MaxAdditiveMovementAmount),
                Random.Range(m_MinAdditiveMovementAmount, m_MaxAdditiveMovementAmount));
        }

        /// <summary>Updates cubes' movement in the dictionary
        /// If a cube reaches its moving time limit, it will be deleted by the OpFunction callback,
        /// Otherwise, keep moving.
        /// </summary>
        void Update()
        {
            if (_shouldRun)
            {
                foreach (GameObject _cube in m_MyCubesAndTimerDictionary.Keys.ToList())
                {
                    float timer = m_MyCubesAndTimerDictionary[_cube];
                    timer += Time.deltaTime;
                    if ((float)timer % 60 > m_CurrentRunningLengthInSecond)
                    {
                        Debug.Log("Removing...");
                        m_MyCubesAndTimerDictionary.Remove(_cube);
                        _cube.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                        {
                            _cube.GetComponent<ASL.ASLObject>().SendAndIncrementLocalPosition(m_CurrentAdditiveMovementAmount, EndOfMovementCallback);
                        });
                    }
                    else
                    {
                        _cube.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                        {
                            _cube.GetComponent<ASL.ASLObject>().SendAndIncrementLocalPosition(m_CurrentAdditiveMovementAmount);
                        });
                        m_MyCubesAndTimerDictionary[_cube] = timer;
                    }
                }
            }
        }

        /// <summary>InstantiateASLObject function's callback</summary>
        private static void CubeCreatedCallback(GameObject _object)
        {
            m_MyCubesAndTimerDictionary.Add(_object, 0);
        }

        /// <summary>OpFunction's callback : IncrementLocalPosition
        /// To delete the given object
        /// </summary>
        private void EndOfMovementCallback(GameObject _object)
        {
            _object.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                _object.GetComponent<ASL.ASLObject>().DeleteObject(EndOfDeletionCallback);
            });
        }

        /// <summary>OpFunction's callback : DeleteObject
        /// To add the very last existing position to the set,
        /// and to check if all objcest in the current iteration have finished the process (been deleted),
        /// If yes, spawns a new set of objects with the new settings.
        /// </summary>
        private void EndOfDeletionCallback(GameObject _object)
        {
            m_CurrentDeletionPositions.Add(_object.transform.position);
            if (m_MyCubesAndTimerDictionary.Count == 0 && m_CurrentDeletionPositions.Count == m_NumOfCubes && !_shouldSpawn)
            {
                _shouldRun = false;
                _shouldSpawn = true;
                Debug.Log("Spawning...");
                m_CurrentCount++;
                if (m_CurrentCount > m_RepeatTimes)
                {
                    Debug.Log("Ended.");
                    return;
                }
                SetSettings();
                SpawnObjects();
            }
        }
    }
}