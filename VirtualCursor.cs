/**
 * $File: VirtualCursor.cs $
 * $Date: 2019/12/23 $
 * $Creator: Louis G $
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualCursor : MonoBehaviour
{
    #region variable field

    /// <summary>
    /// The cursor's image
    /// </summary>
    [SerializeField]
    private Texture cursorImage = null;

    /// <summary>
    /// The cursor's speed
    /// </summary>
    [SerializeField]
    private float cursorSpeed = 300f;

    /// <summary>
    /// The cursor's position
    /// </summary>
    private Vector2 cursorPos = Vector2.zero;

    /// <summary>
    /// In normally, every canvas have this componet
    /// and in my project there may have many canvas in one scene
    /// It means were do many time to check UI click event
    /// Also the value were changed when scene change
    /// </summary>
    [SerializeField]
    private List<GraphicRaycaster> m_Raycaster = new List<GraphicRaycaster>();

    /// <summary>
    /// The pointerEventData for virtualCursor
    /// Will put the cursor's position to check
    /// UI click state with GraphicRaycaster
    /// </summary>
    private PointerEventData m_PointerEventData;

    /// <summary>
    /// The mouse's position in last frame
    /// </summary>
    private Vector3 lastMousePos = Vector3.zero;

    /// <summary>
    /// The slider who is cursor focus
    /// </summary>
    private Slider focusSlider = null;

    /// <summary>
    /// The horizon move value by player control
    /// </summary>
    private float moveValueH = 0f;

    /// <summary>
    /// The vertical move value by player control
    /// </summary>
    private float moveValueV = 0f;

    /// <summary>
    /// The mouse's current position
    /// </summary>
    private Vector3 currentMosuePos = Vector3.zero;

    /// <summary>
    /// The cursor move range
    /// </summary>
    private Vector2 cursorMoveRange = Vector2.zero;

    /// <summary>
    /// The cursor image's size 
    /// </summary>
    private Vector2 cursorSize = Vector2.zero;

    /// <summary>
    /// The Ui canvas reference resolution
    /// </summary>
    [SerializeField]
    private Vector2 referenceResolution = new Vector2(2720, 1440);

    /// <summary>
    /// When user screen resolution different to referenceResolution
    /// The game screen will auto scale
    /// This value used to limit the cursor Y move range
    /// </summary>
    private float autoScaleScreenHeight = 0f;

    /// <summary>
    /// The black outter total y value
    /// Tere will appear when user's screen resolution different 
    /// to referenceResolution
    /// </summary>
    private float outScreenValue = 0f;

    static public Vector3 virtualMousePos = Vector3.zero;

    /// <summary>
    /// Check vursor click UI or not
    /// If true, it means the Physics RayCast shouldn't effect
    /// However Physics RayCast effect or not is judge by Designer
    /// </summary>
    static public bool uiClick = false;

    /// <summary>
    /// The button which cursors is move in
    /// </summary>
    private Button cursorsInButton = null;

    private Vector2 lastCursorPos = Vector2.zero;

    [SerializeField]
    private float maxAccelScale = 0.5f;

    private float accelMaxSpeed = 0f;

    [SerializeField]
    private float accelSpeedPerFrame = 5f;

    private float accelSpeed = 0;

    private float lastHorizontalAxis = 0f;

    private float lastVerticalAxis = 0f;

    static public bool cursorMoving = false;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Disable the true cursor/mouse
        Cursor.visible = false;

        // Set cursor's start position, i choose center in screen
        cursorPos.x = Screen.width * 0.5f;
        cursorPos.y = Screen.height * 0.5f;

        // Set cursor last position
        lastCursorPos = cursorPos;

        // Set mouse last position
        lastMousePos = Input.mousePosition;

        /*
         * Set crusro move range, i choose the screen's width and height to be
         * But i already do auto scale screen when in different screen size
         * So the cursor ,ove range y must to be my reference height
         */
        cursorMoveRange.x = Screen.width;
        cursorMoveRange.y = Screen.height;

        autoScaleScreenHeight = referenceResolution.y * (Screen.width / referenceResolution.x);
        outScreenValue = (Screen.height - autoScaleScreenHeight) * 0.5f;
        // Set cursor image's size
        cursorSize.x = cursorImage.width * 4;
        cursorSize.y = cursorImage.height * 4;
        //Debug.Log(Screen.width + "  " + Screen.height);
        accelMaxSpeed = cursorSpeed * maxAccelScale;

        /*
         * Because of our referenceResolution are 2720&1440,
         * so when user device's screen size different referenceResolution
         * we need to change cursor's basic speed, size and accel speed
         */

        float sizeScale = (1 - (referenceResolution.x 
            - Screen.width) / referenceResolution.x);

        cursorSpeed *= sizeScale;
        accelMaxSpeed *= sizeScale;
        accelSpeedPerFrame *= sizeScale;

        cursorSize.x = (int)(cursorSize.x * sizeScale);
        cursorSize.y = (int)(cursorSize.y * sizeScale);

    }

    // Update is called once per frame
    void Update()
    {
        CursorMove();
    }

    private void CursorMove()
    {

        // Check the cursor is controlled by mouse or other thing

        currentMosuePos = Input.mousePosition;

        if (currentMosuePos != lastMousePos)
        {
            /*
             * If current mouse position is not equal to lastMousePos
             * it means the player are using mouse now
             */

            // Set the cursor position 
            cursorPos = new Vector2
                (currentMosuePos.x, Screen.height - currentMosuePos.y);

            // Set last mouse position
            lastMousePos = currentMosuePos;
        }
        else
        {

            float hSpeed = cursorSpeed;
            float vSpeed = cursorSpeed;
            float currentHorizontalAxis = Input.GetAxis("Horizontal");
            float currentVerticalAxis = Input.GetAxis("Vertical");

            #region Speed accel
            bool hStopMove = true;
            bool vStopMove = true;

            if (currentHorizontalAxis >= lastHorizontalAxis 
                || Mathf.Abs(currentHorizontalAxis) != 0)
            {
                hStopMove = false;
            }
            else
            {
                hSpeed = 0f;
            }

            if (currentVerticalAxis >= lastVerticalAxis 
                || Mathf.Abs(currentVerticalAxis) != 0)
            {
                vStopMove = false;
            }
            else
            {
                vSpeed = 0f;
            }

            if (hStopMove && vStopMove)
            {
                accelSpeed = 0;
            }
            else
            {
                accelSpeed += accelSpeedPerFrame;
                if (accelSpeed >= accelMaxSpeed)
                {
                    accelSpeed = accelMaxSpeed;
                }
                
                if (!hStopMove) { hSpeed += accelSpeed; }
                if (!vStopMove) { vSpeed += accelSpeed; }
            }
            #endregion

           // Debug.Log(currentHorizontalAxis + "  "+ currentVerticalAxis);

            // Cursor is controlled by gamePad or joyStick
            moveValueH = hSpeed * currentHorizontalAxis * Time.deltaTime;
            moveValueV = vSpeed * currentVerticalAxis * Time.deltaTime;

            lastHorizontalAxis = currentHorizontalAxis;
            lastVerticalAxis = currentVerticalAxis;
        }

        cursorPos.x += moveValueH;
        /*
         * Because GUI's y forward value are reverse to Unity's
         * So y value change are reverse to x value
         */
        cursorPos.y -= moveValueV;

        /*
         * Cursor move range limit set
         * Should know the cursor not just a point,
         * it was a rectangle, so when change the limit range
         * should count cursor image's width and height in.
         */
        if (cursorPos.x >= (cursorMoveRange.x - cursorSize.x))
        {
            cursorPos.x = (cursorMoveRange.x - cursorSize.x);
        }
        else if (cursorPos.x <= 0)
        {
            cursorPos.x = 0;
        }

        if (cursorPos.y >= (cursorMoveRange.y - cursorSize.y))
        {
            cursorPos.y = (cursorMoveRange.y - cursorSize.y);
        }
        else if (cursorPos.y <= 0)
        {
            cursorPos.y = 0;
        }

        //if (cursorPos.y >= (cursorMoveRange.y - cursorSize.y) - outScreenValue 
        //    - (float)(cursorSize.y * (Screen.width / referenceResolution.x)))
        //{
        //    cursorPos.y = (cursorMoveRange.y - cursorSize.y) - outScreenValue 
        //        - (float)(cursorSize.y * (Screen.width / referenceResolution.x));
        //}
        //else if (cursorPos.y <= outScreenValue 
        //    + (float)(cursorSize.y * (Screen.width / referenceResolution.x)))
        //{
        //    cursorPos.y = outScreenValue 
        //        + (float)(cursorSize.y * (Screen.width / referenceResolution.x));
        //}

        CursorEvent();

        virtualMousePos = new Vector3(cursorPos.x, Screen.height - cursorPos.y, 0);

        if (lastCursorPos != cursorPos)
        {
            cursorMoving = true;
        }
        else
        {
            cursorMoving = false;
        }

        lastCursorPos = cursorPos;
    }

    /// <summary>
    /// Check the cursor event like on clik down, up and move...etc
    /// </summary>
    private void CursorEvent()
    {
        uiClick = false;

        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(null);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position =
            new Vector2(cursorPos.x, cursorMoveRange.y - cursorPos.y);

        // Mouse click down event
        if (Input.GetKeyDown(KeyCode.Space) || 
            Input.GetKeyDown(KeyCode.Joystick1Button0) || 
            Input.GetMouseButtonDown(0))
        {
            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            bool uiTrigger = false;

            for (int index = 0; index < m_Raycaster.Count; index++)
            {
                if (uiTrigger) { break; }
                m_Raycaster[index].Raycast(m_PointerEventData, results);
                if (results.Count != 0)
                {
                    //Debug.Log(results[0].gameObject.name);
                    // Button check
                    Button btn = results[0].gameObject.GetComponent<Button>();
                    if (btn != null)
                    {
                        //Debug.Log(btn.name);
                        // Trigger on click event
                        if (btn.interactable)
                        {
                            btn.onClick.Invoke();
                        }
                    }

                    Slider slider = results[0].gameObject.GetComponent<Slider>();
                    if (slider != null && focusSlider == null)
                    {
                        focusSlider = slider;
                    }

                    uiTrigger = true;
                    uiClick = true;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space) ||
            Input.GetKeyUp(KeyCode.Joystick1Button0) ||
            Input.GetMouseButtonUp(0))  // // Mouse click up event
        {
            /*
            * Cancel the focusSlider and set handle state to normal
            */
            if (focusSlider != null)
            {
                switch (focusSlider.transition)
                {
                    case Selectable.Transition.Animation:
                        break;
                    case Selectable.Transition.ColorTint:
                        break;
                    case Selectable.Transition.SpriteSwap:
                        focusSlider.handleRect.GetComponent<Image>().sprite
                            = focusSlider.spriteState.disabledSprite;
                        break;
                }

                focusSlider = null;
            }
        }
        else if (Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.Joystick1Button0) ||
            Input.GetMouseButton(0)) // Mouse press and  move
        {
            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            bool uiTrigger = false;

            for (int index = 0; index < m_Raycaster.Count; index++)
            {
                if (uiTrigger) { break; }
                m_Raycaster[index].Raycast(m_PointerEventData, results);

                /*
                 * When focusSlider not a null
                 * then cursor move can change focusSlider's value
                 * when cursor move in every frame
                 * 
                 * Why sliderTotalRange have magic number "0.5F"?
                 * yeah...I don't know why use this can solve my problem...fuck
                 */
                if (focusSlider != null)
                {
                    float sliderTotalRange = 0;
                    Vector2 sliderScreenPos =
                        Camera.main.WorldToScreenPoint(focusSlider.transform.position);
                    float sliderStart = 0;
                    float newValue = 0;
                    RectTransform _sliderRect = focusSlider.GetComponent<RectTransform>();

                   // Debug.Log(sliderScreenPos.x);
                    switch (focusSlider.direction)
                    {
                        case Slider.Direction.BottomToTop:
                            sliderTotalRange = (_sliderRect.rect.height / referenceResolution.y) * Screen.height;
                            sliderStart = sliderScreenPos.y - (sliderTotalRange * 0.5f);
                            if (cursorPos.y >= (sliderStart + sliderTotalRange))
                            {
                                newValue = 1f;
                            }
                            else if (cursorPos.y <= sliderStart)
                            {
                                newValue = 0f;
                            }
                            else
                            {
                                newValue = Mathf.Abs(cursorPos.y - sliderStart)
                                    / sliderTotalRange;
                            }
                            break;
                        case Slider.Direction.LeftToRight:
                            sliderTotalRange = (_sliderRect.rect.width / referenceResolution.x) * Screen.width;
                            sliderStart = sliderScreenPos.x - (sliderTotalRange * 0.5f);

                            if (cursorPos.x >= (sliderStart + sliderTotalRange))
                            {
                                newValue = 1f;
                            }
                            else if (cursorPos.x <= sliderStart)
                            {
                                newValue = 0f;
                            }
                            else
                            {
                                newValue = Mathf.Abs(cursorPos.x - sliderStart)
                                    / sliderTotalRange;
                            }
                            break;
                        case Slider.Direction.RightToLeft:
                            sliderTotalRange = (_sliderRect.rect.width / referenceResolution.x) * Screen.width;
                            sliderStart = sliderScreenPos.x + (sliderTotalRange * 0.5f);
                            if (cursorPos.x <= (sliderStart - sliderTotalRange))
                            {
                                newValue = 1f;
                            }
                            else if (cursorPos.x >= sliderStart)
                            {
                                newValue = 0f;
                            }
                            else
                            {
                                newValue = Mathf.Abs(cursorPos.x - sliderStart)
                                    / sliderTotalRange;
                            }
                            break;
                        case Slider.Direction.TopToBottom:
                            sliderTotalRange = (_sliderRect.rect.height / referenceResolution.y) * Screen.height;
                            sliderStart = sliderScreenPos.y + (sliderTotalRange * 0.5f);
                            if (cursorPos.y <= (sliderStart - sliderTotalRange))
                            {
                                newValue = 1f;
                            }
                            else if (cursorPos.y >= sliderStart)
                            {
                                newValue = 0f;
                            }
                            else
                            {
                                newValue = Mathf.Abs(cursorPos.y - sliderStart)
                                    / sliderTotalRange;
                            }
                            break;
                    }
                    focusSlider.value = newValue;

                    switch (focusSlider.transition)
                    {
                        case Selectable.Transition.Animation:
                            break;
                        case Selectable.Transition.ColorTint:
                            break;
                        case Selectable.Transition.SpriteSwap:
                            focusSlider.handleRect.GetComponent<Image>().sprite
                                = focusSlider.spriteState.pressedSprite;
                            break;
                    }
                }

                if (results.Count != 0)
                {
                    uiTrigger = true;
                }
            }
        }

        // Check cursor in UI(button, image... etc)
        {
            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            for (int index = 0; index < m_Raycaster.Count; index++)
            {
                m_Raycaster[index].Raycast(m_PointerEventData, results);

                if (cursorsInButton != null)
                {
                    switch(cursorsInButton.transition)
                    {
                        case Selectable.Transition.Animation:
                            break;
                        case Selectable.Transition.ColorTint:
                            break;
                        case Selectable.Transition.SpriteSwap:
                            cursorsInButton.image.sprite = cursorsInButton.spriteState.disabledSprite;
                            break;
                    }
                    cursorsInButton = null;
                }

                if (results.Count != 0)
                {
                    Button button = results[0].gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        switch (button.transition)
                        {
                            case Selectable.Transition.Animation:

                                break;
                            case Selectable.Transition.ColorTint:
                                break;
                            case Selectable.Transition.SpriteSwap:
                                button.image.sprite = button.spriteState.pressedSprite;
                                break;
                        }

                        cursorsInButton = button;
                    }
                }
            }
        }
    }

    public void SetGraphicRaycaster(List<GraphicRaycaster> graphicRaycasters, GraphicRaycaster[] sysGraphicRaycaster)
    {
        m_Raycaster.Clear();
        for (int index = 0; index < sysGraphicRaycaster.Length; index++)
        {
            m_Raycaster.Add(sysGraphicRaycaster[index]);
            //Debug.Log(sysGraphicRaycaster[index].transform.name);
        }

        for (int index = 0; index < graphicRaycasters.Count; index++)
        {
            m_Raycaster.Add(graphicRaycasters[index]);
            //Debug.Log(graphicRaycasters[index].transform.name);
        }
    }

    private void OnGUI()
    {
        // Draw the cursor
        GUI.DrawTexture(new Rect(cursorPos.x, cursorPos.y, 
            cursorSize.x, cursorSize.y), cursorImage);
    }
}
