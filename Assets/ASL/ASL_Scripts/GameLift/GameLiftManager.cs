﻿//Used for help debug GameLift packet issues and other misc. GameLift potential problems.
#define ASL_DEBUG
using Aws.GameLift.Realtime.Event;
using Aws.GameLift.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aws.GameLift.Realtime.Command;
using Aws.GameLift.Realtime.Types;
using System.Collections;

namespace ASL
{
    /// <summary>
    /// The class that makes all multiplayer possible
    /// </summary>
    public partial class GameLiftManager : MonoBehaviour
    {
        /// <summary>
        /// An internal class that is used to create and contain information about the Player Session that this user creates when joining a Game Session
        /// </summary>
        [System.Serializable]
        private class PlayerSessionObject
        {
            public string PlayerSessionId = null;
            public string PlayerId = null;
            public string GameSessionId = null;
            public string FleetId = null;
            public string CreationTime = null;
            public string Status = null;
            public string IpAddress = null;
            public string DnsName = null;
            public string Port = null;
            public string GameName = null;
            public string SceneName = null;
        }

        /// <summary>
        /// An internal class that contains all of the GameSessionObjects found by this user
        /// </summary>
        [System.Serializable]
        private class GameSessionObjectCollection
        {
            public List<GameSessionObject> GameSessions = null;
        }

        /// <summary>
        /// An internal class containing information about the GameSession this user has found or joined
        /// </summary>
        [System.Serializable]
        private class GameSessionObject
        {
            public string GameSessionId = null;
            public string Name = null;
            public string CurrentPlayerSessionCount = null;
            public string MaximumPlayerSessionCount = null;
            public string IpAddress = null;
            public string Port = null;
        }

        /// <summary>
        /// The singleton instance for this class
        /// </summary>
        private static GameLiftManager m_Instance;

        /// <summary>
        /// Internal class used to setup and connect users to each other
        /// </summary>
        private LobbyManager m_LobbyManager;

        /// <summary>
        /// Internal class used to load scenes for all users
        /// </summary>
        private SceneLoader m_SceneLoader;

        /// <summary>
        /// Internal class used to decoded packets received from the AWS
        /// </summary>
        private GameController m_GameController;

        /// <summary>
        /// This current user's username
        /// </summary>
        public string m_Username { get; private set; }

        /// <summary>
        /// The name of the game session the user is currently in
        /// </summary>
        public string m_GameSessionName { get; private set; }

        /// <summary>
        /// This current user's peerId
        /// </summary>
        public int m_PeerId { get; private set; }

        /// <summary>
        /// Dictionary containing the all users that are connected peerIds and usernames
        /// </summary>
        public Dictionary<int, string> m_Players = new Dictionary<int, string>();

        /// <summary>
        /// The id of the server,  used to send messages to the server
        /// </summary>
        public int m_ServerId { get; private set; }

        /// <summary>
        /// The group ID used to communicate with all users
        /// </summary>
        public int m_GroupId { get; private set; }

        /// <summary>
        /// The AWS client variable that allows a connection to and the ability to communicate with GameLift
        /// </summary>
        public Client m_Client { get; private set; }

        public DataReceivedEventArgs m_Packet { get; private set; }

        /// <summary>
        /// The peer id of the current selected physics master
        /// </summary>
        private int m_PhysicsMasterId;

        /// Pre-defined callback function for a specific OpFunction
        /// </summary>
        public delegate void OpFunctionCallback(GameObject m_object);

        /// Pre-defined callback function for a specific OpFunction without parameter
        /// </summary>
        public delegate void OpFunctionCallbackNoParam();

        /// <summary>
        /// Dictionary containing the all callbacks that are connected OpFunction's OpCode
        /// </summary>
        public Dictionary<string, OpFunctionCallback> OpFunctionCallbacks = new Dictionary<string, OpFunctionCallback>();

        /// <summary>
        /// Dictionary containing the all callbacks without parameter that are connected OpFunction's OpCode
        /// </summary>
        public Dictionary<string, OpFunctionCallbackNoParam> OpFunctionCallbacksNoParam = new Dictionary<string, OpFunctionCallbackNoParam>();

        /// <summary>A value for callback id when the given null as callback. </summary>
        public static byte[] m_NullCallbackId = new byte[55];

        /// <summary>A value for callback id in string format when the given null as callback. </summary>
        public string m_NullCallbackIdInString { get; } = System.Text.Encoding.Default.GetString(m_NullCallbackId);

