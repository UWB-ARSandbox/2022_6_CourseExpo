// Example Realtime Server Script
'use strict';

// Example override configuration
const configuration = {
    pingIntervalTime: 30000
};

// Timing mechanism used to trigger end of game session. Defines how long, in milliseconds, between each tick in the example tick loop
const tickTime = 1000;

// Defines how to long to wait in Seconds before beginning early termination check in the example tick loop
const minimumElapsedTime = 60;

var session;                        // The Realtime server session object
var logger;                         // Log at appropriate level via .info(), .warn(), .error(), .debug()
var startTime;                      // Records the time the process started
var activePlayers = 0;              // Records the number of connected players
var onProcessStartedCalled = false; // Record if onProcessStarted has been called

// Example custom op codes for user-defined messages
// Any positive op code number can be defined here. These should match your client code.
var OpCode = Object.freeze(
{
  "PlayerJoinedMatch":1,
  "AddPlayerToLobbyUI":2,
	"PlayerDisconnected":3,
	"AllPlayersReady": 4,
	"PlayerDisconnectedBeforeMatchStart": 5,
	"PlayerReady":6,
	"LaunchScene":7,
	"LoadScene":8,
	"ServerSetId":9,
	"ReleaseClaim":10,
	"ClaimObject":11,
	"RejectClaim":12,
	"ClaimObjectWithResponse":13,
	"ClaimObjectResponse":14,
	"SetObjectColor":15,
	"DeleteObject":16,
	"SetLocalPosition":17,
	"IncrementLocalPosition":18,
	"SetLocalRotation":19,
	"IncrementLocalRotation":20,
	"SetLocalScale":21,
	"IncrementLocalScale":22,
	"SetWorldPosition":23,
	"IncrementWorldPosition":24,
	"SetWorldRotation":25,
	"IncrementWorldRotation":26,
	"SetWorldScale":27,
	"IncrementWorldScale":28,
	"SpawnPrefab":29,
	"SpawnPrimitive":30,
	"SendFloats":31,
	"SendTexture2D":32,
	"ResolveAnchorId":33,
	"ResolvedCloudAnchor":34,
	"AnchorIDUpdate":35,
	"LobbyTextMessage":36,
	"AndroidConnectionStream":37, //Used to help keep the socket connected for android
	"TagUpdate":38,
	"SendARPlaneAsMesh":39
});

// Array of function references corresponding to OpCodes. null entries in the array are OpCodes that currently have no
// action within this script when received, but may have other action elsewhere, or are automatically sent.
var OpCodeFunctions = [
	null,	// No function - OpCodes start at 1
	null, 	// PlayerJoinedMatch
    AddPlayerToLobbyUI,
	null,	// PlayerDisconnected
	null, 	// AllPlayersReady
	null, 	// PlayerDisconnectedBeforeMatchStart
	PlayerReady,
	LaunchScene,
	LoadScene,
	ServerSetId,
	ReleaseClaim,
	ClaimObject,
	null, 	// RejectClaim
	null, 	// ClaimObjectWithResponse
	ClaimObjectResponse,
	SetObjectColor,
	DeleteObject,
	SetLocalPosition,
	IncrementLocalPosition,
	SetLocalRotation,
	IncrementLocalRotation,
	SetLocalScale,
	IncrementLocalScale,
	SetWorldPosition,
	IncrementWorldPosition,
	SetWorldRotation,
	IncrementWorldRotation,
	SetWorldScale,
	IncrementWorldScale,
	SpawnPrefab,
	SpawnPrimitive,
	SendFloats,
	SendTexture2D,
	ResolveAnchorId,
	ResolvedCloudAnchor,
	AnchorIDUpdate,
	LobbyTextMessage,
	null, 	// AndroidConnectionStream
	TagUpdate,
	SendARPlaneAsMesh
]

function Player(peerId, readyState, ownedASLObjects, requestedNewObjectId)
{
	this.peerId = peerId;
	this.readyState = readyState;
	this.ownedASLObjects = ownedASLObjects;
	this.requestedNewObjectId = requestedNewObjectId;
}

//if objectClaimer is 0, then no one is currently trying to claim this object
function ASLObject(id, owner, objectClaimer)
{
	this.id = id;
	this.owner = owner;
	this.objectClaimer = objectClaimer;
	this.resolvedCloudAnchorCount = null;
}

