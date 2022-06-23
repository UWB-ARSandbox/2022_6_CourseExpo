using Aws.GameLift.Realtime.Event;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Aws.GameLift.Realtime.Command;
using Aws.GameLift.Realtime.Types;
using UnityEngine.SceneManagement;
using System;

namespace ASL
{
    public partial class GameLiftManager
    {
        /// <summary>
        /// Internal class used to help transition between scenes for all users
        /// </summary>
        private class SceneLoader
        {
            /// <summary>Text used to indicate how the loading progress is coming along</summary>
            private Text m_LoadingProgressTest;
            /// <summary>Flag indicating that all players are finished loading, triggering the scene activation </summary>
            public bool m_AllPlayersLoaded = false;
            /// <summary> Local flag indicating this user has loaded, allowing them to send a packet to the RT server to communicate with others that they are ready </summary>
            private bool m_Loaded = false;

            /// <summary>
            /// Changes the AllPlayersLoaded flag to true
            /// </summary>
            public void LaunchScene()
            {
                m_AllPlayersLoaded = true;
            }

            /// <summary>
            /// Used when a user sends a packet to change scenes
            /// </summary>
            /// <param name="_packet"></param>
            public void LoadScene(DataReceivedEventArgs _packet)
            {
                string sceneName;
                string callbackId = GetInstance().m_NullCallbackIdInString;
                try
                {
                    (int[] startLocation, int[] dataLength) = GetInstance().m_GameController.DataLengthsAndStartLocations(_packet.Data);
                    callbackId = GetInstance().m_GameController.ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                    sceneName = GetInstance().m_GameController.ConvertByteArrayIntoString(_packet.Data, startLocation[1], dataLength[1]);
                } catch (OutOfMemoryException oe)
                {
                    sceneName = Encoding.Default.GetString(_packet.Data);
                }
                SceneManager.LoadScene("ASL_SceneLoader");
                GetInstance().StartCoroutine(AsyncSceneLoader(sceneName, callbackId));
            }

            /// <summary>
            /// Asynchronously loads a scene and once loaded, informs everyone else that this user is ready to go into the newly loaded scene
            /// </summary>
            /// <param name="_sceneName">The name of the scene to be loaded</param>
            /// <param name="_callbackId">The callback id for a specific OpFunction callback</param>
            /// <param name="_opCode">The OpCode</param>
            /// <returns>Null while loading</returns>
            private IEnumerator AsyncSceneLoader(string _sceneName, string _callbackId)
            {
                while (SceneManager.GetActiveScene().name != "ASL_SceneLoader")
                {
                    yield return null;
                }
                m_LoadingProgressTest = GameObject.Find("LoadingProgress").GetComponent<Text>();
                //Begin to load scene specified
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneName);

                //Don't let the scene activate until all users have it loaded (done via RT server)
                asyncOperation.allowSceneActivation = false;

                //While the scene is loading - output the progress
                while (!asyncOperation.isDone)
                {
                    m_LoadingProgressTest.text = "\n\nLoading Progress: " + (asyncOperation.progress * 100) + "%";
                    //Check if scene is finished loading:
                    if (asyncOperation.progress >= 0.9f && !asyncOperation.allowSceneActivation)
                    {
                        if (!m_Loaded)
                        {
                            m_Loaded = true; //Prevent multiple packets from sending unnecessarily
                            RTMessage message = GetInstance().CreateRTMessage(OpCode.LaunchScene, Encoding.ASCII.GetBytes(""));
                            GetInstance().m_Client.SendMessage(message);
                        }
                        //Change to text to inform user that they are now waiting on others
                        m_LoadingProgressTest.text = "\n\nFinished Loading. Waiting for other players...";
                        if (m_AllPlayersLoaded)
                        {
                            m_AllPlayersLoaded = false;
                            asyncOperation.allowSceneActivation = true;
                            m_Loaded = false;
                        }
                    }
                    yield return null;
                }

                GetInstance().DoOpFunctionCallback(_callbackId);
            }


        }


    }
}
