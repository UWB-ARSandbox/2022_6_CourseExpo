
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SimpleFileBrowser;
using UnityEngine.XR;




//This script handles all aspects of drawing on a canvas
//It operates by taking user inputs, sending the information over ASL, and using the inputs to build the drawings
//The script is organized into three parts.
//1. The public methods that allow interaction with the canvas
//2. The methods that send and recieeve user input
//3. The methods that build the drawing using the user input
//The canvas should be modular enough that it can just be used wherever
//Dependencies: ASLObject, 2 MeshRenderers (one for the canvas and one for the canvas's mask), CanvasInput, SimpleFileBrowser, and a bunch of UI components

//Things to do:
//1. Rename script and variables, a lot of them or confusing
//2. Move the building of the drawings to the GPU, to help improve performance
//3. Make a mask for line tool.
//4. Improve line tool in general, solution is not the best solution
//5. Consider change the brush shape from a square to a circle
//6. better way of doing text, where the text size increases at a better rate (currently it is just interpolated to make it bigger)

//Important notes:
//The canvas drawing get built on each client whether they can see it or not. Needs more testing to see full performance impact of people drawing at once
//


public class NewPaint : MonoBehaviour
{
	
	//Variable determines whether a player is allowed to draw on the canvas
	//Important note: The canvas can be not visible (black texture) but still be drawn on
	public bool allowedPlayer;

	//When enabled at start up this will make the canvas be usable for drawing for all players
	[SerializeField] bool allowForEveryone;

	//When enabled at start up this will make the texture on the canvas visible for all players
	[SerializeField] bool visibleForEveryone;

	//The IDs of the players allowed to use the canvas at startup
	[SerializeField] List<int> startingAllowedPlayers;

	//The IDs of the players allowed to see the canvas at startup
	[SerializeField] List<int> startingAllowedViewers;



	//Note: the enable/disable canvas methods allow for players to draw on the canvas, but do not make them visible. Same thing goes for the enable/disable viewing methods

	//Enables the canvas for the player locally
	public void enableCanvasLocal()
	{
		allowedPlayer = true;
		
		
	}
	//Disables the canvas for the player locally
	public void disableCanvasLocal()
	{
		allowedPlayer = false;
		
	}
	//Enables viewing of the canvas locally, does this by switching the texture of the canvas to the main texture
	public void enableViewingLocal()
	{
		gameObject.GetComponent<Renderer>().material.mainTexture = studentCanvas;

		//Event gets invoked so that canvas mirrors get updated with the knew texture
		canvasTextureSwitch.Invoke(studentCanvas);
	}
	//Enables viewing of the canvas locally, does this by switching the texture of the canvas to the blank texture
	public void disableViewingLocal()
	{
		gameObject.GetComponent<Renderer>().material.mainTexture = blankCanvas;

		//Event gets invoked so that canvas mirrors get updated with the knew texture
		canvasTextureSwitch.Invoke(blankCanvas);
	}

	//Enables a player to use the canvas based off their id, can be called from anyone, not just the local player
	public IEnumerator enableCanvasForPlayer(int peerID)
	{
		yield return new WaitForSeconds(1); //This is necessary for when this method gets called during Start, there may be better solution 
		float[] fArray = {1, peerID}; 
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
				
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
	}
	//Disables a player to use the canvas based off their id, can be called from anyone, not just the local player
	public IEnumerator disableCanvasForPlayer(int peerID)
	{
		yield return new WaitForSeconds(1); //This is necessary for when this method gets called during Start, there may be better solution
		float[] fArray = {2, peerID};
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
	}
	//Enables a player to view the canvas based off their id, can be called from anyone, not just the local player
	public IEnumerator enableViewingForPlayer(int peerID)
	{
		yield return new WaitForSeconds(1); //This is necessary for when this method gets called during Start, there may be better solution
		float[] fArray = {3, peerID};
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
	}
	//Disables a player to view the canvas based off their id, can be called from anyone, not just the local player
	public IEnumerator disableViewingForPlayer(int peerID)
	{
		yield return new WaitForSeconds(1); //This is necessary for when this method gets called during Start, there may be better solution
		float[] fArray = {4, peerID};
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
	}
	//Returns the current texture applied to the canvas
	public Texture2D getTexture()
	{
		if(allowedPlayer)
		{
			return studentCanvas;
		}
		else
		{
			return blankCanvas;
		}
	}

	//Event that signifies that the canvas has switched textures, so that the canvas mirrors can know to be updated as well
	public event Action<Texture2D> canvasTextureSwitch;

	
	//Struct containing all the input information that is needed to build whatever the player is drawing on the canvas

    struct InputInformation 
	{


		//When this is true, it means that the clear command gets called. Should not be within InputInformation
		public bool ClearCanvas;

		//When this is true, it means the load texture command gets called. Also should not be within InputInformation
		public bool loadTexture;

		//The texture that gets loaded when the loadTexture command is called. Also should not be within InputInformation
		public Texture2D textureToLoad;

		//Where on the canvas the player clicked.
		public Vector2 canvasClick;

		//Whether the mouse was held down.
		public bool mouseDown;

		//Whether the mouse was held down on the previous input
		public bool previousMouseDown;

		//Where on the canvas the player had clicked previously (used for interpolation)
		public Vector2 previousCanvasClick;
		
		//The color the player was using
		public Color brushColor;

		//The brush size the player was using
		public int brushSize;

		//Whether erase mode was on
		public bool eraseMode;

		//Whether text mode was on
		public bool textMode;

		//Whether line mode was on
		public bool lineMode;

		//The text that was inputed if text mode was on
		public string textInput;

		//Which alphabet (essentially the font) that was being used. 0 corresponds to the smaller alphabet, 1 to the bigger
		public int alphabetNumber;

		
		
		
	}

	//Queue of inputs to go through when using the inputs to build the texture
    Queue<InputInformation> myQueue;

	//the canvas of the students
	public Texture2D studentCanvas;

	//A fully black texture that gets put up when the user isn't allowed to see the canvas

	Texture2D blankCanvas;

	//Texture for the mask (the part that shows where the user will be drawing)

	Texture2D maskCanvas;

	//An empty canvas mask to copy into the maskCanvas to reset it
	Texture2D unMarkedCanvasMask;

