using UnityEngine;
using System.Collections;

/*
 * Handles the controls for a player. It tracks the state of a set of buttons.
 * The directionnal keys always follow the order of: Up, Right, Down, Left.
 */
public class PlayerControls {

    #region Variables  --------------------------------------------------------- */

    private KeyCode[] directionKeys;
    private bool[] directionStates;
    private int[] directionHoldTime;
    private KeyCode[] buttonsKey;
    private bool[] buttonsStates;

    /* How many extra buttons the controller has */
    public static int extraButtonCount = 1;

    #endregion


    #region Constructors  --------------------------------------------------------- */

    public PlayerControls() {
        /*
         * Create the controller with default key values
         */

        /* Create the arrays that hold the state of the buttons and their held time */
        directionStates = new bool[4];
        directionHoldTime = new int[4];
        for(int i = 0; i < directionHoldTime.Length; i++) { directionHoldTime[i] = 0; }
        buttonsStates = new bool[extraButtonCount];
        for(int i = 0; i < buttonsStates.Length; i++) { buttonsStates[i] = false; }

        /* Create the arrays for the keycodes */
        directionKeys = new KeyCode[4];
        for(int i = 0; i < directionKeys.Length; i++) { directionKeys[i] = KeyCode.None; }
        buttonsKey = new KeyCode[extraButtonCount];
        for(int i = 0; i < buttonsKey.Length; i++) { buttonsKey[i] = KeyCode.None; }
    }

    #endregion


    #region Key Setting Functions  --------------------------------------------------------- */

    public void SetMovementKeys(KeyCode newUp, KeyCode newRight, KeyCode newDown, KeyCode newLeft) {
        /*
         * Set the current keycode for the movement keys to the given keycodes.
         * The order of the keys are always in: Up, Right, Down, Left.
         */

        directionKeys[0] = newUp;
        directionKeys[1] = newRight;
        directionKeys[2] = newDown;
        directionKeys[3] = newLeft;
    }

    public void SetExtraButtonKey(int index, KeyCode newButton) {
        /*
         * Set the given new keycode to the button defined by the given index.
         * If the index is out of range, print out an error and do not set anything.
         */

        if(index > -1 && index < extraButtonCount) {
            buttonsKey[index] = newButton;
        }
        else {
            Debug.Log("WARNING: Trying to set a keycode to a button out of range of the button array");
        }
    }

    #endregion


    #region Input Functions  --------------------------------------------------------- */

    public void UpdateInputs() {
        /*
         * Update the state of the booleans that track the key states
         */

        /* Update the state of each directionnal button */
        for(int i = 0; i < directionStates.Length; i++) {
            directionStates[i] = Input.GetKey(directionKeys[i]);

            /* Increment the button held counter for each key */
            if(directionStates[i]) {
                directionHoldTime[i]++;
            }
            else {
                directionHoldTime[i] = 0;
            }
        }
        
        /* Update the state of each extra button */
        for(int i = 0; i < extraButtonCount; i++) {
            buttonsStates[i] = Input.GetKey(buttonsKey[i]);
        }
    }

    public OrthogonalDirection GetPrimaryInput() {
        /*
         * Return the OrthogonalDirection of the longest inputted directional key 
         * that has been held down for the longest.
         */
        OrthogonalDirection direction = OrthogonalDirection.NULL;
        int longestTime = 0;

        for(int i = 0; i < directionHoldTime.Length; i++) {
            if(longestTime < directionHoldTime[i]) {
                longestTime = directionHoldTime[i];
                
                if(i == 0) {
                    direction = OrthogonalDirection.Up;
                }
                else if(i == 1) {
                    direction = OrthogonalDirection.Right;
                }
                else if(i == 2) {
                    direction = OrthogonalDirection.Down;
                }
                else if(i == 3) {
                    direction = OrthogonalDirection.Left;
                }
            }
        }

        return direction;
    }

    public OrthogonalDirection GetSecondairyInput() {
        /*
         * Return the OrthogonalDirection of the second longest held directional
         * key that is not parallel to the primary inputted key.
         */
        OrthogonalDirection primaryInput = GetPrimaryInput();
        OrthogonalDirection secondairyInput = OrthogonalDirection.NULL;
        
        /* Check whether the right or left inputs are pressed/which one for longer */
        if(primaryInput == OrthogonalDirection.Up || primaryInput == OrthogonalDirection.Down) {
            if(directionHoldTime[1] != 0 || directionHoldTime[3] != 0) {
                if(directionHoldTime[1] >= directionHoldTime[3]) {
                    secondairyInput = OrthogonalDirection.Right;
                }else {
                    secondairyInput = OrthogonalDirection.Left;
                }
            }
        }

        /* Check whether the top or down inputs are pressed/which one for longer */
        else if(primaryInput == OrthogonalDirection.Right || primaryInput == OrthogonalDirection.Left) {
            if(directionHoldTime[0] != 0 || directionHoldTime[2] != 0) {
                if(directionHoldTime[0] >= directionHoldTime[2]) {
                    secondairyInput = OrthogonalDirection.Up;
                }
                else {
                    secondairyInput = OrthogonalDirection.Down;
                }
            }
        }
        
        return secondairyInput;
    }

    public bool GetButtonInput(int buttonIndex) {
        /*
         * Given a button index, return whether the button is pressed or not.
         */
        bool buttonState = false;

        if(buttonIndex >= extraButtonCount || buttonIndex < 0) {
            Debug.Log("WARNING: Looking for input of a button out of button index");
        }
        else {
            buttonState = buttonsStates[buttonIndex];
        }

        return buttonState;
    }

    #endregion
    
}
