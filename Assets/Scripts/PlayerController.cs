using UnityEngine;
using System.Collections;

/*
 * A controller for a player. It tracks the state of a set of buttons 
 * used to control the player.
 */
public class PlayerController {

    #region Variables  --------------------------------------------------------- */

    public bool up;
    public bool right;
    public bool down;
    public bool left;
    public bool[] buttons;
    public KeyCode upKey;
    public KeyCode rightKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode[] buttonsKey;

    /* How many extra buttons the controller has */
    public static int extraButtonCount = 1;

    #endregion


    #region Player Controller Creation Functions  --------------------------------------------------------- */

    public PlayerController() {
        /*
         * Create the playerController with default key values
         */

        up = false;
        right = false;
        down = false;
        left = false;
        buttons = new bool[extraButtonCount];
        for(int i = 0; i < buttons.Length; i++) { buttons[i] = false; }
        upKey = KeyCode.None;
        rightKey = KeyCode.None;
        downKey = KeyCode.None;
        leftKey = KeyCode.None;
        buttonsKey = new KeyCode[extraButtonCount];
        for(int i = 0; i < buttonsKey.Length; i++) { buttonsKey[i] = KeyCode.None; }
    }

    #endregion


    #region Key Setting Functions  --------------------------------------------------------- */

    public void SetMovementKeys(KeyCode newUp, KeyCode newRight, KeyCode newDown, KeyCode newLeft) {
        /*
         * Set the current keycode for the movement keys to the given keycodes
         */

        upKey = newUp;
        rightKey = newRight;
        downKey = newDown;
        leftKey = newLeft;
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

        up = Input.GetKey(upKey);
        right = Input.GetKey(rightKey);
        down = Input.GetKey(downKey);
        left = Input.GetKey(leftKey);

        for(int i = 0; i < extraButtonCount; i++) {
            buttons[i] = Input.GetKey(buttonsKey[i]);
        }
    }

    #endregion
}