	//The size of the brush of the current user (linked to ui)
    int brushSize;

	//how wide the canvas is in pixels
	int canvasWidth;

	//how long the canvas is in pixels
	int canvasHeight;

	//how wide is one grid character in alphabet.png
	int textWidth;

	//how high is the text character in alphabet.png
	int textHeight;

	//Is the player erasing
	bool eraseMode;

	//Is the player typing things in
	bool textMode;

	//Is the player using the line tool
	bool lineMode;
	Vector2 previousLineCoord;

    //Has the player clicked save canvas button
	bool canSave;

	//Has the player clicked load canvas button
	bool canLoad;
	
	//Whether the canvas is the last canvas clicked on. Unused as of right now
	bool selected;

    //string for typed text
    string textOnType;

	//what color the user wants to paint with
	Color brushColor;

	//alphabet of characters
	Texture2D alphabet;
	Texture2D alphabet2;
	Texture2D alphabetUsed; //Alphabet that gets sent

	Texture2D inputAlphabet; //Recieved alphabet

	//Which alphabet (essentially the font) that was being used. 0 corresponds to the smaller alphabet, 1 to the bigger
	int alphabetNumber;

	//The amount of times it redraws the square between the previous mouse click and the next. THis allows for the drawing to be smooth
	public int numberOfInterpolations;

	//Previous coordinate that the mouse hit, used for interpolation
	Vector2 previousCoord;

	//Whether the mouse was down on the previous frame or not
	bool previousMouseDown = false;

	
	//The renderer to be used by the canvas mask
    public Renderer maskRenderer;
	
	//Method that stores the last built InputInfo, for debugging purposes mostly. 
    InputInformation inputInfo;

	//All the UI fields that need to be linked

	//Color fields
	[SerializeField] Slider rSlider, gSlider, bSlider;
	[SerializeField] InputField rInput, gInput, bInput;

	//Fields for text and brush sizes
	[SerializeField] InputField textField = null;
	[SerializeField] InputField brushSizeInput = null;
	[SerializeField] Slider brushSizeSlider = null;

	//Buttons for deleting and loading things into the canvas
	[SerializeField] Button loadB = null;
	[SerializeField] Button deleteB = null;
	

	//Toggles for the different canvas modes
	[SerializeField] Toggle eraseTog = null;
	[SerializeField] Toggle textTog = null;
	[SerializeField] Toggle lineTog = null;

	//Field for changing the text size
	[SerializeField] Dropdown textSizeDrop = null;

	//Special UI field containing a circle gameobject which gets colored the sanme as the brush color
    [SerializeField] GameObject brushColorUI;
	//End of UI fields ---------------------------------------------------------------------------------------------------------

	//Whether the image being loaded is done being loaded
	bool doneLoading;

	//Whether the pointer is on the canvas, used for the canvas mask
	bool OnCanvas;
	
	//Whether the left controller is on the canvas, used for the vr canvas mask
	bool OnCanvasLeft;

	//Whether the right controller is on the canvas, used for the vr canvas mask
	bool OnCanvasRight;

	//Whether the canvas mask is blank

	bool maskIsEmpty;

	//List of left vr input devices (should only be one)

	List<InputDevice>  leftDevices;

	//List of right vr input devices (should only be one)

	List<InputDevice>  rightDevices;

	//Whether the trigger on the left controller was down in the previous frame
	bool previousTriggerDownLeft;

	//Whether the trigger on the right controller was down in the previous frame

	bool previousTriggerDownRight;

	//Where the left controller hit on the canvas

	Vector2 previousVRCoordL;

	//Where the right contoller hit on the canvas

	Vector2 previousVRCoordR;

	
	
    // Start is called before the first frame update
    void Start()
    {

        //Sets what to do with the float array when recieved
        GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(recieveInput);

		//Finds and sets the VR devices, will need to be changed if switching from windows to VR is desired
		leftDevices = new List<InputDevice>();
		var desiredCharacteristicsLeft = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
		InputDevices.GetDevicesWithCharacteristics(desiredCharacteristicsLeft, leftDevices);

		rightDevices = new List<InputDevice>();
		var desiredCharacteristicsRight = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
		InputDevices.GetDevicesWithCharacteristics(desiredCharacteristicsRight, rightDevices);
		
		//Statup initialization of variables
        myQueue = new Queue<InputInformation>();
        brushSize = 10;
		canvasWidth = 768;
		canvasHeight = 512;
		textWidth = 7;
		textHeight = 14;
		eraseMode = false;
		textMode = false;
        lineMode = false;
		canSave = false;
		canLoad = false;
		doneLoading = true;
		selected = false;
		OnCanvas = false;
		OnCanvasLeft = false;
		OnCanvasRight = false;
		textOnType = "";
		brushColor = Color.black;
        previousCoord = new Vector2(0, 0);

		allowedPlayer = true;

        alphabet = Resources.Load("alphabet", typeof(Texture2D)) as Texture2D;
		alphabet2 = Resources.Load("alphabet2", typeof(Texture2D)) as Texture2D;

        alphabetUsed = alphabet;

		alphabetNumber = 0;

        
		brushColorUI.GetComponent<Image>().color = brushColor;

		
        

		//Start up initialization of textures, can maybe be improved?

        studentCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
		maskCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
		blankCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
		unMarkedCanvasMask = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false)
;
        for (int x = 0; x < canvasWidth; x++)
		{
			for (int y = 0; y < canvasHeight; y++)
			{
				studentCanvas.SetPixel(x, y, Color.white);
				maskCanvas.SetPixel(x, y, Color.clear);
				blankCanvas.SetPixel(x,y, Color.black );
				unMarkedCanvasMask.SetPixel(x,y, Color.clear);

			}
		}
		studentCanvas.Apply();
		maskCanvas.Apply();
		blankCanvas.Apply();
		unMarkedCanvasMask.Apply();


        gameObject.GetComponent<Renderer>().material.mainTexture = studentCanvas;
		maskRenderer.material.mainTexture = maskCanvas;

        
		

        // Adding Listeners to all relevant objects.

		textSizeDrop.onValueChanged.AddListener(ChangeTextSize);

		rSlider.onValueChanged.AddListener(delegate { ChangeRed(rSlider.value); });
		gSlider.onValueChanged.AddListener(delegate { ChangeGreen(gSlider.value); });
		bSlider.onValueChanged.AddListener(delegate { ChangeBlue(bSlider.value); });
		