        /// <summary>The index value for callback id in data payload. </summary>
        public static int m_callbackIdIndex = 0;
        /// <summary>
        /// Can be any positive number, but must be matched with the OpCodes in the RealTime script.
        /// </summary>
        public enum OpCode
        {
            /// <summary>Packet identifier that indicates a player has logged in</summary>
            PlayerLoggedIn,
            /// <summary>Packet identifier that indicates a player has joined the match</summary>
            PlayerJoinedMatch,
            /// <summary>Packet identifier that indicates a packet that contains the information to add a player to a lobby</summary>
            AddPlayerToLobbyUI,
            /// <summary>Packet identifier that indicates a packet that contains the information on which player disconnected</summary>
            PlayerDisconnected,
            /// <summary>Packet identifier that indicates a packet that contains the information that all players are ready to launch the first scene</summary>
            AllPlayersReady,
            /// <summary>Packet identifier that indicates a packet that contains the information that a player has disconnected before the match began</summary>
            PlayerDisconnectedBeforeMatchStart,
            /// <summary>Packet identifier that indicates a packet that contains the information that a player is ready</summary>
            PlayerReady,
            /// <summary>Packet identifier that indicates a packet that contains the information to launch a scene</summary>
            LaunchScene,
            /// <summary>Packet code for changing the scene</summary>
            LoadScene,
            /// <summary>Packet code for creating an id for an object on the server</summary>
            ServerSetId,
            /// <summary>Packet code to release a claim back to the server</summary>
            ReleaseClaimToServer,
            /// <summary>Packet code representing a claim</summary>
            Claim,
            /// <summary>Packet code informing a player their claim was rejected</summary>
            RejectClaim,
            /// <summary>Packet code informing player who has a claim on an object to release to another player</summary>
            ReleaseClaimToPlayer,
            /// <summary>Packet code informing a player who claimed an object from another player that they have it now</summary>
            ClaimFromPlayer,
            /// <summary>Packet code for setting an object's color</summary>
            SetObjectColor,
            /// <summary>Packet code for deleting an object</summary>
            DeleteObject,
            /// <summary>Packet code representing data that will set the local position of an ASL object</summary>
            SetLocalPosition,
            /// <summary>Packet code representing data that will add to the local position of an ASL object</summary>
            IncrementLocalPosition,
            /// <summary>Packet code representing data that will set the local rotation of an ASL object</summary>
            SetLocalRotation,
            /// <summary>Packet code representing data that will add to the local rotation of an ASL object</summary>
            IncrementLocalRotation,
            /// <summary>Packet code representing data that will set the local scale of an ASL object</summary>
            SetLocalScale,
            /// <summary>Packet code representing data that will add to the local scale of an ASL object</summary>
            IncrementLocalScale,
            /// <summary>Packet code representing data that will set the world position of an ASL object</summary>
            SetWorldPosition,
            /// <summary>Packet code representing data that will add to the world position of an ASL object</summary>
            IncrementWorldPosition,
            /// <summary>Packet code representing data that will set the world rotation of an ASL object</summary>
            SetWorldRotation,
            /// <summary>Packet code representing data that will add to the world rotation of an ASL object</summary>
            IncrementWorldRotation,
            /// <summary>Packet code representing data that will set the world scale of an ASL object</summary>
            SetWorldScale,
            /// <summary>Packet code representing data that will add to the world scale of an ASL object</summary>
            IncrementWorldScale,
            /// <summary>Packet code for spawning a prefab</summary>
            SpawnPrefab,
            /// <summary>Packet code for spawning a primitive object</summary>
            SpawnPrimitive,
            /// <summary>Packet code informing a player float(s) were sent</summary>
            SendFloats,
            /// <summary>Packet code representing data that will be used to recreate a Texture2D as pieces of it come in</summary>
            SendTexture2D,
            /// <summary>Packet code representing data that will be used to resolve a cloud anchor</summary>
            ResolveAnchorId,
            /// <summary>Packet code representing data that will be used to inform the relay server that this user has completed
            /// resolving a cloud anchor and will be received as a flag indicating that all users have resolved this cloud anchor </summary>
            ResolvedCloudAnchor,
            /// <summary>Packet code for updating the AR anchor point</summary>
            AnchorIDUpdate,
            /// <summary>Packet code for sending text messages to other users</summary>
            LobbyTextMessage,
            /// <summary>Used to help keep the Android socket connection alive</summary>
            AndroidKeepConnectionAlive,
            /// <summary>Packet code for sending object tags</summary>
            TagUpdate,
            /// <summary>Packet code representing data that will be used to recreate a Mesh</summary>
            SendARPlaneAsMesh

        }

        /// <summary>
        /// Array of OpCode function references executed when a packet is received with the corresponding OpCode.
        /// null entries in the array are OpCodes that currently have no action within this script when received, 
        /// but may have other action elsewhere or are automatically sent.
        /// </summary>
        public Action[] OpCodeFunctions =
        {
            null, // PlayerLoggedIn - Auto packet sent by GameLift
            () => GetInstance().PlayerJoinedMatch(GetInstance().m_Packet),
            () => GetInstance().AddPlayerToLobbyUI(GetInstance().m_Packet),
            () => GetInstance().PlayerDisconnected(GetInstance().m_Packet),
            () => GetInstance().AllPlayersReady(GetInstance().m_Packet),
            () => GetInstance().PlayerDisconnectedBeforeMatchStart(GetInstance().m_Packet),
            null,   // PlayerReady
            () => GetInstance().LaunchScene(GetInstance().m_Packet),
            () => GetInstance().LoadScene(GetInstance().m_Packet),
            () => GetInstance().ServerSetId(GetInstance().m_Packet),
            null,   // ReleaseClaimToServer
            () => GetInstance().Claim(GetInstance().m_Packet),
            () => GetInstance().RejectClaim(GetInstance().m_Packet),
            () => GetInstance().ReleaseClaimToPlayer(GetInstance().m_Packet),
            () => GetInstance().ClaimFromPlayer(GetInstance().m_Packet),
            () => GetInstance().SetObjectColor(GetInstance().m_Packet),
            () => GetInstance().DeleteObject(GetInstance().m_Packet),
            () => GetInstance().SetLocalPosition(GetInstance().m_Packet),
            () => GetInstance().IncrementLocalPosition(GetInstance().m_Packet),
            () => GetInstance().SetLocalRotation(GetInstance().m_Packet),
            () => GetInstance().IncrementLocalRotation(GetInstance().m_Packet),
            () => GetInstance().SetLocalScale(GetInstance().m_Packet),
            () => GetInstance().IncrementLocalScale(GetInstance().m_Packet),
            () => GetInstance().SetWorldPosition(GetInstance().m_Packet),
            () => GetInstance().IncrementWorldPosition(GetInstance().m_Packet),
            () => GetInstance().SetWorldRotation(GetInstance().m_Packet),
            () => GetInstance().IncrementWorldRotation(GetInstance().m_Packet),
            () => GetInstance().SetWorldScale(GetInstance().m_Packet),
            () => GetInstance().IncrementWorldScale(GetInstance().m_Packet),
            () => GetInstance().SpawnPrefab(GetInstance().m_Packet),
            () => GetInstance().SpawnPrimitive(GetInstance().m_Packet),
            () => GetInstance().SendFloats(GetInstance().m_Packet),
            () => GetInstance().SendTexture2D(GetInstance().m_Packet),
            () => GetInstance().ResolveAnchorId(GetInstance().m_Packet),
            () => GetInstance().ResolvedCloudAnchor(GetInstance().m_Packet),
            () => GetInstance().AnchorIDUpdate(GetInstance().m_Packet),
            () => GetInstance().LobbyTextMessage(GetInstance().m_Packet),
            null,   // AndroidKeepConnectionAlive
            () => GetInstance().TagUpdate(GetInstance().m_Packet),
            () => GetInstance().SendARPlaneAsMesh(GetInstance().m_Packet)
        };