var Players = [];
var ASLObjects = {};
var ASLObjectSyncHolder = [];
const ID_LENGTH = 36;
const CALLBACK_ID_LENGTH = 55; // CallbackId always at index 0
const CALLBACK_ID_START_LOCATION_WHEN_DELETE_OBJECT = 3*4; //3 numbers appear before the [CallbackId + ID] appears in packet, 4 is the size of an int
const ID_START_LOCATION_WHEN_CREATED_BY_USER = 14*4; //14 numbers appear before the ID appears in packet, 4 is the size of an int
const ID_START_LOCATION_WHEN_CREATED_FOR_CLOUD_ANCHOR = 6*4 //6 numbers appear before the [CallbackId + ID] appears in the packet, 4 is the size of an int
var MatchStarted = false;
var InitialScene = "";
var ASLObjectsSynchronizedAtSceneLoad = 0;


// Called when game server is initialized, passed server's object of current session
function init(rtSession) {
    session = rtSession;
    logger = session.getLogger();
}

// On Process Started is called when the process has begun and we need to perform any
// bootstrapping.  This is where the developer should insert any code to prepare
// the process to be able to host a game session, for example load some settings or set state
//
// Return true if the process has been appropriately prepared and it is okay to invoke the
// GameLift ProcessReady() call.
function onProcessStarted(args) {
    onProcessStartedCalled = true;
    logger.info("Starting process with args: " + args);
    logger.info("Ready to host games...");

    return true;
}

// Called when a new game session is started on the process
function onStartGameSession(gameSession) {
    // Complete any game session set-up

    // Set up an example tick loop to perform server initiated actions
    startTime = getTimeInS();
    tickLoop();
}

// Handle process termination if the process is being terminated by GameLift
// You do not need to call ProcessEnding here
function onProcessTerminate() {
    // Perform any clean up
}

// Return true if the process is healthy
function onHealthCheck() {
    return true;
}

// On Player Connect is called when a player has passed initial validation
// Return true if player should connect, false to reject
function onPlayerConnect(connectMsg) {
    // Perform any validation needed for connectMsg.payload, connectMsg.peerId
    return true;
}

// Called when a Player is accepted into the game
function onPlayerAccepted(player) {
    // This player was accepted -- let's send them a message

	const message = session.newTextGameMessage(OpCode.PlayerJoinedMatch, session.getServerId(),
						session.getServerId() + ":" + session.getAllPlayersGroupId() + ":" + player.peerId); //Get server Id and group Id for all users
	session.sendReliableMessage(message, player.peerId);
    activePlayers++;

	Players.push(new Player(player.peerId, false, [], false));
}

// On Player Disconnect is called when a player has left or been forcibly terminated
// Is only called for players that actually connected to the server and not those rejected by validation
// This is called before the player is removed from the player list
function onPlayerDisconnect(peerId) {

	//Set owned objects to server before removing player
	ReturnUsersASLObjectsToServer(peerId);
	//Remove player from Players array
	RemovePlayer('peerId', peerId);

    // send a message to each remaining player letting them know about the disconnect
   session.getPlayers().forEach((player, playerId) =>
   {
		const outMessage = session.newTextGameMessage(OpCode.PlayerDisconnected, session.getServerId(), "" + peerId); //Who disconnected
		const unReadyMessage = session.newTextGameMessage(OpCode.PlayerDisconnectedBeforeMatchStart, session.getServerId(), ""); //If ready, unready
	    if (playerId != peerId)
	    {
		    session.sendReliableMessage(outMessage, playerId);
		    if (!MatchStarted)
			{
				var user = FindPlayer('peerId', playerId);
				if (user != null) { user.readyState = false; }
				session.sendReliableMessage(unReadyMessage, playerId);
			}
	    }
    });
	activePlayers--;

}

// Handle a message to the server
// Directly executes the OpCode function corresponding to the OpCode number as determined by the OpCode enum
function onMessage(gameMessage)
{
	if(gameMessage.opCode > OpCodeFunctions.length || gameMessage.opCode < 0)	//Check for valid OpCode
	{
		logger.error("Unassigned OpCode received: " + (gameMessage.opCode));
		return;
	}
	if(OpCodeFunctions[gameMessage.opCode] != null)		//Check that a function exists for the OpCode
	{
		OpCodeFunctions[gameMessage.opCode](gameMessage);	//Valid OpCode, execute corresponding function
	}
}