		rInput.onEndEdit.AddListener(ChangeRed);
		gInput.onEndEdit.AddListener(ChangeGreen);
		bInput.onEndEdit.AddListener(ChangeBlue);

		textField.onEndEdit.AddListener(SetTextOnType);
		brushSizeInput.onEndEdit.AddListener(SetBrushSize);
		brushSizeSlider.onValueChanged.AddListener(delegate { SetBrushSize(brushSizeSlider.value); });

		

		eraseTog.onValueChanged.AddListener(SetErase);
		textTog.onValueChanged.AddListener(SetText);
		lineTog.onValueChanged.AddListener(SetLine);

		

		deleteB.onClick.AddListener(sendClearCanvas);

		loadB.onClick.AddListener(SetCanLoad);


		//Starts the loop for building user inputs on the canvas
		StartCoroutine(UpdateCanvas());

		//Enables the starting players who can access and view the canvas
		if(!allowForEveryone)
		{
			

			//enableCanvasLocal();
			disableCanvasLocal();
			
			for(int i = 0; i < startingAllowedPlayers.Count; i++)
			{
				
				StartCoroutine(enableCanvasForPlayer(startingAllowedPlayers[i])); 
				
			}
		}
		else{
			
			enableCanvasLocal();
		}
        
		if(!visibleForEveryone)
			{
			disableViewingLocal();
			for(int i = 0; i < startingAllowedViewers.Count; i++)
			{
				StartCoroutine(enableViewingForPlayer(startingAllowedViewers[i]));
			}
		}
		else{
			enableViewingLocal();
		}
		
    }


	//Method gets necessary inputs and sends them across the network
	void GetAndSendInputs()
	{

		//Updates the canvas mask
		UpdateMask();

		//Get inputs to send over for the canvas
		int playerID = ASL.GameLiftManager.GetInstance().m_PeerId;

		//Activates when the mouse isn't over UI and was pressed (and not text mode since it only activates on button up)
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			if (Input.GetMouseButton(0) == true && !textMode)
			{
				
				//Checks if CanvasInput hit anything
				if(CanvasInput.Instance.getRaycastHitObject())
				{
					
					RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHit();

					//If the raycast hit this canvas, send the inputs
					if (raycastHit.transform == this.transform)
					{
						selected = true;
						Vector2 uv = raycastHit.textureCoord;
						
						Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));

						
						
						inputInfo.canvasClick = pixelCoord;
						inputInfo.textMode = this.textMode;
						inputInfo.eraseMode = this.eraseMode;
						inputInfo.lineMode = this.lineMode;
						inputInfo.previousCanvasClick = this.previousCoord;
						inputInfo.previousMouseDown = this.previousMouseDown;
						inputInfo.brushColor = this.brushColor;
						inputInfo.textInput = textOnType;
						inputInfo.brushSize = this.brushSize;
						inputInfo.alphabetNumber = alphabetNumber;
						

						if(lineMode)
						{
							if(!previousMouseDown)
							{
								previousCoord = pixelCoord;
							}
						}
						else 
						{
							previousCoord = pixelCoord;
						}
						previousMouseDown = true;

						//Sends the inputs if not in line mode (line mode sends on button up only, but still needs some of the previous click information for interpolation)
						if(!lineMode)
						{
							SendInput(inputInfo);
						}
						
					}
					else
					{
						
						selected = false;
						previousMouseDown = false;
					}
				}
				
			}
			//Activates when the mouse is up and the canvas is in line mode or text mode
			if(Input.GetMouseButtonUp(0))
			{
				if(textMode || lineMode)
				{
					//Checks if raycast hit anything
					if(CanvasInput.Instance.getRaycastHitObject())
					{
						
						RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHit();
						
						//If the raycast hit the canvas, create and send the inputs
						if (raycastHit.transform == this.transform)
						{
							selected = true;
							Vector2 uv = raycastHit.textureCoord;
							
							Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));

							inputInfo.canvasClick = pixelCoord;
							inputInfo.textMode = this.textMode;
							inputInfo.eraseMode = this.eraseMode;
							inputInfo.lineMode = this.lineMode;
							inputInfo.previousCanvasClick = this.previousCoord;
							inputInfo.previousMouseDown = this.previousMouseDown;
							inputInfo.brushColor = this.brushColor;
							inputInfo.textInput = textOnType;
							inputInfo.brushSize = this.brushSize;
							inputInfo.alphabetNumber = alphabetNumber;
							

							SendInput(inputInfo);
							
							
							
							
						}
						else
						{
							selected = false;
						}
					}
					

				}
				previousMouseDown = false;
				
			}
			
		}
	}

	//Same as GetAndSendInput, but for VR. Essentially, does getAndSendInput but twice, once for each controller. 
	void GetAndSendInputsVR()
	{
		//Updates the canvas mask
		UpdateMaskVR();

		
		bool triggerDownLeft;
		if (leftDevices[0].TryGetFeatureValue(CommonUsages.triggerButton, out triggerDownLeft) && triggerDownLeft)
		{
			
			if(CanvasInput.Instance.getRaycastHitObjectVR(0))
			{
				
				RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHitVR()[0];
				

				if (raycastHit.transform == this.transform)
				{
					
					selected = true;
					Vector2 uv = raycastHit.textureCoord;
					
					Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));

					
					
					inputInfo.canvasClick = pixelCoord;
					inputInfo.textMode = this.textMode;
					inputInfo.eraseMode = this.eraseMode;
					inputInfo.lineMode = this.lineMode;
					inputInfo.previousCanvasClick = this.previousVRCoordL;
					inputInfo.previousMouseDown = this.previousTriggerDownLeft;
					inputInfo.brushColor = this.brushColor;
					inputInfo.textInput = textOnType;
					inputInfo.brushSize = this.brushSize;
					inputInfo.alphabetNumber = alphabetNumber;
					

					
					if(lineMode)
					{
						if(!previousTriggerDownLeft)
						{
							this.previousVRCoordL = pixelCoord;
						}
					}
					else 
					{
						this.previousVRCoordL = pixelCoord;
					}
					previousTriggerDownLeft = true;
					if(!lineMode)
					{
						SendInput(inputInfo);
					}
					
				}
				else
				{
					
					selected = false;
					previousTriggerDownLeft = false;
				}
			}
			
		}
		
		if(!triggerDownLeft && previousTriggerDownLeft)
		{
			if(textMode || lineMode)
			{
				if(CanvasInput.Instance.getRaycastHitObjectVR(0))
				{
					
					RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHitVR()[0];
					
					
					



					if (raycastHit.transform == this.transform)
					{
						selected = true;
						Vector2 uv = raycastHit.textureCoord;
						
						Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));

						inputInfo.canvasClick = pixelCoord;
						inputInfo.textMode = this.textMode;
						inputInfo.eraseMode = this.eraseMode;
						inputInfo.lineMode = this.lineMode;
						inputInfo.previousCanvasClick = this.previousVRCoordL;
						inputInfo.previousMouseDown = this.previousTriggerDownLeft;
						inputInfo.brushColor = this.brushColor;
						inputInfo.textInput = textOnType;
						inputInfo.brushSize = this.brushSize;
						inputInfo.alphabetNumber = alphabetNumber;
						

						SendInput(inputInfo);
						
						
						
						
					}
					else
					{
						selected = false;
					}
				}
				

			}
			previousTriggerDownLeft = false;
			
		}

		bool triggerDownRight;
		if (rightDevices[0].TryGetFeatureValue(CommonUsages.triggerButton, out triggerDownRight) && triggerDownRight)
		{
			
			if(CanvasInput.Instance.getRaycastHitObjectVR(1))
			{
				
				RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHitVR()[1];

				if (raycastHit.transform == this.transform)
				{
					selected = true;
					Vector2 uv = raycastHit.textureCoord;
					
					Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));

					
					
					inputInfo.canvasClick = pixelCoord;
					inputInfo.textMode = this.textMode;
					inputInfo.eraseMode = this.eraseMode;
					inputInfo.lineMode = this.lineMode;
					inputInfo.previousCanvasClick = this.previousVRCoordR;
					inputInfo.previousMouseDown = this.previousTriggerDownRight;
					inputInfo.brushColor = this.brushColor;
					inputInfo.textInput = textOnType;
					inputInfo.brushSize = this.brushSize;
					inputInfo.alphabetNumber = alphabetNumber;
					

					
					if(lineMode)
					{
						if(!previousTriggerDownRight)
						{
							this.previousVRCoordR = pixelCoord;
						}
					}
					
					else 
					{
						this.previousVRCoordR = pixelCoord;
					}
					previousTriggerDownRight = true;
					if(!lineMode)
					{
						SendInput(inputInfo);
					}
					
				}
				else
				{
					
					selected = false;
					previousTriggerDownRight = false;
				}
			}
			
		}
		
		if(!triggerDownRight && previousTriggerDownRight)
		{
			if(textMode || lineMode)
			{
				if(CanvasInput.Instance.getRaycastHitObjectVR(1))
				{
					
					RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHitVR()[1];
					
					
					



					if (raycastHit.transform == this.transform)
					{
						selected = true;
						Vector2 uv = raycastHit.textureCoord;
						
						Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));

						inputInfo.canvasClick = pixelCoord;
						inputInfo.textMode = this.textMode;
						inputInfo.eraseMode = this.eraseMode;
						inputInfo.lineMode = this.lineMode;
						inputInfo.previousCanvasClick = this.previousVRCoordR;
						inputInfo.previousMouseDown = this.previousTriggerDownRight;
						inputInfo.brushColor = this.brushColor;
						inputInfo.textInput = textOnType;
						inputInfo.brushSize = this.brushSize;
						inputInfo.alphabetNumber = alphabetNumber;
						

						SendInput(inputInfo);
						
						
						
						
					}
					else
					{
						selected = false;
					}
				}
				

			}
			previousTriggerDownRight = false;
			
		}
			
		
	}

	
    void Update()
    {
        //If player is allowed to draw on the canvas, send the player's inputs
		if(allowedPlayer)
		{
			
			if(PlayerController.isXRActive)
			{
				GetAndSendInputsVR();
			}
			else{
				GetAndSendInputs();
			}

			
			
		}
			
		

    }

	//Method reads through the input queue and builds the inputs
    IEnumerator UpdateCanvas()
	{
        
		while(true)
		{
            
			while(myQueue.Count != 0)
			{
                
				InputInformation inp = myQueue.Dequeue();
				if(inp.ClearCanvas)
				{
					ClearCanvas();
				}
				else if(inp.loadTexture)
				{
					applyTexture(inp.textureToLoad);
				}
				else if(inp.textMode)
				{
					
					DrawText(inp);
				}
				else if(inp.eraseMode)
				{
					
					Draw(inp, Color.white);
				}
				else if(inp.lineMode)
				{
					
					DrawLine(inp);
				}
				else{
					Draw(inp, inp.brushColor);
				}

			}
            yield return null;
		}
	}

	//Method draws using the InputInformation. 
    void Draw(InputInformation inp, Color brush)
	{
		//Dictionary for making sure we aren't applying repeats (not sure if repeats hinders performance when the changes get applied to the texture)
		Dictionary<(int, int), bool> dictionary = new Dictionary<(int, int), bool>();
        if (!inp.previousMouseDown)
        {
            //draw area of brush size if greater than 1
            if (inp.brushSize > 1)
            {
                

				
                for (int x = (int)(inp.canvasClick.x - (inp.brushSize / 2)); x < (int)(inp.canvasClick.x + (inp.brushSize / 2)); x++)
                {
                    if (x >= canvasWidth || x < 0)
                    {
                        continue;
                    }
                    for (int y = (int)(inp.canvasClick.y - (inp.brushSize / 2)); y < (int)(inp.canvasClick.y + (inp.brushSize / 2)); y++)
                    {
                        if (y >= canvasHeight || y < 0)
                        {
                            continue;
                        }
                        // Set pixel on canvas to the current brush color
                        
						
                        studentCanvas.SetPixel(x, y, brush);
                        
                        
                    }
                }
                
            }
            else{
                int x = (int)inp.canvasClick.x;
                int y = (int)inp.canvasClick.y;
                studentCanvas.SetPixel(x, y, brush);
            }
        }
        //If the mouse was down, interpolate
        else
        {
            if (inp.brushSize > 1)
            {

                for (int i = 0; i < numberOfInterpolations; i++)
                {
                    float distanceX = (i * (inp.canvasClick.x - inp.previousCanvasClick.x) / numberOfInterpolations);
                    float distanceY = (i * (inp.canvasClick.y - inp.previousCanvasClick.y) / numberOfInterpolations);

                    // Make sure no other toggle are on
                    
                    
                    for (int x = (int)(inp.canvasClick.x - distanceX - (inp.brushSize / 2)); x < (int)(inp.canvasClick.x - distanceX + (inp.brushSize / 2)); x++)
                    {
                        if (x >= canvasWidth || x < 0)
                        {
                            continue;
                        }
                        for (int y = (int)(inp.canvasClick.y - distanceY - (inp.brushSize / 2)); y < (int)(inp.canvasClick.y - distanceY + (inp.brushSize / 2)); y++)
                        {
                            if (y >= canvasHeight || y < 0)
                            {
                                continue;
                            }
                            if(!dictionary.ContainsKey((x, y)))
							{
								dictionary.Add((x, y), true);
                            	studentCanvas.SetPixel(x, y, brush);
							}
							
                            
                        }
                    }
                    
                }

            }
            else
            {

                
                for (int i = 0; i < numberOfInterpolations; i++)
                {
                    float distanceX = (i * (inp.canvasClick.x - inp.previousCanvasClick.x) / numberOfInterpolations);
                    float distanceY = (i * (inp.canvasClick.y - inp.previousCanvasClick.y) / numberOfInterpolations);
                    
					int x = (int)(inp.canvasClick.x - distanceX);
					int y = (int)(inp.canvasClick.y - distanceY);
                    if(!dictionary.ContainsKey((x, y)))
					{
						dictionary.Add((x, y), true);
						studentCanvas.SetPixel(x, y, brush);
					}
                    
                    
                }
                
                
            }
            
        }

		//Applies all the changes made to the texture
		studentCanvas.Apply();
	}
	//Gets alphabet and draws the text
	void DrawText(InputInformation inp)
	{
		
		Vector2 startChar = new Vector2(inp.canvasClick.x, inp.canvasClick.y);
		int tWidth = 0;
		int tHeight = 0;
		if(inp.alphabetNumber == 0)
		{
			
			inputAlphabet = alphabet;
			tWidth = 7;
			tHeight = 14;
		}
		else if(inp.alphabetNumber == 1)
		{
			
			inputAlphabet = alphabet2;
			tWidth = 12;
			tHeight = 28;
		}
		
		for (int i = 0; i < inp.textInput.Length; i++)
		{
			int spot = DetermineCharacter(inp.textInput[i]);
			if (spot != -1)
			{
				
				DrawCharacter(startChar, spot, tWidth, tHeight, inp);
				startChar.x += tWidth * inp.brushSize;
				startChar.x += 1;
			}
		}
	}
	//Determines which character should be drawn
	int DetermineCharacter(char c)
	{
		int modifiedVal = -1;
		if (c >= 97 && c < 123)
		{
			modifiedVal = c - 97;
		}
		else if (c >= 48 && c < 58)
		{
			modifiedVal = c - 48 + 26;
		}
		else if (c >= 32 && c < 48)
		{
			modifiedVal = c + 30;
		}
		else if (c >= 65 && c < 91)
		{
			modifiedVal = c - 29;
		}
		else if (c >= 58 && c < 65)
		{
			modifiedVal = c + 20;
		}
		return modifiedVal;
	}

	//Draws a single character
	void DrawCharacter(Vector2 currUV, int spot, int tWidth, int tHeight, InputInformation inp)
	{
		Dictionary<(int, int), bool> dictionary = new Dictionary<(int, int), bool>();
		spot *= tWidth;
		int currX = (int)currUV.x;
		int currY = (int)currUV.y;
		
		
		for (int x = spot; x < (spot + tWidth); x++)
		{
			for (int y = 0; y < tHeight; y++)
			{
				Color pixelColor = inputAlphabet.GetPixel(x, y);
				if (pixelColor.a != 0)
				{
					pixelColor = inp.brushColor;
					for (int i = 0; i < inp.brushSize; i++)
					{
						for (int j = 0; j < inp.brushSize; j++)
						{
							if (((currX + j) < canvasWidth) && (currY + ((y - 2) * inp.brushSize) + i < canvasHeight))
							{
								int tempX = currX + j;
								int tempY = currY + ((y - 2) * inp.brushSize) + i;
								if(!dictionary.ContainsKey((tempX, tempY)))
								{
									dictionary.Add((tempX, tempY), true);
									studentCanvas.SetPixel(tempX, tempY, pixelColor);
								}
								
							}

						}

					}

				}

			}
			currX += inp.brushSize;
		}

		studentCanvas.Apply();
	}

	void DrawLine(InputInformation inp) 
	{
		//Old way of drawing line
		
		/*
		Dictionary<(int, int), bool> dictionary = new Dictionary<(int, int), bool>();
		float lineInterpolations = 1000 - 9 * inp.brushSize;
		for (int i = 0; i < lineInterpolations; i++)
		{
			float distanceX = (i * (inp.canvasClick.x - inp.previousCanvasClick.x) / lineInterpolations);
			float distanceY = (i * (inp.canvasClick.y - inp.previousCanvasClick.y) / lineInterpolations);
			for (int x = (int)(inp.canvasClick.x - distanceX - ((float)inp.brushSize / 2)); x < (int)(inp.canvasClick.x - distanceX + ((float)inp.brushSize / 2)); x++)
			{
				if (x >= canvasWidth || x < 0)
				{
					continue;
				}
				for (int y = (int)(inp.canvasClick.y - distanceY - ((float)inp.brushSize / 2)); y < (int)(inp.canvasClick.y - distanceY + ((float)inp.brushSize / 2)); y++)
				{
					if (y >= canvasHeight || y < 0)
					{
						continue;
					}
					if (inp.eraseMode == false)
					{
						if(!dictionary.ContainsKey((x, y)))
						{
							dictionary.Add((x, y), true);
							studentCanvas.SetPixel(x, y, brushColor);
						}
					}
					else
					{
						if(!dictionary.ContainsKey((x, y)))
						{
							dictionary.Add((x, y), true);
							studentCanvas.SetPixel(x, y, Color.white);
						}
					}
				}
			}
		}
		studentCanvas.Apply();
		*/

		//This method interpolates between the two points to form a line
		//This method works by incrementing either the x value or the y value by 1
		//This way calculates the pixel locations accurately without any repeats caused from the rounding of the hypotenuse.

		
		if(inp.previousCanvasClick == inp.canvasClick)
		{
			return;
		}
		float yDistance = inp.previousCanvasClick.y - inp.canvasClick.y;
		float xDistance =  inp.previousCanvasClick.x - inp.canvasClick.x;
		float xOverY = Math.Abs(xDistance / yDistance) * (xDistance < 0 ? 1 : -1);
		float yOverX = Math.Abs(yDistance / xDistance) * (yDistance < 0 ? 1 : -1);

		//Check which of the distances is greater to determine when interpolating which direction (x or y) gets incremented by 1.
		//If the y is greater, then it gets incremented by 1, else the x gets incremented by 1
		if(Math.Abs(yDistance) >= Math.Abs(xDistance))
		{
			
			
			
			int increment = (yDistance < 0) ? 1 : -1;
			float horizontalHeight = (float)(Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance)) / Math.Abs(yDistance)) * inp.brushSize;

			
			
			for(int i = 0; (i < Math.Abs(yDistance)); i++)
			{
				
				for(int j = 0; j < horizontalHeight/2; j++)
				{
					Vector2 interpolationOffset = new Vector2(i * xOverY, i * increment);
					Vector2 brushOffset = new Vector2(j, 0);
					Vector2 pixelToSet = inp.previousCanvasClick + interpolationOffset + brushOffset;
					
					
					studentCanvas.SetPixel((int)pixelToSet.x, (int)pixelToSet.y, inp.brushColor);
					pixelToSet = inp.previousCanvasClick + interpolationOffset - brushOffset;
					studentCanvas.SetPixel((int)pixelToSet.x, (int)pixelToSet.y, inp.brushColor);
					
				}
				

			}
		}
		else{
			
			int increment = (xDistance < 0) ? 1 : -1;
			float verticalHeight = (float)(Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance)) / Math.Abs(xDistance)) * inp.brushSize;

			for(int i = 0; (i < Math.Abs(xDistance)); i++)
			{
				for(int j = 0; j < verticalHeight/2; j++)
				{
					Vector2 interpolationOffset = new Vector2(i * increment, i * yOverX);
					Vector2 brushOffset = new Vector2(0, j);
					Vector2 pixelToSet = inp.previousCanvasClick + interpolationOffset + brushOffset;

					studentCanvas.SetPixel((int)pixelToSet.x, (int)pixelToSet.y, inp.brushColor);
					pixelToSet = inp.previousCanvasClick + interpolationOffset - brushOffset;
					studentCanvas.SetPixel((int)pixelToSet.x, (int)pixelToSet.y, inp.brushColor);
				}
				

			}
		}
		studentCanvas.Apply();
		


		

	}
	
	//Updates the area
    void UpdateMask()
    {
		
		if(CanvasInput.Instance.getRaycastHitObject() && CanvasInput.Instance.GetRaycastHit().transform == this.transform)
		{
			RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHit();
			
			
			OnCanvas = true;
			Vector2 uv = raycastHit.textureCoord;
			Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));
			
			Graphics.CopyTexture(unMarkedCanvasMask, maskCanvas);
			if(lineMode)
			{
				lineMask();
			}
			else if(textMode)
			{
				textMask(pixelCoord);
			}
			else if(eraseMode)
			{
				drawMask(pixelCoord, Color.white);
			}
			else{
				
				drawMask(pixelCoord, brushColor);
			}
			
			
		}
		else
		{
			if(OnCanvas)
			{
				for (int x = 0; x < canvasWidth; x++)
				{
					for (int y = 0; y < canvasHeight; y++)
					{
						maskCanvas.SetPixel(x, y, Color.clear);
					}
				}
				maskCanvas.Apply();

			}
			OnCanvas = false;
			
		}
		
    }
	//Works the same as update mask but takes in two VR controllers as inputs
	void UpdateMaskVR()
    {
		
		
		
		if(CanvasInput.Instance.getRaycastHitObjectVR(0) && CanvasInput.Instance.GetRaycastHitVR()[0].transform == this.transform)
		{
			RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHitVR()[0];
			
			maskIsEmpty = false;
			OnCanvasLeft = true;
			Graphics.CopyTexture(unMarkedCanvasMask, maskCanvas);
			Vector2 uv = raycastHit.textureCoord;
			Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));
			
			if(lineMode)
			{
				lineMask();
			}
			else if(textMode)
			{
				textMask(pixelCoord);
			}
			else if(eraseMode)
			{
				drawMask(pixelCoord, Color.white);
			}
			else{
				
				drawMask(pixelCoord, brushColor);
			}
			
			
		}
		else
		{
			
			OnCanvasLeft = false;
			
		}
		if(CanvasInput.Instance.getRaycastHitObjectVR(1) && CanvasInput.Instance.GetRaycastHitVR()[1].transform == this.transform)
		{
			RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHitVR()[1];

			maskIsEmpty = false;
			OnCanvasRight = true;
			if(OnCanvasLeft == false)
			{
				Graphics.CopyTexture(unMarkedCanvasMask, maskCanvas);
			}
			
			Vector2 uv = raycastHit.textureCoord;
			Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));
			if(lineMode)
			{
				lineMask();
			}
			else if(textMode)
			{
				textMask(pixelCoord);
			}
			else if(eraseMode)
			{
				drawMask(pixelCoord, Color.white);
			}
			else{
				
				drawMask(pixelCoord, brushColor);
			}
			
			
		}
		else
		{
			
			OnCanvasRight = false;
			
		}
		if(!maskIsEmpty && !OnCanvasLeft && !OnCanvasRight)
		{
			Graphics.CopyTexture(unMarkedCanvasMask, maskCanvas);
			maskIsEmpty = true;
		}
		
    }
	//The mask for draw, works the same way as the draw method but without interpolation
    void drawMask(Vector2 canvasClick, Color brush)
    {
		Color clearBrush = brush;
		clearBrush.a = 0.5f;
		
		if (brushSize > 1)
		{
			


			for (int x = (int)(canvasClick.x - (brushSize / 2)); x < (int)(canvasClick.x + (brushSize / 2)); x++)
			{
				if (x >= canvasWidth || x < 0)
				{
					continue;
				}
				for (int y = (int)(canvasClick.y - (brushSize / 2)); y < (int)(canvasClick.y + (brushSize / 2)); y++)
				{
					if (y >= canvasHeight || y < 0)
					{
						continue;
					}
					// Set pixel on canvas to the current brush color
					
					maskCanvas.SetPixel(x, y, clearBrush);
					
					
				}
			}
			
		}
		else{
			int x = (int)canvasClick.x;
			int y = (int)canvasClick.y;
			maskCanvas.SetPixel(x, y, clearBrush);
		}
		maskCanvas.Apply();
    }
	//The mask for the text, works the same as the draw text method
    void textMask(Vector2 pixelCoord)
    {
		if (textOnType.Equals("") == false && textMode == true)
		{
			Vector2 startChar = new Vector2(pixelCoord.x, pixelCoord.y);
			for (int i = 0; i < textOnType.Length; i++)
			{

				int spot = DetermineCharacter(textOnType[i]);
				if (spot != -1)
				{
					DrawCharacterMask(startChar, spot);
					startChar.x += textWidth * brushSize;
					startChar.x += 1;
				}
			}

		}
    }
	//same as drawcharacter, but for the mask
	void DrawCharacterMask(Vector2 currUV, int spot)
	{
		spot *= textWidth;
		int currX = (int)currUV.x;
		int currY = (int)currUV.y;
		for (int x = spot; x < (spot + textWidth); x++)
		{
			for (int y = 0; y < textHeight; y++)
			{

				Color pixelColor = alphabetUsed.GetPixel(x, y);
				if (pixelColor.a != 0)
				{
					pixelColor = brushColor;
					pixelColor.a = 0.5f;
					for (int i = 0; i < brushSize; i++)
					{
						for (int j = 0; j < brushSize; j++)
						{
							if (((currX + j) < canvasWidth) && (currY + ((y - 2) * brushSize) + i < canvasHeight))
							{
								maskCanvas.SetPixel(currX + j, currY + ((y - 2) * brushSize) + i, pixelColor);
							}


						}



					}

				}

			}
			currX += brushSize;
		}
		maskCanvas.Apply();
	}
    //Not implemented yet.
    void lineMask()
    {

    }

    //Methods for UI stuff
    public void ChangeToWhite(string s)
	{
		GameObject.Find("SaveField").GetComponent<Image>().color = Color.white;
	}
    public void SetErase(bool erase)
	{
		eraseMode = erase;
		if (eraseMode == true)
		{
			textMode = false;
			lineMode = false;

			textTog.isOn = false;
			lineTog.isOn = false;

			textField.GetComponent<InputField>().interactable = false;
		}
	}
    public void SetText(bool text)
	{
		textMode = text;
		if (textMode == true)
		{
			// Turning Modes off 
			eraseMode = false;
			lineMode = false;

			// Turning toggles off
			lineTog.isOn = false;
			eraseTog.isOn = false;

			textField.interactable = true;
			//GameObject.Find("SaveField").GetComponent<InputField>().interactable = false;

			canLoad = false;
			canSave = false;
		}
		else
		{
			textField.GetComponent<InputField>().interactable = false;
		}
	}
    public void SetLine(bool line)
	{
		lineMode = line;
		if (line)
		{
			// Properly set the apprioate modes
			eraseMode = false;
			textMode = false;

			// Set other toggles to off
			textTog.isOn = false;
			eraseTog.isOn = false;
		}
	}

    public void SetTextOnType(string output)
	{
		textOnType = output;
	}
	public void SetBrushSize(string size)
	{
		int.TryParse(size, out brushSize);
		brushSizeSlider.value = brushSize;
	}

	public void SetBrushSize(float size)
	{
		brushSize = (int)size;
	}

	public void ChangeRed(string r)
	{
		float red;
		float.TryParse(r, out red);
		brushColor = new Color(red, brushColor.g, brushColor.b, brushColor.a);
		
		brushColorUI.GetComponent<Image>().color = brushColor;

		rSlider.value = red;
	}

	public void ChangeRed(float r)
	{
		//float red;
		//float.TryParse(r, out red);
		brushColor = new Color(r, brushColor.g, brushColor.b, brushColor.a);
		
		brushColorUI.GetComponent<Image>().color = brushColor;
	}

	public void ChangeGreen(string g)
	{
		float green;
		float.TryParse(g, out green);
		brushColor = new Color(brushColor.r, green, brushColor.b, brushColor.a);
		
		brushColorUI.GetComponent<Image>().color = brushColor;

		gSlider.value = green;
	}

	public void ChangeGreen(float g)
	{
		//float green;
		//float.TryParse(g, out green);
		brushColor = new Color(brushColor.r, g, brushColor.b, brushColor.a);
		
		brushColorUI.GetComponent<Image>().color = brushColor;
	}

	public void ChangeBlue(string b)
	{
		float blue;
		float.TryParse(b, out blue);
		brushColor = new Color(brushColor.b, brushColor.g, blue, brushColor.a);
		
		brushColorUI.GetComponent<Image>().color = brushColor;

		bSlider.value = blue;
	}

	public void ChangeBlue(float b)
	{
		//float blue;
		//float.TryParse(b, out blue);
		brushColor = new Color(brushColor.r, brushColor.g, b, brushColor.a);
		
		brushColorUI.GetComponent<Image>().color = brushColor;
	}
	public void ChangeAlpha(string a)
	{
		float alpha;
		float.TryParse(a, out alpha);
		brushColor = new Color(brushColor.r, brushColor.g, brushColor.b, alpha);
		
		brushColorUI.GetComponent<Image>().color = brushColor;
	}
	
	public void ChangeTextSize(int option)
	{
		if (option == 0)
		{
			textWidth = 7;
			textHeight = 14;
			alphabetUsed = alphabet;
			alphabetNumber = 0;
		}
		else if (option == 1)
		{
			textWidth = 12;
			textHeight = 28;
			alphabetUsed = alphabet2;
			alphabetNumber = 1;
		}
	}

	//Method for load button, starts the simplefilebrowser for selecting an image
	public void SetCanLoad()
	{
		
		if (canSave == false && doneLoading)
		{
			doneLoading = false;
			canLoad = true;
			Texture2D newPng = new Texture2D(1, 1);
			StartCoroutine(LoadWindow(newPng));
			
		}
		canLoad = false;
		
		
		
		

	}
	//Opens up simplefielbrowser and waits for user to put in a file. Then it makes a texture out of it and Sends the texture.
	IEnumerator LoadWindow(Texture2D texture)
	{
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true,
			null, null, "Load Files and Folders", "Load");

		if (FileBrowser.Success && FileBrowser.Result.Length == 1)
		{
			doneLoading = true;
			Texture2D text2D = (Texture2D)texture;
			text2D.LoadImage(System.IO.File.ReadAllBytes(FileBrowser.Result[0]));
			text2D.Apply();
			SendTexture(texture);
			yield return FileBrowser.Result[0];
		}
		
		else
		{
			doneLoading = true;
		}
	}
	//Sends a texture using ASL. Notee that this is typically pretty expensive on the recieving end
	public void SendTexture(Texture2D newPng)
	{
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
			{
				this.GetComponent<ASL.ASLObject>().SendAndSetTexture2D(newPng,
					recieveTexture, true);
				
			});
	}
	//Methods called when a texture is recieved from ASL. It takes it, puts makes a InputInformation out of it, and puts it on the queue.
	public static void recieveTexture(GameObject gameObject, Texture2D tex)
	{
		gameObject.GetComponent<NewPaint>().makeTextureInput(tex);
	}
	public void makeTextureInput(Texture2D tex) 
	{
		InputInformation i = new InputInformation();
		i.loadTexture = true;
		i.ClearCanvas = false;
		i.textureToLoad = tex;
		myQueue.Enqueue(i);
	}

	//Applies a texture to the canvas.
	void applyTexture(Texture2D tex)
	{
		for (int x = 0; x < canvasWidth; x++)
		{
			for (int y = 0; y < canvasHeight; y++)
			{
				if (x < studentCanvas.width && y < studentCanvas.height)
				{
					float x0 = (float)tex.width/(float)canvasWidth;
					float y0 = (float)tex.height/(float)canvasHeight;
					studentCanvas.SetPixel(x, y, tex.GetPixel((int)(x * x0), (int)(y * y0)));
				}
			}
		}
		studentCanvas.Apply();
	}
	//Sends the command to clear the canvas
	void sendClearCanvas()
	{
		
		float[] fArray = {1};
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
			{
				GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
			});
		
		
	}
	//Clears the canvas. May be better to use Graphics.copy with a blank white texture, but since this isn't called that much this should be fine
	void ClearCanvas()
	{
		for (int x = 0; x < canvasWidth; x++)
		{
			for (int y = 0; y < canvasHeight; y++)
			{
				studentCanvas.SetPixel(x, y, Color.white);

			}
		}
		studentCanvas.Apply();
	}
	

		
	

    //Method turns input information into an array and sends it
    void SendInput(InputInformation i)
    {
        var fArray = ConstructFloatsFromInput(i);
        this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
    }

	//Method recieves ASL input and turns it back into InputInformation/calls commands with it
    public void recieveInput(string id, float[] i)
	{
		
        if(i.Length == 2) //Special case for allowing players access to the canvas
		{
			
			if(ASL.GameLiftManager.GetInstance().m_PeerId == i[1])
			{
				if(i[0] == 1)
				{
					enableCanvasLocal();
				}
				else if(i[0] == 2)
				{
					disableCanvasLocal();
				}
				else if(i[0] == 3)
				{
					enableViewingLocal();
				}
				else if(i[0] == 4)
				{
					disableViewingLocal();
				}
			}
		}
		else{
			InputInformation theInput = ConstructInputFromFloats(i);
        //Queues up the input to be built
        	myQueue.Enqueue(theInput);
		}
		
		
	}

    float[] ConstructFloatsFromInput(InputInformation i)
    {
        List<float> inputFloats = new List<float>
        {
            i.canvasClick.x, 
            i.canvasClick.y,
            System.Convert.ToInt16(i.mouseDown),
            i.previousCanvasClick.x, 
            i.previousCanvasClick.y,
            System.Convert.ToInt16(i.previousMouseDown),
            i.brushColor.r,
            i.brushColor.g,
            i.brushColor.b,
            i.brushSize,
            System.Convert.ToInt16(i.eraseMode),
            System.Convert.ToInt16(i.lineMode),
            System.Convert.ToInt16(i.textMode),
			i.alphabetNumber,
            i.textInput.Length
        };
        inputFloats.AddRange(stringToFloats(i.textInput));

            
        return inputFloats.ToArray();
        

    }
    InputInformation ConstructInputFromFloats(float[] i)
    {
		
        InputInformation inputInfoNew = new InputInformation();
		if(i.Length == 1) //Special case for clear command
		{
			inputInfoNew.ClearCanvas = true;
			
		}
		
		else{

		
			inputInfoNew.canvasClick = new Vector2(i[0], i[1]);
			inputInfoNew.mouseDown = System.Convert.ToBoolean(i[2]);
			inputInfoNew.previousCanvasClick = new Vector2(i[3], i[4]);
			inputInfoNew.previousMouseDown = System.Convert.ToBoolean(i[5]);
			inputInfoNew.brushColor = new Color(i[6], i[7], i[8], 1);
			inputInfoNew.brushSize = (int)i[9];
			inputInfoNew.eraseMode = System.Convert.ToBoolean(i[10]);
			inputInfoNew.lineMode = System.Convert.ToBoolean(i[11]);
			inputInfoNew.textMode = System.Convert.ToBoolean(i[12]);
			inputInfoNew.alphabetNumber = (int)i[13];
			string text = "";
			for(int j = 0; j < i[14]; j++)
			{
				text += (char)i[j + 15];
			}

			inputInfoNew.textInput = text;

			//Unecessary dead code (I think)
			inputInfoNew.ClearCanvas = false; 
			inputInfo.loadTexture = false;
			

			
		}
		return inputInfoNew;

    }
    

    List<float> stringToFloats(string toConvert) {
        var floats = new List<float>();
        foreach (var c in toConvert) {
            floats.Add((int)c);
        }
        return floats;
    }
    


}