        /// <summary>
        /// The queue that will hold all of the functions to be triggered by AWS events
        /// </summary>
        private readonly Queue<Action> m_MainThreadQueue = new Queue<Action>();

        /// <summary>
        /// Flag indicating whether or not we have activated the KeepConnectionAlive() coroutine so it doesn't get activated more than once
        /// </summary>
        private bool m_StreamActive = false;

        /// <summary>
        /// Used to get the Singleton instance of the GameLiftManager class
        /// </summary>
        /// <returns>The singleton instance of this class</returns>
        public static GameLiftManager GetInstance()
        {
            if (m_Instance != null)
            {
                return m_Instance;
            }
            else
            {
#if (ASL_DEBUG)
                Debug.LogError("GameLift not initialized.");
#endif
            }
            return null;
        }

        /// <summary>
        /// The Awake function, called right after object creation. Used to communicate that this object should not be destroyed between scene loads
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);           
        }

        /// <summary>
        /// The start function called upon object creation but after Awake, used to initialize internal classes and setup the singleton for this class
        /// </summary>
        private void Start()
        {
            m_Instance = this;
            m_LobbyManager = new LobbyManager();
            m_LobbyManager.Start();
            m_SceneLoader = new SceneLoader();
            m_GameController = new GameController();
            m_GameController.Start();
#if UNITY_ANDROID
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //Prevents Android app from turning the screen off, thus closing the application
#endif
        }

        /// <summary>
        /// Update is called once per frame and in this case is used to ensure any function triggered by AWS is ran by the main Unity thread
        /// </summary>
        private void Update()
        {
            RunMainThreadQueueActions();
        }

        /// <summary>
        /// An AWS listener function that is called when a connection is opened
        /// </summary>
        /// <param name="sender">The entity that called this function</param>
        /// <param name="error">Any error associated with this function</param>
        private void OnOpenEvent(object sender, EventArgs error)
        {
#if (ASL_DEBUG)
            Debug.Log("[server-sent] OnOpenEvent");
#endif
        }

        /// <summary>
        /// An AWS listener function that is called when a connection is ended with GameLift
        /// </summary>
        /// <param name="sender">The entity that called this error</param>
        /// <param name="error">Any error associated with this connection termination</param>
        private void OnCloseEvent(object sender, EventArgs error)
        {
#if (ASL_DEBUG)
            Debug.Log("[server-sent] OnCloseEvent: " + error);
#endif
            DisconnectFromServer();
            if (m_LobbyManager != null)
            {
                QForMainThread(m_LobbyManager.Reset);
            }
        }

        /// <summary>
        /// An AWS listener function that is called when there is a connection error event
        /// </summary>
        /// <param name="sender">The entity that called this error</param>
        /// <param name="error">The error that was received</param>
        private void OnConnectionErrorEvent(object sender, Aws.GameLift.Realtime.Event.ErrorEventArgs error)
        {
#if (ASL_DEBUG)
            Debug.Log($"[client] Connection Error! : ");
            if (error != null)
            {
                Debug.Log("Exception: \n" + error.Exception);
            }
#endif
            QForMainThread(DisconnectFromServer);
        }

        /// <summary>
        /// An AWS listener function that gets called every time a packet is received.
        /// Directly invokes the OpCode function corresponding to the OpCode number, determined by the OpCode enum.
        /// </summary>
        /// <param name="sender">The packet sender</param>
        /// <param name="_packet">The packet that was received</param>
        private void OnDataReceived(object sender, DataReceivedEventArgs _packet)
        {
            #if (ASL_DEBUG)
            string data = System.Text.Encoding.Default.GetString(_packet.Data);
            //Debug.Log($"[server-sent] OnDataReceived - Sender: {_packet.Sender} OpCode: {_packet.OpCode} data: {data.ToString()}");
            #endif
            m_Packet = _packet; // Set packet
            if(_packet.OpCode > OpCodeFunctions.Length || _packet.OpCode < 0) // Check for invalid OpCode
            {
                Debug.LogError("Unassigned OpCode received: " + _packet.OpCode);
                return;
            }
            if(OpCodeFunctions[_packet.OpCode] != null)     // Check that a function exists for the OpCode
            {
                OpCodeFunctions[_packet.OpCode].Invoke();   // Valid OpCode, invoke corresponding function
            }
        }

        // OpCode Functions
        private void PlayerJoinedMatch(DataReceivedEventArgs _packet)
        {
#if UNITY_ANDROID
            QForMainThread(StartPacketStream);
#endif
            QForMainThread(m_LobbyManager.PlayerJoinedMatch, _packet);
        }

        private void AddPlayerToLobbyUI(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_LobbyManager.AddPlayerToMatch, _packet);
        }
        private void PlayerDisconnected(DataReceivedEventArgs _packet)
        {
            QForMainThread(RemovePlayerFromList, _packet);
        }

        private void AllPlayersReady(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_LobbyManager.LockSession);
            QForMainThread(DestroyLobbyManager);
            QForMainThread(m_SceneLoader.LoadScene, _packet);
        }

        private void PlayerDisconnectedBeforeMatchStart(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_LobbyManager.AllowReadyUp);
        }

        private void LaunchScene(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_SceneLoader.LaunchScene);
        }

        private void LoadScene(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_SceneLoader.LoadScene, _packet);
        }

        private void ServerSetId(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetObjectID, _packet);
        }

        private void Claim(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetObjectClaim, _packet);
        }

        private void RejectClaim(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.RejectClaim, _packet);
        }

        private void ReleaseClaimToPlayer(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.ReleaseClaimedObject, _packet);
        }

        private void ClaimFromPlayer(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.ObjectClaimReceived, _packet);
        }

        private void SetObjectColor(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetObjectColor, _packet);
        }

        private void DeleteObject(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.DeleteObject, _packet);
        }

        private void SetLocalPosition(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetLocalPosition, _packet);
        }

        private void IncrementLocalPosition(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.IncrementLocalPosition, _packet);
        }

        private void SetLocalRotation(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetLocalRotation, _packet);
        }

        private void IncrementLocalRotation(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.IncrementLocalRotation, _packet);
        }

        private void SetLocalScale(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetLocalScale, _packet);
        }

        private void IncrementLocalScale(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.IncrementLocalScale, _packet);
        }

        private void SetWorldPosition(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetWorldPosition, _packet);
        }

        private void IncrementWorldPosition(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.IncrementWorldPosition, _packet);
        }

        private void SetWorldRotation(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetWorldRotation, _packet);
        }

        private void IncrementWorldRotation(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.IncrementWorldRotation, _packet);
        }

        private void SetWorldScale(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetWorldScale, _packet);
        }

        private void IncrementWorldScale(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.IncrementWorldScale, _packet);
        }

        private void SpawnPrefab(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SpawnPrefab, _packet);
        }

        private void SpawnPrimitive(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SpawnPrimitive, _packet);
        }

        private void SendFloats(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SentFloats, _packet);
        }

        private void SendTexture2D(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.RecieveTexture2D, _packet);
        }

        private void ResolveAnchorId(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.ResolveAnchorId, _packet);
        }

        private void ResolvedCloudAnchor(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.AllClientsFinishedResolvingCloudAnchor, _packet);
        }

        private void AnchorIDUpdate(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetAnchorID, _packet);
        }

        private void LobbyTextMessage(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_LobbyManager.UpdateChatLog, _packet);
        }

        private void TagUpdate(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.SetObjectTag, _packet);
        }

        private void SendARPlaneAsMesh(DataReceivedEventArgs _packet)
        {
            QForMainThread(m_GameController.ReceiveARPlaneAsMesh, _packet);
        }

        /// <summary>
        /// Destroys the LobbyManager internal class to free up space after a game is started
        /// </summary>
        private void DestroyLobbyManager()
        {
            m_LobbyManager = null;
        }

        /// <summary>
        /// Adds a player to the player list for this session
        /// </summary>
        /// <param name="_peerId">The peerId of the player to be added. Is a unique number that originally started at 1.</param>
        /// <param name="_username">The username of the player to be added</param>
        private void AddPlayerToList(int _peerId, string _username)
        {
            m_Players.Add(_peerId, _username);
        }

        /// <summary>
        /// Removes a player from the player list for this session. Happens when a player disconnects.
        /// It calls reassign function to reassign the physics master for the game if the leaving player was the master.
        /// </summary>
        /// <param name="_packet">The packet that was received from the server</param>
        private void RemovePlayerFromList(DataReceivedEventArgs _packet)
        {
            string data = Encoding.Default.GetString(_packet.Data);
            m_Players.Remove(int.Parse(data));
            m_LobbyManager?.UpdateLobbyScreen();
            if (m_PhysicsMasterId == int.Parse(data))
            {
                ASL_PhysicsMasterSingleton.Instance.ReassignPhysicsMaster();
            }
            ASL_AutonomousObjectHandler.Instance.ReasignObjects(int.Parse(data));
        }

        /// <summary>
        /// Assigns the given peer id to physics master's id.
        /// </summary>
        /// <param name="id">Peer id</param>
        public void SetPhysicsMasterId(int id)
        {
            m_PhysicsMasterId = id;
        }

        /// <summary>
        /// Creates an RealTime message to be sent to other users
        /// </summary>
        /// <param name="_opCode">The OpCode to be used to communicate what packet this is</param>
        /// <param name="_payload">The byte array containing the information to be transmitted in this message</param>
        /// <param name="_deliveryIntent">How this message should be sent. The default is reliable (TCP)</param>
        /// <param name="_targetGroup">The target group to send this message to. The default is the group for all users</param>
        /// <param name="_targetPlayer">The target player to send this message to. The default is the server where it is then intercepted and sent to all players reliably. </param>
        /// <returns></returns>
        public RTMessage CreateRTMessage(OpCode _opCode, byte[] _payload, DeliveryIntent _deliveryIntent = DeliveryIntent.Reliable, int _targetGroup = -1, int _targetPlayer = -1)
        {
            RTMessage message = m_Client.NewMessage((int)_opCode);
            message.WithPayload(_payload);
            message.WithDeliveryIntent(_deliveryIntent);
            if (_targetGroup == -1) { _targetGroup = m_GroupId; } //Default group is all
            message.WithTargetGroup(_targetGroup); //Default group is every user
            if (_targetPlayer == -1) { _targetPlayer = m_ServerId; } //Default player is server -> thus, don't specify player if not changed (if -1)
            message.WithTargetPlayer(_targetPlayer); //Default player is the server
            return message;
        }

        //Functions dealing with converting variable types to byte arrays and combining byte arrays
        #region Byte[] Conversions and Combinations

        // It should be noted that the order in which a packet is created matters.
        // The order should be as follows so that it can be decoded properly. Amount of data pieces (e.g., 2), the lengths in bytes of these data pieces (e.g., 4,4), 
        // and the data themselves converted to byte format (e.g., 4,4). The Combing functions found here automatically add this meta data for you.
        // If you follow how ASLObject functions already create these data packets then you will be following 
        // the correct formatting. The important thing to remember is that however you encode a packet, you must remember to decode in the same manner.

        /// <summary>
        /// Converts a Vector4 variable into a byte array
        /// </summary>
        /// <param name="_vector4">The Vector4 to convert</param>
        /// <returns>A byte array representing a Vector4 variable</returns>
        public byte[] ConvertVector4ToByteArray(Vector4 _vector4)
        {
            float[] vectorInFloatFormat = new float[4] { _vector4.x, _vector4.y, _vector4.z, _vector4.w };
            byte[] bytes = new byte[vectorInFloatFormat.Length * sizeof(float)];

            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.x), 0, bytes, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.y), 0, bytes, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.z), 0, bytes, 2 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.w), 0, bytes, 3 * sizeof(float), sizeof(float));

            return bytes;
        }

        /// <summary>
        /// Converts an int into a byte array
        /// </summary>
        /// <param name="_int">The int to be converted into a byte array</param>
        /// <returns>A byte array representing an int</returns>
        public byte[] ConvertIntToByteArray(int _int)
        {
            byte[] bytes = BitConverter.GetBytes(_int);
            return bytes;
        }

        /// <summary>
        /// Converts a bool into a byte array
        /// </summary>
        /// <param name="_bool">The bool to convert into a byte array</param>
        /// <returns>A byte array representing a boolean value</returns>
        public byte[] ConvertBoolToByteArray(bool _bool)
        {
            return BitConverter.GetBytes(_bool);
        }

        /// <summary>
        /// Converts a vector3 variable into a byte array
        /// </summary>
        /// <param name="_vector3">The vector 3 to convert</param>
        /// <returns>A byte array representing a vector3</returns>
        public byte[] ConvertVector3ToByteArray(Vector3 _vector3)
        {
            float[] vectorInFloatFormat = new float[3] { _vector3.x, _vector3.y, _vector3.z };
            byte[] bytes = new byte[vectorInFloatFormat.Length * sizeof(float)];

            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.x), 0, bytes, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.y), 0, bytes, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.z), 0, bytes, 2 * sizeof(float), sizeof(float));

            return bytes;
        }

        /// <summary>
        /// Converts a vector3 array into a float array
        /// </summary>
        /// <param name="_vectors">The vector3 array to convert into a float array</param>
        /// <returns>A float array representing the vector3 array passed in</returns>
        public float[] ConvertVector3ArrayToFloatArray(Vector3[] _vectors)
        {
            // Multiply the vector length by 3 since the float array will contain the x, y, and z positions of every point
            float[] vectorsInFloatFormat = new float[(_vectors.Length * 3)];
            for (int i = 0; i < _vectors.Length; i++)
            {
                // Separate a single vector3 into 3 floats for the x, y, and z positions
                vectorsInFloatFormat[(i * 3)] = _vectors[i].x;
                vectorsInFloatFormat[(i * 3) + 1] = _vectors[i].y;
                vectorsInFloatFormat[(i * 3) + 2] = _vectors[i].z;
            }
            return vectorsInFloatFormat;
        }

        /// <summary>
        /// Converts a float array into a Vector3 array
        /// </summary>
        /// <param name="_floatVectors">The float array to convert into a Vector3 array</param>
        /// <returns>A Vector3 array representing the float array passed in</returns>
        public static Vector3[] ConvertFloatArrayToVector3Array(float[] _floatVectors)
        {
            Vector3[] vectorArray = new Vector3[_floatVectors.Length / 3];
            for (int i = 0; i < _floatVectors.Length; i += 3)
            {
                int index = i / 3;
                vectorArray[index].x = _floatVectors[i];
                vectorArray[index].y = _floatVectors[i + 1];
                vectorArray[index].z = _floatVectors[i + 2];
                
            }
            return vectorArray;
        }

        /// <summary>
        /// Converts a Color array into a float array
        /// </summary>
        /// <param name="_colors">The color array to convert into a float array</param>
        /// <returns>A float array representing the color array passed in</returns>
        public static float[] ConvertColorArrayToFloatArray(Color[] _colors)
        {
            // Multiply the vector length by 3 since the float array will contain the x, y, and z positions of every point
            float[] colorsInFloatFormat = new float[(_colors.Length * 4)];
            for (int i = 0; i < _colors.Length; i++)
            {
                // Separate a single vector3 into 3 floats for the x, y, and z positions
                colorsInFloatFormat[(i * 4)] = _colors[i].r;
                colorsInFloatFormat[(i * 4) + 1] = _colors[i].g;
                colorsInFloatFormat[(i * 4) + 2] = _colors[i].b;
                colorsInFloatFormat[(i * 4) + 3] = _colors[i].a;
            }
            return colorsInFloatFormat;
        }

        /// <summary>
        /// Converts a float array into a Color array
        /// </summary>
        /// <param name="_floatColors">The float array to convert into a Color array</param>
        /// <returns>A Color array representing the float array passed in</returns>
        public static Color[] ConvertFloatArrayToColorArray(float[] _floatColors)
        {            
            Color[] colorArray = new Color[_floatColors.Length / 4];
            for (int i = 0; i < _floatColors.Length; i += 4)
            {
                int index = i / 4;
                colorArray[index].r = _floatColors[i];
                colorArray[index].g = _floatColors[i + 1];
                colorArray[index].b = _floatColors[i + 2];
                colorArray[index].a = _floatColors[i + 3];
            }
            return colorArray;
        }

        /// <summary>
        /// Converts a float array into a byte array
        /// </summary>
        /// <param name="_floats">The float array to convert</param>
        /// <returns>A byte array representing the floats passed in</returns>
        public byte[] ConvertFloatArrayToByteArray(float[] _floats)
        {
            byte[] bytes = new byte[_floats.Length * sizeof(float)];
            Buffer.BlockCopy(_floats, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Splits up a single byte array, returning only the section requested
        /// </summary>
        /// <param name="_byteArray">The byte array to be sliced</param>
        /// <param name="_startLocation">The location on the byte array in which to start from</param>
        /// <param name="_length">How long of a slice you want from the byte array</param>
        /// <returns>A section of the original byte array</returns>
        public byte[] SpiltByteArray(byte[] _byteArray, int _startLocation, int _length)
        {
            byte[] newByteArray = new byte[_length];
            Buffer.BlockCopy(_byteArray, _startLocation, newByteArray, 0, _length);
            return newByteArray;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <returns>A single byte array containing just the original byte arrays</returns>
        public byte[] CombineByteArrayWithoutLengths(byte[] _first, byte[] _second)
        {

            byte[] combinedResults = new byte[_first.Length + _second.Length];

            Buffer.BlockCopy(_first, 0, combinedResults, 0, _first.Length);
            Buffer.BlockCopy(_second, 0, combinedResults, _first.Length, _second.Length);

            return combinedResults;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users. Can be called with 2 or more byte[] arguments.
        /// Note that this method supports a variable number of arguments.
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <param name="arrays">Additional byte arrays to combine</param>
        /// <returns>A single byte array containing the original byte arrays and length information about them so that they can be properly decoded when received by users</returns>
        public byte[] CombineByteArrays(byte[] _first, byte[] _second, params byte[][] arrays)
        {
            byte[] count = BitConverter.GetBytes(2 + arrays.Length);
            byte[] firstLength = BitConverter.GetBytes(_first.Length);
            byte[] secondLength = BitConverter.GetBytes(_second.Length);

            // Compute size in bytes of combinedResults
            int totalLength = count.Length + _first.Length + _second.Length + firstLength.Length * (2 + arrays.Length);
            foreach (byte[] i in arrays)
            {
                totalLength += i.Length;
            }

            byte[] combinedResults = new byte[totalLength];

            Buffer.BlockCopy(count, 0, combinedResults, 0, count.Length);

            // Copy byte[] lengths into combinedResults
            Buffer.BlockCopy(firstLength, 0, combinedResults, count.Length, firstLength.Length);
            Buffer.BlockCopy(secondLength, 0, combinedResults, count.Length + firstLength.Length, secondLength.Length);
            int writeHead = count.Length + firstLength.Length + secondLength.Length;
            for (int i = 0; i < arrays.Length; ++i)
            {
                byte[] length = BitConverter.GetBytes(arrays[i].Length);
                Buffer.BlockCopy(length, 0, combinedResults, writeHead, length.Length);
                writeHead += length.Length;
            }

            // Copy byte[] contents into combinedResults
            Buffer.BlockCopy(_first, 0, combinedResults, writeHead, _first.Length);
            writeHead += _first.Length;
            Buffer.BlockCopy(_second, 0, combinedResults, writeHead, _second.Length);
            writeHead += _second.Length;
            for (int i = 0; i < arrays.Length; ++i)
            {
                Buffer.BlockCopy(arrays[i], 0, combinedResults, writeHead, arrays[i].Length);
                writeHead += arrays[i].Length;
            }

            return combinedResults;
        }
        #endregion

        //As AWS runs on a different thread, but Unity is single threaded, this is how we ensure that any information on the AWS thread is communicated to Unity thread
        #region QForMainThreadSection

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains no parameters
        /// </summary>
        /// <param name="fn">The name of the function to be called</param>
        private void QForMainThread(Action fn)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 1 parameter
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        private void QForMainThread<T1>(Action<T1> fn, T1 p1)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 2 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        private void QForMainThread<T1, T2>(Action<T1, T2> fn, T1 p1, T2 p2)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 3 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3>(Action<T1, T2, T3> fn, T1 p1, T2 p2, T3 p3)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 4 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <typeparam name="T4">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        /// <param name="p4">The fourth parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3, T4>(Action<T1, T2, T3, T4> fn, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3, p4); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 5 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <typeparam name="T4">The type of function to be called</typeparam>
        /// <typeparam name="T5">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        /// <param name="p4">The fourth parameter of the function to be called</param>
        /// <param name="p5">The fifth parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> fn, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3, p4, p5); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 6 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <typeparam name="T4">The type of function to be called</typeparam>
        /// <typeparam name="T5">The type of function to be called</typeparam>
        /// <typeparam name="T6">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        /// <param name="p4">The fourth parameter of the function to be called</param>
        /// <param name="p5">The fifth parameter of the function to be called</param>
        /// <param name="p6">The sixth parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> fn, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3, p4, p5, p6); });
            }
        }

        /// <summary>
        /// Locks and executes any functions that have been added to the QForMainThread queue. Is continuously called from this class's Update function
        /// </summary>
        private void RunMainThreadQueueActions()
        {
            // as our server messages come in on their own thread
            // we need to queue them up and run them on the main thread
            // when the methods need to interact with Unity
            lock (m_MainThreadQueue)
            {
                while (m_MainThreadQueue.Count > 0)
                {
                    m_MainThreadQueue.Dequeue().Invoke();
                }
            }
        }

        #endregion

        /// <summary>
        /// Disconnects the user from the GameLift servers if they are connected
        /// </summary>
        public void DisconnectFromServer()
        {
            if (m_Client != null && m_Client.Connected)
            {
                m_Client.Disconnect();
            }
        }

        /// <summary>
        /// Called when an application quits, ensuring the user cleanly exits the GameLift server
        /// </summary>
        private void OnApplicationQuit()
        {
            DisconnectFromServer();            
        }

        /// <summary>
        /// Used to make sure Android devices can disconnect from the GameLift servers. This function will quit the application when it loses focus
        /// e.g., when you exit the app to the home screen. Doing so is a good thing and prevents hanging connections.
        /// </summary>
        /// <param name="_isPaused">flag representing if the app is paused or not</param>
        private void OnApplicationPause(bool _isPaused)
        {
#if UNITY_ANDROID || UNITY_WSA
            if (_isPaused) //If we exit the app but don't force quit - e.g., go to the home screen
            {
                Application.Quit(); //Then quit the application, which calls our disconnect function
            }
#endif
        }

        /// <summary>
        /// Starts the coroutine that will send an empty packet to the relay server every second to help maintain the Android connection
        /// </summary>
        private void StartPacketStream()
        {
            if (!m_StreamActive)
            {
                m_StreamActive = true;
                StartCoroutine(KeepConnectionAlive());
            }
        }

        /// <summary>
        /// Sends an empty packet to the server to help maintain the android socket
        /// </summary>
        /// <returns>Waits for 1 second before sending another packet</returns>
        private IEnumerator KeepConnectionAlive()
        {
            while (m_Client.ConnectedAndReady)
            {
                yield return new WaitForSeconds(1);
                RTMessage message = CreateRTMessage(OpCode.AndroidKeepConnectionAlive, null, DeliveryIntent.Fast); 
                m_Client.SendMessage(message);
            }
            while (!m_Client.ConnectedAndReady)
            {
                yield return new WaitForSeconds(1);
            }
            StartCoroutine(KeepConnectionAlive());
        }

        /// <summary>Returns the current lowest peerID value out of all the currently connected players</summary>
        /// <returns>The lowest peer id of all the users in this match</returns>
        public int GetLowestPeerId()
        {
            int lowestPeerId = int.MaxValue;
            foreach (KeyValuePair<int, string> _aPlayer in m_Players)
            {
                if (lowestPeerId > _aPlayer.Key)
                {
                    lowestPeerId = _aPlayer.Key;
                }
            }
            return lowestPeerId;
        }

        /// <summary>
        /// Returns true if the caller is the lowest peer id user in the match. This is a good way to assign a "Host" player if desired.
        /// Though do keep in mind that ASL is a P2P network.
        /// </summary>
        /// <returns>True if caller has the lowest peer id</returns>
        public bool AmLowestPeer()
        {
            int currentLowest = GetLowestPeerId();
            if (currentLowest == m_PeerId)
            {
                return true;
            }
            return false;
        }



        /// <summary>Returns the current highest peerID value out of all the currently connected players</summary>
        /// <returns>The highest peer id of all the users in this match</returns>
        public int GetHighestPeerId()
        {
            int highestPeerId = int.MinValue;
            foreach (KeyValuePair<int, string> _aPlayer in m_Players)
            {
                if (highestPeerId < _aPlayer.Key)
                {
                    highestPeerId = _aPlayer.Key;
                }
            }
            return highestPeerId;
        }

        /// <summary>
        /// Returns true if the caller is the highest peer id user in the match. This is a good way to assign a "Host" player if desired.
        /// Though do keep in mind that ASL is a P2P network.
        /// </summary>
        /// <returns>True if caller has the highest peer id</returns>
        public bool AmHighestPeer()
        {
            int currentHighest = GetHighestPeerId();
            if (currentHighest == m_PeerId)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates a unique callback id for a callback function.
        /// </summary>
        /// <returns>A unique callback id in string</returns>
        public string GenerateOpFunctionCallbackKey()
        {
            Guid guid = Guid.NewGuid();
            string guidInString = guid.ToString();
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string callbackId = guidInString + "_" + timeStamp;
            return callbackId;
        }

        /// <summary>
        /// Adds the given callback function with the given OpCode as the key into the dictionary
        /// </summary>
        /// <param name="callback">pre-defined callback function</param>
        /// <param name="key">callback id</param>
        public void SetOpFunctionCallback(OpFunctionCallback callback, string key)
        {
            if (OpFunctionCallbacks.ContainsKey(key)) return;
            OpFunctionCallbacks.Add(key, callback);
        }

        /// <summary>
        /// Gets the corresponding callback function with the given OpCode and callback id from the dictionary.
        /// Removes the callback function after it has been invoked.
        /// </summary>
        /// <param name="callbackId">The OpCode function's callback id</param>
        /// <param name="obj">The game object processed</param>
        public void DoOpFunctionCallback(string callbackId, GameObject obj)
        {
            //get callback function base on key, if key = "0", no callback assigned
            if (callbackId.Equals(m_NullCallbackIdInString)) return;
            if (OpFunctionCallbacks.ContainsKey(callbackId))
            {
                OpFunctionCallback callback = OpFunctionCallbacks[callbackId];
                OpFunctionCallbacks.Remove(callbackId);
                callback.Invoke(obj);
               
            }
            return;
        }

        /// <summary>
        /// Removes the corresponding callback function by the given callback id from the dictionary.
        /// </summary>
        /// <param name="callbackId">The OpCode function's callback id</param>
        public void RemoveOpFunctionCallbackByCallbackId(string callbackId)
        {
            if (OpFunctionCallbacks.ContainsKey(callbackId))
            {
                OpFunctionCallbacks.Remove(callbackId);
            }
        }

        /// <summary>
        /// Generate callback id with given information.
        /// save the callback with generated id as the key into dictionary
        /// </summary>
        /// <param name="callback">user pre-defined callback function</param>
        /// <returns>A unique callback id in byte array</returns>
        public byte[] SetOpFunctionCallback(OpFunctionCallback callback)
        {
            if (callback == null) return m_NullCallbackId;
            string callbackId = SetOpFunctionCallbackString(callback);
            return Encoding.ASCII.GetBytes(callbackId);
        }

        /// <summary>
        /// Generate callback id with given information.
        /// save the callback with generated id as the key into dictionary
        /// </summary>
        /// <param name="callback">user pre-defined callback function</param>
        /// <returns>A unique callback id in string</returns>
        public string SetOpFunctionCallbackString(OpFunctionCallback callback)
        {
            if (callback == null) return m_NullCallbackIdInString;
            string callbackId = GetInstance().GenerateOpFunctionCallbackKey();
            GetInstance().SetOpFunctionCallback(callback, callbackId);
            return callbackId;
        }

        /// <summary>
        /// Adds the given callback function without parameter with the given OpCode as the key into the dictionary
        /// </summary>
        /// <param name="callback">pre-defined callback function without parameter</param>
        /// <param name="key">callback id</param>
        public void SetOpFunctionCallback(OpFunctionCallbackNoParam callback, string key)
        {
            if (OpFunctionCallbacksNoParam.ContainsKey(key)) return;
            OpFunctionCallbacksNoParam.Add(key, callback);
        }

        /// <summary>
        /// Gets the corresponding callback without parameter function with the given OpCode and callback id from the dictionary.
        /// Removes the callback function after it has been invoked.
        /// </summary>
        /// <param name="callbackId">The OpCode function's callback id</param>
        public void DoOpFunctionCallback(string callbackId)
        {
            //get callback function base on key, if key = "0", no callback assigned
            if (callbackId.Equals(m_NullCallbackIdInString)) return;
            if (OpFunctionCallbacksNoParam.ContainsKey(callbackId))
            {
                OpFunctionCallbackNoParam callback = OpFunctionCallbacksNoParam[callbackId];
                OpFunctionCallbacksNoParam.Remove(callbackId);
                callback.Invoke();

            }
            return;
        }

        /// <summary>
        /// Removes the corresponding callback function without parameter by the given callback id from the dictionary.
        /// </summary>
        /// <param name="callbackId">The OpCode function's callback id</param>
        public void RemoveOpFunctionCallbackNoParamByCallbackId(string callbackId)
        {
            if (OpFunctionCallbacksNoParam.ContainsKey(callbackId))
            {
                OpFunctionCallbacksNoParam.Remove(callbackId);
            }
        }

        /// <summary>
        /// Generate callback id with given information.
        /// save the callback with generated id as the key into dictionary
        /// </summary>
        /// <param name="callback">user pre-defined callback function without parameter</param>
        /// <returns>A unique callback id in byte array</returns>
        public byte[] SetOpFunctionCallback(OpFunctionCallbackNoParam callback)
        {
            if (callback == null) return m_NullCallbackId;
            string callbackId = SetOpFunctionCallbackString(callback);
            return Encoding.ASCII.GetBytes(callbackId);
        }

        /// <summary>
        /// Generate callback id with given information.
        /// save the callback with generated id as the key into dictionary
        /// </summary>
        /// <param name="callback">user pre-defined callback function without parameter</param>
        /// <returns>A unique callback id in string</returns>
        public string SetOpFunctionCallbackString(OpFunctionCallbackNoParam callback)
        {
            if (callback == null) return m_NullCallbackIdInString;
            string callbackId = GetInstance().GenerateOpFunctionCallbackKey();
            GetInstance().SetOpFunctionCallback(callback, callbackId);
            return callbackId;
        }


    }
}