// OpCode Functions
function AddPlayerToLobbyUI(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.AddPlayerToLobbyUI, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function PlayerReady(gameMessage)
{
	var player = FindPlayer('peerId', gameMessage.sender);
	if (player != null) { player.readyState = true; }
	if (gameMessage.payload != "") { InitialScene = gameMessage.payload; }
	//If all players are ready (can't find a player with a readyState of false)
	if (FindPlayer('readyState', false) == null)
	{
		MatchStarted = true;
		SetReadyStateToFalse();
		const outMessage = session.newTextGameMessage(OpCode.AllPlayersReady, session.getServerId(), InitialScene);
		session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
	}
}

function LaunchScene(gameMessage)
{
	var player = FindPlayer('peerId', gameMessage.sender);
	if (player != null) { player.readyState = true; }
	//If all players are ready (can't find a player with a readyState of false)
	if (FindPlayer('readyState', false) == null)
	{
		SetReadyStateToFalse();
		const outMessage = session.newTextGameMessage(OpCode.LaunchScene, session.getServerId(), "");
		session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());

		//ASLObjects.length = 0; //new scene, new ASLObjects. This helps keep the list small
		ASLObjectSyncHolder.length = 0;
		ASLObjectsSynchronizedAtSceneLoad = 0;
		SetRequestedNewObjectIdToFalse();
	}
}

function LoadScene(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.LoadScene, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function ServerSetId(gameMessage)
{
	//Only create guid from the "host"'s packet
	if (gameMessage.sender == GetLowestPeerId())
	{
		var id = uuidv4();
		ASLObjects[id] = new ASLObject(id, "server", "0");
		ASLObjectSyncHolder.push(ASLObjects[id]);
	}

	var player = FindPlayer('peerId', gameMessage.sender);
	if (player != null) { player.requestedNewObjectId = true; }

	//if all players have requested a new object id then all players have found all the ASL objects in the current scene
	if (FindPlayer('requestedNewObjectId', false) == null)
	{
		for (let i = ASLObjectsSynchronizedAtSceneLoad; i < ASLObjectSyncHolder.length; i++)
		{
			const outMessage = session.newTextGameMessage(OpCode.ServerSetId, session.getServerId(), ASLObjectSyncHolder[i].id);
			session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
			ASLObjectsSynchronizedAtSceneLoad++;
		}
	}
}

function ReleaseClaim(gameMessage)
{
	var id = UnpackPayload(gameMessage.payload);
	ASLObjects[id].owner = "server";
}

function ClaimObject(gameMessage)
{
	var decodedMessage = UnpackPayload(gameMessage.payload);

  const data = decodedMessage.split(":");
  var callbackId = data[0];
  var id = data[1];
	//if object trying to be claimed has no id - reject
	if (id == "")
	{
    var newPayload = callbackId + ":" + id;
		const outMessage = session.newTextGameMessage(OpCode.RejectClaim, session.getServerId(), newPayload);
		session.sendReliableMessage(outMessage, gameMessage.sender);
	}

	//if no one is currently trying to claim this object
	if (ASLObjects[id].objectClaimer == "0")
	{
		ASLObjects[id].objectClaimer = gameMessage.sender + ""; //update claimer

		if (ASLObjects[id].owner == "server" || ASLObjects[id].owner == gameMessage.sender + "") //if server or current claimer currently owns this object
		{

			//Update everyone of ownership
			ASLObjects[id].owner = gameMessage.sender + "";
			ASLObjects[id].objectClaimer = "0";
			var newPayload = callbackId + ":" + id + ":" + gameMessage.sender + "";

			const outMessage = session.newTextGameMessage(OpCode.ClaimObject, session.getServerId(), newPayload);
			session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());

		}
		else //we wait for previous owner to inform us they gave up the object so we can successfully claim it
		{

			var newPayload = callbackId + ":" + id + ":" + ASLObjects[id].owner + ":" + gameMessage.sender;
			const outMessage = session.newTextGameMessage(OpCode.ClaimObjectWithResponse, session.getServerId(), newPayload);
			session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
		}
	}
	else //There is an outstanding claim -> reject new claimer
	{
    var newPayload = callbackId + ":" + id;
		const outMessage = session.newTextGameMessage(OpCode.RejectClaim, session.getServerId(), newPayload);
		session.sendReliableMessage(outMessage, gameMessage.sender);
	}
}

function ClaimObjectResponse(gameMessage)
{
	var decodedMessage = UnpackPayload(gameMessage.payload);
	//Get ID and new owner from payload
	var id = "";
	var newOwner = ""

  const data = decodedMessage.split(":");
  var callbackId = data[0];
  var id = data[1];
  var newOwner = data[2];

	ASLObjects[id].owner = newOwner;
	ASLObjects[id].objectClaimer = "0";
	//Pass on to new owner that they own this object
	const outMessage = session.newTextGameMessage(OpCode.ClaimObjectResponse, session.getServerId(), gameMessage.payload);
	session.sendReliableMessage(outMessage, Number(newOwner));
}

