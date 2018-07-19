using UnityEngine;
using System.Collections;

/*
 * The player object which contains 
 */
public class Player {

    #region Variables  --------------------------------------------------------- */

    /* The controls used by this player */
    public PlayerControls controls;

    /* The player's position in the game area */
    public Vector3 gamePosition;

    /* The line the player is currently linked to */
    public Line currentLine;

    /* The gameObject that contains the player's sprite renderer */
    public GameObject playerGameObject;
    public SpriteRenderer spriteRenderer;

    /* How many pixels the player can move in 1 second */
    public float defaultMovementSpeed;

    #endregion


    #region Constructors --------------------------------------------------------- */

    public Player(GameObject playerContainer) {
        /*
         * Create the player and their required objects. Any gameObjects created will
         * be placed in the given playerContainer.
         */

        currentLine = null;
        controls = new PlayerControls();
        gamePosition = Vector3.zero;
        defaultMovementSpeed = 1;

        /* Create an object with a sprite renderer for the player */
        playerGameObject = new GameObject();
        playerGameObject.name = "Player";
        playerGameObject.transform.SetParent(playerContainer.transform);
        playerGameObject.transform.position = new Vector3(0, 0, 0);
        playerGameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        playerGameObject.transform.localScale = new Vector3(1, 1, 1);
        spriteRenderer = playerGameObject.AddComponent<SpriteRenderer>();
    }

    #endregion
    

    #region Setting Functions --------------------------------------------------------- */

    public void SetupControls(int playerNumber) {
        /*
         * Setup the controls of the player with a set of default controls
         */
        KeyCode[][] defaultKeyCodes = new KeyCode[2][];
        
        /* Set the default controls for the player's controls */
        defaultKeyCodes[0] = new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.S, KeyCode.A, KeyCode.Space };
        defaultKeyCodes[1] = new KeyCode[] { KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightControl };
        
        /* Set the default controls of the character if the given playerNumber has a set to use */
        if(playerNumber <= defaultKeyCodes.Length) {
            /* Assign the movement keys to the player */
            controls.SetMovementKeys(
                    defaultKeyCodes[playerNumber][0], 
                    defaultKeyCodes[playerNumber][1], 
                    defaultKeyCodes[playerNumber][2], 
                    defaultKeyCodes[playerNumber][3]);

            /* Assign the extra buttons to the player */
            for(int j = 4; j < defaultKeyCodes[playerNumber].Length; j++) {
                controls.SetExtraButtonKey(j-4, defaultKeyCodes[playerNumber][j]);
            }
        }
        else {
            Debug.Log("WARNING: Player " + playerNumber + " does not have default controls");
        }
    }

    public void SetupSprite(Sprite playerSprite) {
        /*
         * Set the sprite of the player to the given sprite
         */

        spriteRenderer.sprite = playerSprite;
    }

    public void SetStartingLine(Line startingLine, float distance) {
        /*
         * Setup the player's starting position onto the given line at the position
         * with the given amount of distance from it's starting point.
         * Distance ranges from [0, 1].
         */

        currentLine = startingLine;
        SetPlayerPosition(Vector3.Lerp(startingLine.start, startingLine.end, distance));
    }

    public void SetSpritesize(float spriteSize) {
        /*
         * Set the size of the player's sprite
         */

        playerGameObject.transform.localScale = new Vector3(spriteSize, spriteSize, spriteSize);
    }

    public void SetPlayerPosition(Vector3 playerPos) {
        /*
         * Place the player's position in the game by setting their gamePosition
         */

        gamePosition = playerPos;
    }

    public void SetSpritePositon(Vector3 spritePos) {
        /*
         * Set the position of the player's sprite by setting the player object's position
         */

        playerGameObject.transform.position = spritePos;
    }

    #endregion
}
