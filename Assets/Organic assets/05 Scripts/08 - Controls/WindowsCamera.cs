// Just add this script to your camera. It doesn't need any configuration.
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class WindowsCamera : MonoBehaviour
{



    public CubeManager cubeManager;
    public ExplorationMenu explorationMenu;
    public LeftJoystick leftJoystick;
    public CityButtons cityButtons;
    public GameObject characterMoving;
    public Transform skyBoxCameraTracker;
    private Camera cameraObject;


    // inputs
    public int touch;
    public float zoom;
    public Vector2[] touchPosition;
    public EventSystem eventSystem;


    // memory
    private Vector2 oldTouchPosition_0;
    private Vector2 oldTouchPosition_1;
    public Vector2 firstPosition;
    private float oldTouchDistance;
    private float newTouchDistance;

    // actions
    private Vector3 MoveCam;


    // calculations
    private float x_delta_translation;
    private float y_delta_translation;
    private float x_total_translation;
    private float y_total_translation;

    public bool HasTheTouchMoved;
    private bool Objectselected;
    private bool LiftingFinger;
    private bool back;
    public bool interactingOnUI;


    // zoom limits
    private readonly float minZoom = 1;
    private readonly float maxZoom = 5;

    // screen moving speed
    private readonly float HorizontalSpeedRatio = 2f;
    private readonly float VerticalSpeedRatio = 2f;

    // Platform tracker
    private bool isMobile;

    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) isMobile = true;

        cameraObject = GetComponent<Camera>();
        interactingOnUI = true;

    }


    void Update()
    {

        if (isMobile==true)  GetInputFromMobile(); else GetInputFromWindows();

        // if we are pointing to the ground on exploration mode, then we calculate the city naviguation logic
        if (cityButtons.MenuOpened == 1) CityNavigation();

    }

    void SelectObject(Vector2 target)
    {

        //Debug.Log("Checking where I click on y=" + target.y);
        
        if ((explorationMenu.explorationMenuOpened == true && target.y > 860)
            ||
            (explorationMenu.explorationMenuOpened == false && explorationMenu.selectionMenuOpened == true && target.y > 430)
            ||
            (explorationMenu.selectionMenuOpened == false && explorationMenu.selectionMenuOpened == false && target.y > 230)
            ) 
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(target);

            Vector3Int tpos = cubeManager.tileMapGround.WorldToCell(worldPoint);

            Debug.Log("cell at x:" + tpos.x + " , y:" + tpos.y + " , z:" + tpos.z);


            if (cubeManager.Visible[tpos.x, tpos.y] != 4)
            {
                Objectselected = true;
                explorationMenu.Select(tpos.x, tpos.y);
            }

        }

    }

    void DeselectCamera()
    {

        Debug.Log("Deselecting camera and removing exploration mode");
        explorationMenu.Deselect();
        Objectselected = false;
        if (explorationMenu.explorationMenuOpened == true)
        {
            explorationMenu.DesactivateSubMenu("exploration");
            cityButtons.ChangeColourButton(1, false);
        }
        
    }

    void CityNavigation() {


        //select only if i touched and didn't move until i lifted my finger
        if (HasTheTouchMoved == false && LiftingFinger == true && interactingOnUI == false) SelectObject(touchPosition[0]);

        //deselect if I start moving around and I am in the SelectionOnly Mode
        else if (HasTheTouchMoved == true && Objectselected == true && touch == 1 && interactingOnUI == false) DeselectCamera();

        // Tracking the character selected if moving with joystick (depreciated) or if in menu exploration
        if (explorationMenu.explorationMenuOpened == true)
        {
            if (characterMoving == null) characterMoving = cubeManager.playerInstantiated.gameObject;

            // Finding distance between character and camera
            float x_translation = characterMoving.transform.position.x - transform.position.x;
            float y_translation = characterMoving.transform.position.y - transform.position.y;

            //Moving Camera towards character
            transform.position = new Vector3(transform.position.x + x_translation * Time.deltaTime, transform.position.y + y_translation * Time.deltaTime, transform.position.z);

            // Moving the skybox to match
            skyBoxCameraTracker.position = new Vector3(0.95f * transform.position.x, 2.5f + 0.95f * transform.position.y, 0);
        }


        // Moving around with one finger on the screen
        if (touch == 1 && leftJoystick.moveDetectedOnJoystick<=1 && interactingOnUI == false)
        {
            // resetting the camera to free movement
            leftJoystick.moveDetectedOnJoystick = 0;

            // Calculating the movement needed
            MoveCam = transform.position + transform.TransformDirection(x_delta_translation, y_delta_translation, 0);

            // Setting the new position of the camera
            transform.position = new Vector3(
                    Mathf.Min(Mathf.Max(MoveCam.x, 0.25f * cubeManager.MinVisibleSizeOnX - 0.25f* cubeManager.MaxVisibleSizeOnY - 5), 0.25f * cubeManager.MaxVisibleSizeOnX - 0.25f * cubeManager.MinVisibleSizeOnY + 5), 
                    Mathf.Min(Mathf.Max(MoveCam.y, 0.25f * cubeManager.MinVisibleSizeOnX + 0.25f* cubeManager.MinVisibleSizeOnY - 2.5f), 0.25f * cubeManager.MaxVisibleSizeOnX + 0.25f * cubeManager.MaxVisibleSizeOnY + 2.5f), 
                    transform.position.z);

            // Moving the skybox to match
            skyBoxCameraTracker.position = new Vector3(0.95f * transform.position.x, 2.5f+0.95f * transform.position.y, 0);

            // Saving the latest position
            oldTouchPosition_0 = touchPosition[0];
        }

        // If joystick was released, then freeing up camera for next touch
        if (leftJoystick.moveDetectedOnJoystick == 2 && interactingOnUI == false) leftJoystick.moveDetectedOnJoystick = 1;

        // Zooming and dezooming
        if (zoom != 0)
        {
            float newZoom = Mathf.Max(minZoom, Mathf.Min(maxZoom, GetComponent<Camera>().orthographicSize - zoom));
            float sizeRatio = (newZoom - minZoom) / (maxZoom - minZoom);
            GetComponent<Camera>().orthographicSize = newZoom;
            skyBoxCameraTracker.localScale = new Vector3(1 + sizeRatio, 1 + sizeRatio, 1 + sizeRatio);
        }



    }

    void GetInputFromMobile() {

        touch = Input.touchCount;

        //Re-initialise settings when no more touch on the screen
        if (touch == 0)
        {
            oldTouchPosition_0 = Vector2.zero;
            oldTouchPosition_1 = Vector2.zero;
            firstPosition= Vector2.zero;
        }

        // When I sense one finger on the screen, i record its position on 0, forget if i had a 2 finger, and trace the historic point where I started touching the screen
        if (touch == 1)
        {
            touchPosition[0] = Input.GetTouch(0).position;
            Debug.Log("First click, and is old touchpoint zero: "+ oldTouchPosition_0.x+" and " + oldTouchPosition_0.y);

            if (oldTouchPosition_0 == Vector2.zero) 
            { 
                firstPosition= touchPosition[0]; 
                oldTouchPosition_0 = touchPosition[0];


                //Debug.Log("Casting a ray");
                PointerEventData pointerEventData = new(eventSystem)
                {
                    position = touchPosition[0]
                };

                List<RaycastResult> results = new();
                eventSystem.RaycastAll(pointerEventData, results);
                //Debug.Log("Casted a ray at x=" + touchPosition[0].x + " and y=" + touchPosition[0].y);

                //foreach (RaycastResult result in results)
                //{
                //    Debug.Log("Hit " + result.gameObject.name);
                //}


                if (results.Count >= 1) interactingOnUI = true;
                else interactingOnUI = false;

            }
            if (oldTouchPosition_1 != Vector2.zero) oldTouchPosition_1 = Vector2.zero;
            

        }

        // When I record two or more fingers on the screen, i record the first and second positions on 0 and 1.
        if (touch > 1)
        {
            touchPosition[0] = Input.GetTouch(0).position;
            touchPosition[1] = Input.GetTouch(1).position;

            if (oldTouchPosition_1 == Vector2.zero)
            {
                oldTouchPosition_0 = touchPosition[0];
                oldTouchPosition_1 = touchPosition[1];
            }

            // je met a jour les distances
            oldTouchDistance = (oldTouchPosition_0 - oldTouchPosition_1).magnitude;
            newTouchDistance = (touchPosition[0] - touchPosition[1]).magnitude;

            // Je get mon zoom
            zoom = GetComponent<Camera>().orthographicSize * (1 - (oldTouchDistance / newTouchDistance));

            // Je reset pour la prochaine fois
            oldTouchPosition_0 = touchPosition[0];
            oldTouchPosition_1 = touchPosition[1];
        }
        else zoom = 0;

        LiftingFinger = touch > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;

        back = Input.GetKeyDown(KeyCode.Escape);

        TouchMoved();

        

    }

    void GetInputFromWindows() {

        //Get the input
        if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            touch = 1;
            touchPosition[0] = Input.mousePosition;

            if (oldTouchPosition_0 == Vector2.zero) 
            { 
                oldTouchPosition_0 = touchPosition[0];
                firstPosition = touchPosition[0];

                //interactingOnUI = ;
                Debug.Log("Casting a ray");
                PointerEventData pointerEventData = new(eventSystem)
                {
                    position = touchPosition[0]
                };

                List<RaycastResult> results = new();
                eventSystem.RaycastAll(pointerEventData, results);
                Debug.Log("Casted a ray at x="+ touchPosition[0].x + " and y="+ touchPosition[0].y);

                foreach (RaycastResult result in results)
                {
                    Debug.Log("Hit " + result.gameObject.name);
                }


                if (results.Count>=1) interactingOnUI = true;
                else interactingOnUI = false;
            }

            if (oldTouchPosition_1 != Vector2.zero) oldTouchPosition_1 = Vector2.zero;
        }
        else
        {
            touch = 0;
            oldTouchPosition_0 = Vector2.zero;
            oldTouchPosition_1 = Vector2.zero;
            firstPosition = Vector2.zero;
            

        }

        zoom = Input.GetAxis("Mouse ScrollWheel");

        LiftingFinger = Input.GetMouseButtonUp(0);

        back = Input.GetMouseButtonUp(1);

        TouchMoved();


    }

    void TouchMoved() {

        x_delta_translation = (oldTouchPosition_0.x - touchPosition[0].x) * GetComponent<Camera>().orthographicSize / GetComponent<Camera>().pixelHeight * HorizontalSpeedRatio;
        y_delta_translation = (oldTouchPosition_0.y - touchPosition[0].y) * GetComponent<Camera>().orthographicSize / GetComponent<Camera>().pixelHeight * VerticalSpeedRatio;

        x_total_translation = (firstPosition.x - touchPosition[0].x) * GetComponent<Camera>().orthographicSize / GetComponent<Camera>().pixelHeight * HorizontalSpeedRatio;
        y_total_translation = (firstPosition.y - touchPosition[0].y) * GetComponent<Camera>().orthographicSize / GetComponent<Camera>().pixelHeight * VerticalSpeedRatio;

        HasTheTouchMoved = (x_total_translation + y_total_translation > 0.1) || (x_total_translation + y_total_translation < -0.1) ;
    }


}
