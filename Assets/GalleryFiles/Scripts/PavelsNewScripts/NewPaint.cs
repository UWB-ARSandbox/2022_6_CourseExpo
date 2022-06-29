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

public class NewPaint : MonoBehaviour
{

    struct InputInformation //: IEquatable<InputInformation>, IComparable<InputInformation>
	{


		//public int peerID;
		public Vector2 canvasClick;

		public bool mouseDown;
		public bool previousMouseDown;
		public Vector2 previousCanvasClick;

		public Color32 brushColor;
		public int brushSize;

		public bool eraseMode;
		public bool textMode;
		public bool lineMode;

		public string textInput;
		/*
		public override bool Equals(object other)
        {
			if (other == null || this.GetType() != other.GetType()) 
			{
				return false;
			}
            return this.peerID == ((InputInformation)other).peerID;
        }
		public override int GetHashCode()
        {
            return this.GetHashCode();
        }
		public bool Equals(InputInformation other)
        {
            return this.Equals((object)other);
        }
		public int CompareTo(InputInformation other)
        {
            if(peerID > (other.peerID))
			{
				return 1;
			}
			else if (peerID < (other.peerID))
			{
				return -1;
			}
			else 
			{
				return 0;
			}

            // Code that compares two variables
        }
		*/
		
	}

    Queue<InputInformation> myQueue;

	//the canvas of the students
	Texture2D studentCanvas;

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

    //string for typed text
    string textOnType;

	//what color the user wants to paint with
	Color brushColor;

	//alphabet of characters
	Texture2D alphabet;
	Texture2D alphabet2;
	Texture2D alphabetUsed;

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

    // Start is called before the first frame update
    void Start()
    {

        
        GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(recieveInput);
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
		textOnType = "";
		brushColor = Color.black;
		pixelToDraw = new Vector2(0, 0);
        previousCoord = new Vector2(0, 0);

        alphabet = Resources.Load("alphabet", typeof(Texture2D)) as Texture2D;
		alphabet2 = Resources.Load("alphabet2", typeof(Texture2D)) as Texture2D;

        alphabetUsed = alphabet;

        GameObject brushColorUI = GameObject.Find("BrushColor");
		brushColorUI.GetComponent<Image>().color = brushColor;


        //Texture stuff
        studentCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
		maskCanvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);

        for (int x = 0; x < canvasWidth; x++)
		{
			for (int y = 0; y < canvasHeight; y++)
			{
				studentCanvas.SetPixel(x, y, Color.white);
				maskCanvas.SetPixel(x, y, Color.clear);

			}
		}
		maskCanvas.Apply();

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


        

