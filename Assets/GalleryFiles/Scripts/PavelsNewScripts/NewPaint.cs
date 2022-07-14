/* PaintOnCanvas.cs
 * Author: Tyler Miller
 * Date: 4/7/2022
 * Description: PaintOnCanvas allows user to create a blank canvas
 * then paint on it. User must save canvas before quitting application
 * to save progress. The load button will allow the user to load a png.
 * The user is allowed to change the brush size and color. User can type
 * text in two sizes 7 pixels wide or 12 pixels wide.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using UnityEngine.EventSystems;
using SimpleFileBrowser;

public class NewPaint : MonoBehaviour
{
	bool allowedPlayer;

	public void enableCanvasLocal()
	{
		allowedPlayer = true;
		gameObject.GetComponent<Renderer>().material.mainTexture = studentCanvas;

	}
	public void disableCanvasLocal()
	{
		allowedPlayer = false;
		gameObject.GetComponent<Renderer>().material.mainTexture = blankCanvas;
	}
	public void enableCanvasForStudent(int peerID)
	{
		float[] fArray = {1, peerID};
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
	}
	public void disableCanvasForStudent(int peerID)
	{
		float[] fArray = {2, peerID};
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
	}
	
	

    struct InputInformation //: IEquatable<InputInformation>, IComparable<InputInformation>
	{


		//public int peerID;
		public bool ClearCanvas;

		public bool loadTexture;
		public Texture2D textureToLoad;
		public Vector2 canvasClick;

		public bool mouseDown;
		public bool previousMouseDown;
		public Vector2 previousCanvasClick;

		public Color brushColor;
		public int brushSize;

		public bool eraseMode;
		public bool textMode;
		public bool lineMode;

		public string textInput;

		public int alphabetNumber;

		
		
		
	}

    Queue<InputInformation> myQueue;

	//the canvas of the students
	public Texture2D studentCanvas;

	Texture2D blankCanvas;

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
	
	//Whether the canvas is selected
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

	int alphabetNumber;

    Vector2 pixelToDraw;

	public int numberOfInterpolations;

	// Current Canvas has been loaded
	bool clicked = false;
	float freeze = 1f;
	float maskClock = 0.2f;

	Vector2 previousCoord;

	bool previousMouseDown = false;

    public Renderer maskRenderer;
	Texture2D maskCanvas;
	Color maskColor;

    InputInformation inputInfo;


    // UI field listeners
	Slider rSlider, gSlider, bSlider;
	InputField rInput, gInput, bInput;
	InputField aField = null;

	InputField slField = null;
	InputField textField = null;
	InputField brushSizeInput = null;
	Slider brushSizeSlider = null;

	// UI button listeners
	Button loadB = null;
	Button saveB = null;
	Button deleteB = null;
	Button controlsB = null;

	Button subGalB = null;
	Button subStuB = null;

	// UI toggle listeners
	Toggle eraseTog = null;
	Toggle textTog = null;
	Toggle lineTog = null;

	//UI dropdown listeners
	Dropdown textSizeDrop = null;

    GameObject brushColorUI;

	bool doneLoading;

	bool OnCanvas;

	
	
    // Start is called before the first frame update
    void Start()
    {

        
        GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(recieveInput);

		int numOfPlayers = ASL.GameLiftManager.GetInstance().m_Players.Count;
		

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
		textOnType = "";
		brushColor = Color.black;
		pixelToDraw = new Vector2(0, 0);
        previousCoord = new Vector2(0, 0);

		allowedPlayer = true;

        alphabet = Resources.Load("alphabet", typeof(Texture2D)) as Texture2D;
		alphabet2 = Resources.Load("alphabet2", typeof(Texture2D)) as Texture2D;

        alphabetUsed = alphabet;

		alphabetNumber = 0;

        GameObject brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;


        //Texture stuff
        studentCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
		maskCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
		blankCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);

        for (int x = 0; x < canvasWidth; x++)
		{
			for (int y = 0; y < canvasHeight; y++)
			{
				studentCanvas.SetPixel(x, y, Color.white);
				maskCanvas.SetPixel(x, y, Color.clear);
				blankCanvas.SetPixel(x,y, Color.black );

			}
		}
		studentCanvas.Apply();
		maskCanvas.Apply();
		blankCanvas.Apply();

        gameObject.GetComponent<Renderer>().material.mainTexture = studentCanvas;
		maskRenderer.material.mainTexture = maskCanvas;

        //UI stuff

		// UI field code
		rSlider = GameObject.Find("RedSlider").GetComponent<Slider>();
		gSlider = GameObject.Find("GreenSlider").GetComponent<Slider>();
		bSlider = GameObject.Find("BlueSlider").GetComponent<Slider>();
		//aField = GameObject.Find("AlphaInputField").GetComponent<InputField>();

		rInput = GameObject.Find("RedInputField").GetComponent<InputField>();
		gInput = GameObject.Find("GreenInputField").GetComponent<InputField>();
		bInput = GameObject.Find("BlueInputField").GetComponent<InputField>();

		//slField = GameObject.Find("SaveField").GetComponent<InputField>();
		textField = GameObject.Find("TextInput").GetComponent<InputField>();
		brushSizeInput = GameObject.Find("SizeInputField").GetComponent<InputField>();
		brushSizeSlider = GameObject.Find("BrushSizeSlider").GetComponent<Slider>();

		textSizeDrop = GameObject.Find("TextSizeDropdown").GetComponent<Dropdown>();

        // Adding Listeners to all relevant objects.

		textSizeDrop.onValueChanged.AddListener(ChangeTextSize);

		rSlider.onValueChanged.AddListener(delegate { ChangeRed(rSlider.value); });
		gSlider.onValueChanged.AddListener(delegate { ChangeGreen(gSlider.value); });
		bSlider.onValueChanged.AddListener(delegate { ChangeBlue(bSlider.value); });
		//aField.onEndEdit.AddListener(ChangeAlpha);

		rInput.onEndEdit.AddListener(ChangeRed);
		gInput.onEndEdit.AddListener(ChangeGreen);
		bInput.onEndEdit.AddListener(ChangeBlue);

		//slField.onEndEdit.AddListener(SaveOrLoadToPNG);
		//slField.onValueChanged.AddListener(ChangeToWhite);
		textField.onEndEdit.AddListener(SetTextOnType);
		brushSizeInput.onEndEdit.AddListener(SetBrushSize);
		brushSizeSlider.onValueChanged.AddListener(delegate { SetBrushSize(brushSizeSlider.value); });

		eraseTog = GameObject.Find("EraserToggle").GetComponent<Toggle>();
		textTog = GameObject.Find("TextToggle").GetComponent<Toggle>();
		lineTog = GameObject.Find("LineToggle").GetComponent<Toggle>();

		eraseTog.onValueChanged.AddListener(SetErase);
		textTog.onValueChanged.AddListener(SetText);
		lineTog.onValueChanged.AddListener(SetLine);

		deleteB = GameObject.Find("DeleteCanvasButton").GetComponent<Button>();
		
		loadB = GameObject.Find("LoadButton").GetComponent<Button>();

		deleteB.onClick.AddListener(sendClearCanvas);

		loadB.onClick.AddListener(SetCanLoad);

    

        StartCoroutine(UpdateCanvas());
    }

    // Update is called once per frame
    void Update()
    {
        // Makes sure that the paint brush mask is not applied every frame
		if(allowedPlayer)
		{
			UpdateMask();

			//Get inputs to send over for the canvas
			int playerID = ASL.GameLiftManager.GetInstance().m_PeerId;
			

			
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				if (Input.GetMouseButton(0) == true && !textMode)
				{
					
					
					if(CanvasInput.Instance.getRaycastHitObject())
					{
						
						RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHit();

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
				if(Input.GetMouseButtonUp(0))
				{
					if(textMode || lineMode)
					{
						if(CanvasInput.Instance.getRaycastHitObject())
						{
							
							RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHit();
							
							
							



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
			
		

    }
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
    void Draw(InputInformation inp, Color brush)
	{
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
                            
                            studentCanvas.SetPixel(x, y, brush);
                            
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
                    
                    studentCanvas.SetPixel((int)(inp.canvasClick.x - distanceX), (int)(inp.canvasClick.y - distanceY), brush);
                    
                    
                }
                
                
            }
            
        }
		studentCanvas.Apply();
	}
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
	//Helper methods for drawing text
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
	void DrawCharacter(Vector2 currUV, int spot, int tWidth, int tHeight, InputInformation inp)
	{
		
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
								studentCanvas.SetPixel(currX + j, currY + ((y - 2) * inp.brushSize) + i, pixelColor);
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
						studentCanvas.SetPixel(x, y, inp.brushColor);
					}
					else
					{
						studentCanvas.SetPixel(x, y, Color.white);
					}
				}
			}
		}
		studentCanvas.Apply();
	}
	
    void UpdateMask()
    {
		
		if(CanvasInput.Instance.getRaycastHitObject() && CanvasInput.Instance.GetRaycastHit().transform == this.transform)
		{
			RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHit();
			
			
			OnCanvas = true;
			Vector2 uv = raycastHit.textureCoord;
			Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));
			for (int x = 0; x < canvasWidth; x++)
			{
				for (int y = 0; y < canvasHeight; y++)
				{
					maskCanvas.SetPixel(x, y, Color.clear);
				}
			}
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

			GameObject.Find("TextInput").GetComponent<InputField>().interactable = false;
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

			GameObject.Find("TextInput").GetComponent<InputField>().interactable = true;
			//GameObject.Find("SaveField").GetComponent<InputField>().interactable = false;

			canLoad = false;
			canSave = false;
		}
		else
		{
			GameObject.Find("TextInput").GetComponent<InputField>().interactable = false;
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
		brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;

		rSlider.value = red;
	}

	public void ChangeRed(float r)
	{
		//float red;
		//float.TryParse(r, out red);
		brushColor = new Color(r, brushColor.g, brushColor.b, brushColor.a);
		brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;
	}

	public void ChangeGreen(string g)
	{
		float green;
		float.TryParse(g, out green);
		brushColor = new Color(brushColor.r, green, brushColor.b, brushColor.a);
		brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;

		gSlider.value = green;
	}

	public void ChangeGreen(float g)
	{
		//float green;
		//float.TryParse(g, out green);
		brushColor = new Color(brushColor.r, g, brushColor.b, brushColor.a);
		brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;
	}

	public void ChangeBlue(string b)
	{
		float blue;
		float.TryParse(b, out blue);
		brushColor = new Color(brushColor.b, brushColor.g, blue, brushColor.a);
		brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;

		bSlider.value = blue;
	}

	public void ChangeBlue(float b)
	{
		//float blue;
		//float.TryParse(b, out blue);
		brushColor = new Color(brushColor.r, brushColor.g, b, brushColor.a);
		brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;
	}
	public void ChangeAlpha(string a)
	{
		float alpha;
		float.TryParse(a, out alpha);
		brushColor = new Color(brushColor.r, brushColor.g, brushColor.b, alpha);
		GameObject brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;
	}
    /*DetermineCharacter
	* Description: converts the character value (bascially an int)
	* to where it is in the alphabet.png or alphabet2.png. Due to functionality
	* issues with some of the ascii values (NUL, carriage return, etc.) 
	* the alphabet.png and alphabet2.png do not follow ascii and if changed
	* must reflect in this function.
	* Parameter: char c, which character to pull from the alphabet.
	* Return: int, modified spot in ascii inside of the png. -1 is a 
	* character that does not currently exist in the png.
	*/
	
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

	public void SetCanLoad()
	{
		if(selected)
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
		
		
		

	}
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
		// Allow user to open a file explorer without loading a null image
		else
		{
			doneLoading = true;
		}
	}
	
	void SendTexture(Texture2D newPng)
	{
		this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
			{
				this.GetComponent<ASL.ASLObject>().SendAndSetTexture2D(newPng,
					recieveTexture, true);
				
			});
	}
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

	void sendClearCanvas()
	{
		if(selected)
		{
			float[] fArray = {1};
			this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
				{
					GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
				});
		}
		
	}
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
	

		
	

    //Methods for sending/receiving input
    void SendInput(InputInformation i)
    {
        var fArray = ConstructFloatsFromInput(i);
        this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
    }
    public void recieveInput(string id, float[] i)
	{
        
		InputInformation theInput = ConstructInputFromFloats(i);
        //Queues up the input to be built
        myQueue.Enqueue(theInput);
		
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
		else if(i.Length == 2) //Special case for allowing players access to the canvas
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
			}
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