function SetObjectColor(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SetObjectColor, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function DeleteObject(gameMessage)
{
	//var id = UnpackPayload(gameMessage.payload);
  var decodedMessage = UnpackPayload(gameMessage.payload);
  var id = "";

  for (let i = CALLBACK_ID_LENGTH + CALLBACK_ID_START_LOCATION_WHEN_DELETE_OBJECT; i < ID_LENGTH + CALLBACK_ID_LENGTH + CALLBACK_ID_START_LOCATION_WHEN_DELETE_OBJECT; i++)
  {
    id += decodedMessage.charAt(i);
  }

	delete ASLObjects[id].id;
	delete ASLObjects[id].owner;
	delete ASLObjects[id].objectClaimer;

  // or switch to test to see id value
	const outMessage = session.newTextGameMessage(OpCode.DeleteObject, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SetLocalPosition(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SetLocalPosition, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function IncrementLocalPosition(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.IncrementLocalPosition, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SetLocalRotation(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SetLocalRotation, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function IncrementLocalRotation(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.IncrementLocalRotation, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SetLocalScale(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SetLocalScale, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function IncrementLocalScale(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.IncrementLocalScale, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SetWorldPosition(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SetWorldPosition, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function IncrementWorldPosition(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.IncrementWorldPosition, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SetWorldRotation(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SetWorldRotation, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function IncrementWorldRotation(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.IncrementWorldRotation, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SetWorldScale(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SetWorldScale, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function IncrementWorldScale(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.IncrementWorldScale, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SpawnPrefab(gameMessage)
{
	var decodedMessage = UnpackPayload(gameMessage.payload);
	//Get ID and new owner from payload
	var id = "";
	//split on ':' to find id and newOwner
	for (let i = ID_START_LOCATION_WHEN_CREATED_BY_USER; i < ID_LENGTH + ID_START_LOCATION_WHEN_CREATED_BY_USER; i++)
	{
		id += decodedMessage.charAt(i);
	}

	ASLObjects[id] = new ASLObject(id, "server", "0");
	const outMessage = session.newTextGameMessage(OpCode.SpawnPrefab, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SpawnPrimitive(gameMessage)
{
	var decodedMessage = UnpackPayload(gameMessage.payload);
	//Get ID and new owner from payload
	var id = "";
	//split on ':' to find id and newOwner
	for (let i = ID_START_LOCATION_WHEN_CREATED_BY_USER; i < ID_LENGTH + ID_START_LOCATION_WHEN_CREATED_BY_USER; i++)
	{
		id += decodedMessage.charAt(i);
	}

	ASLObjects[id] = new ASLObject(id, "server", "0");
	const outMessage = session.newTextGameMessage(OpCode.SpawnPrimitive, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SendFloats(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SendFloats, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SendTexture2D(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SendTexture2D, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function ResolveAnchorId(gameMessage)
{
	//Look at id of object, if we don't have id on server side, then a new obkect was created for this anchor id and we need to add its id
	var decodedMessage = UnpackPayload(gameMessage.payload);
	//Get ID and new owner from payload
	var id = "";
	//split on ':' to find id and newOwner
	for (let i = ID_START_LOCATION_WHEN_CREATED_FOR_CLOUD_ANCHOR; i < ID_LENGTH + ID_START_LOCATION_WHEN_CREATED_FOR_CLOUD_ANCHOR; i++)
	{
		id += decodedMessage.charAt(i);
	}

	if (ASLObjects[id] == null)
	{
		ASLObjects[id] = new ASLObject(id, "server", "0");
	}


	const outMessage = session.newTextGameMessage(OpCode.ResolveAnchorId, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function ResolvedCloudAnchor(gameMessage)
{
	var decodedMessage = UnpackPayload(gameMessage.payload);
	//Get ID and new owner from payload
	var id = "";
	//split on ':' to find id and newOwner
	for (let i = 0; i < ID_LENGTH; i++)
	{
		id += decodedMessage.charAt(i);
	}

	if (ASLObjects[id].resolvedCloudAnchorCount == null)
	{
		ASLObjects[id].resolvedCloudAnchorCount = 0;
	}
	ASLObjects[id].resolvedCloudAnchorCount++;
	if (ASLObjects[id].resolvedCloudAnchorCount >= Players.length)
	{
		//Reset
		ASLObjects[id].resolvedCloudAnchorCount = 0;
		const outMessage = session.newTextGameMessage(OpCode.ResolvedCloudAnchor, session.getServerId(), gameMessage.payload);
		session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
	}
}

function AnchorIDUpdate(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.AnchorIDUpdate, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function LobbyTextMessage(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.LobbyTextMessage, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function TagUpdate(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.TagUpdate, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}

function SendARPlaneAsMesh(gameMessage)
{
	const outMessage = session.newTextGameMessage(OpCode.SendARPlaneAsMesh, session.getServerId(), gameMessage.payload);
	session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
}


// Return true if the send should be allowed
function onSendToPlayer(gameMessage)
{
    return true;
}

// Return true if the send to group should be allowed
// Use gameMessage.getPayloadAsText() to get the message contents
function onSendToGroup(gameMessage)
{
    return true;
}

// Return true if the player is allowed to join the group
function onPlayerJoinGroup(groupId, peerId)
{
    return true;
}

// Return true if the player is allowed to leave the group
function onPlayerLeaveGroup(groupId, peerId)
{
    return true;
}

// A simple tick loop example
// Checks to see if a minimum amount of time has passed before seeing if the game has ended
async function tickLoop() {
    const elapsedTime = getTimeInS() - startTime;
    //logger.info("Tick... " + elapsedTime + " activePlayers: " + activePlayers);

    // In Tick loop - see if all players have left early after a minimum period of time has passed
    // Call processEnding() to terminate the process and quit
    if ( (activePlayers == 0) && (elapsedTime > minimumElapsedTime))
	{
        logger.info("All players disconnected. Ending game");
        const outcome = await session.processEnding();
        logger.info("Completed process ending with: " + outcome);
        process.exit(0);
    }
    else {
        setTimeout(tickLoop, tickTime);
    }
}

// Calculates the current time in seconds
function getTimeInS() {
    return Math.round(new Date().getTime()/1000);
}


function FindPlayer(key, value)
{
	for (let i = 0; i < Players.length; i++)
	{
		if (Players[i][key] == value)
		{
			return Players[i];
		}
	}
	return null;
}

function RemovePlayer(key, value)
{
	for (let i = 0; i < Players.length; i++)
	{
		if (Players[i][key] == value)
		{
			Players.splice(i, 1);
		}
	}
	return null;
}

function ReturnUsersASLObjectsToServer(peerId)
{
	for (var key in ASLObjects)
	{
		if (ASLObjects[key].owner == peerId)
		{
			ASLObjects[key].owner = "server";
		}
		if (ASLObjects[key].objectClaimer != "0")
		{
			ASLObjects[key].owner = ASLObjects[key].objectClaimer;
			ASLObjects[key].objectClaimer = "0";
			const outMessage = session.newTextGameMessage(OpCode.ClaimObject, session.getServerId(), ASLObjects[key].id + ":" + ASLObjects[key].owner);
			session.sendReliableGroupMessage(outMessage, session.getAllPlayersGroupId());
		}
	}
}

function SetRequestedNewObjectIdToFalse()
{
	for (let i = 0; i < Players.length; i++)
	{
		Players[i].requestedNewObjectId = false;
	}
}

function SetReadyStateToFalse()
{
	for (let i = 0; i < Players.length; i++)
	{
		Players[i].readyState = false;
	}
}

function GetLowestPeerId()
{
	var lowest = 5000; //Random high number
	for (let i = 0; i < Players.length; i++)
	{
		if (Players[i].peerId < lowest)
		{
			lowest = Players[i].peerId;
		}
	}
	return lowest;

}

//Function taken from here: https://stackoverflow.com/questions/105034/create-guid-uuid-in-javascript#comment16687461_105074
//This may not produce an exactly random guid, but it should produce a unique enough one for our scenario.
function uuidv4()
{
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

function UnpackPayload(byteArray)
{
	return String.fromCharCode.apply(null, byteArray);
}

exports.ssExports = {
    configuration: configuration,
    init: init,
    onProcessStarted: onProcessStarted,
    onMessage: onMessage,
    onPlayerConnect: onPlayerConnect,
    onPlayerAccepted: onPlayerAccepted,
    onPlayerDisconnect: onPlayerDisconnect,
    onSendToPlayer: onSendToPlayer,
    onSendToGroup: onSendToGroup,
    onPlayerJoinGroup: onPlayerJoinGroup,
    onPlayerLeaveGroup: onPlayerLeaveGroup,
    onStartGameSession: onStartGameSession,
    onProcessTerminate: onProcessTerminate,
    onHealthCheck: onHealthCheck
};