        StartCoroutine(UpdateCanvas());
    }

    // Update is called once per frame
    void Update()
    {
        // Makes sure that the paint brush mask is not applied every frame
        if (maskClock <= 0)
		{
			RaycastHit raycastHit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			int layerMask = 1 << 30;
			if (clicked)
			{
				layerMask = (1 << LayerMask.NameToLayer("DoNotRenderCanavas")) |
				(1 << LayerMask.NameToLayer("DoNotRenderTCan"));

				// Set canvas to render to screen
				layerMask |= (1 << 10);
				// Set canvas to render to screen
				layerMask |= (1 << 11);
			}
			layerMask = ~layerMask;

			// If mouse is not over the canvas and canvas is rendered, do not draw mask
			if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerMask) == true
			&& raycastHit.transform.GetComponent<NewPaint>() != null)
			{
				UpdateMask();
				maskClock = 0.1f;
			}
		}

        //Get inputs to send over for the canvas
        if (!EventSystem.current.IsPointerOverGameObject())
		{
			if (Input.GetMouseButton(0) == true)
			{
                
				RaycastHit raycastHit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


				int layerMask = 1 << 30;
				if (clicked)
				{
					layerMask = (1 << LayerMask.NameToLayer("DoNotRenderCanavas")) |
					(1 << LayerMask.NameToLayer("DoNotRenderTCan"));

					// Set canvas to render to screen
					layerMask |= (1 << 10);
					// Set canvas to render to screen
					layerMask |= (1 << 11);
				}
				layerMask = ~layerMask;

				if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerMask) == true
					&& raycastHit.transform.GetComponent<NewPaint>() != null)
				{
					Vector2 uv = raycastHit.textureCoord;
					
					Vector2 pixelCoord = new Vector2((int)(uv.x * (float)(canvasWidth)), (int)(uv.y * (float)(canvasHeight)));

					
					
					inputInfo.canvasClick = pixelCoord;
					inputInfo.textMode = this.textMode;
					inputInfo.eraseMode = this.eraseMode;
					inputInfo.lineMode = this.lineMode;
					inputInfo.previousCanvasClick = this.previousCoord;
					inputInfo.previousMouseDown = this.previousMouseDown;
                    inputInfo.brushColor = this.brushColor;
                    inputInfo.textInput = "";
                    inputInfo.brushSize = this.brushSize;

                    previousMouseDown = true;
                    previousCoord = pixelCoord;
                    SendInput(inputInfo);
                }
            }
        }
        if(Input.GetMouseButtonUp(0) == true)
        {
            previousMouseDown = false;
            
        }

    }
    IEnumerator UpdateCanvas()
	{
        Debug.Log("begin looping");
		while(true)
		{
            
			while(myQueue.Count != 0)
			{
                
				InputInformation inp = myQueue.Dequeue();
				if(inp.textMode)
				{

				}
				else if(inp.eraseMode)
				{

				}
				else if(inp.lineMode)
				{

				}
				else{
                    Debug.Log("drawing");
					Draw(inp);
				}

			}
            yield return null;
		}
	}
    void Draw(InputInformation inp)
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
                        
                        studentCanvas.SetPixel(x, y, inp.brushColor);
                        
                        
                    }
                }
                
            }
            else{
                int x = (int)inp.canvasClick.x;
                int y = (int)inp.canvasClick.y;
                studentCanvas.SetPixel(x, y, inp.brushColor);
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
                            
                            studentCanvas.SetPixel(x, y, inp.brushColor);
                            
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
                    
                    studentCanvas.SetPixel((int)(inp.canvasClick.x - distanceX), (int)(inp.canvasClick.y - distanceY), inp.brushColor);
                    
                    
                }
                
                
            }
            studentCanvas.Apply();
        }
	}
    void UpdateMask()
    {

    }
    void drawMask()
    {

    }
    void textMask()
    {

    }
    void eraseMask()
    {

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
	public void ChangeTextSize(int option)
	{
		if (option == 0)
		{
			textWidth = 7;
			textHeight = 14;
			alphabetUsed = alphabet;
		}
		else if (option == 1)
		{
			textWidth = 12;
			textHeight = 28;
			alphabetUsed = alphabet2;
		}
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
        Debug.Log("recieving");
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
            i.textInput.Length
        };
        inputFloats.AddRange(stringToFloats(i.textInput));

            
        return inputFloats.ToArray();
        

    }
    InputInformation ConstructInputFromFloats(float[] i)
    {
        InputInformation inputInfoNew = new InputInformation();
        inputInfoNew.canvasClick = new Vector2(i[0], i[1]);
        inputInfoNew.mouseDown = System.Convert.ToBoolean(i[2]);
        inputInfoNew.previousCanvasClick = new Vector2(i[3], i[4]);
        inputInfoNew.previousMouseDown = System.Convert.ToBoolean(i[5]);
        inputInfoNew.brushColor = new Color32((byte)i[6], (byte)i[7], (byte)i[8], 1);
        inputInfoNew.brushSize = (int)i[9];
        inputInfoNew.eraseMode = System.Convert.ToBoolean(i[10]);
        inputInfoNew.lineMode = System.Convert.ToBoolean(i[11]);
        inputInfoNew.textMode = System.Convert.ToBoolean(i[12]);

        string text = "";
        for(int j = 14; j < i[13]; j++)
        {
            text += i[j];
        }

        inputInfoNew.textInput = text;

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
