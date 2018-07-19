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
        defaultMovementSpeed = 5;

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
    

    #region Set Functions --------------------------------------------------------- */

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


    #region Get Functions --------------------------------------------------------- */

    public LineCorner GetCorner() {
        /*
         * Return the corner the player is currently positioned on.
         * Return null if they are not placed on a corner.
         */
        LineCorner currentCorner = null;

        if(gamePosition.Equals(currentLine.startCorner.position)) {
            currentCorner = currentLine.startCorner;
        }
        else if(gamePosition.Equals(currentLine.endCorner.position)) {
            currentCorner = currentLine.endCorner;
        }

        return currentCorner;
    }

    public OrthogonalDirection GetInputDirection() {
        /*
         * Return the direction the user has inputted
         */
        OrthogonalDirection inputtedDirection = OrthogonalDirection.NULL;

        if(controls.up) {
            inputtedDirection = OrthogonalDirection.Up;
        }
        else if(controls.right) {
            inputtedDirection = OrthogonalDirection.Right;
        }
        else if(controls.down) {
            inputtedDirection = OrthogonalDirection.Down;
        }
        else if(controls.left) {
            inputtedDirection = OrthogonalDirection.Left;
        }

        return inputtedDirection;
    }

    #endregion


    #region Moving Functions --------------------------------------------------------- */

    public void MovePlayerRequest(OrthogonalDirection direction, ref float distance) {
        /*
         * Move the player along their line in the given direction for up to the given distance or
         * they reach a corner with no linked line in the given direction.
         */
         
        /*
         * Keep moving the player along the given direction until they run out of distance or are blocked
         */
        bool blocked = false;
        while(distance > 0 && !blocked) {
            
            /* If the player is on a corner, change their current line realtive to their direction */
            ChangeCurrentLine(direction);

            /* The given direction is parallel to the current line */
            if(LineCorner.HoriDirection(direction) && currentLine.IsHorizontal() ||
                    LineCorner.VertDirection(direction) && currentLine.IsVertical()) {
                
                /* Move along the line in the given direction */
                MoveTowardsCorner(direction, ref distance);

                /* We reached the upcomming corner if we still have distance to travel */
                if(distance > 0) {

                    /* Check if we can pass the corner */
                    if(GetCorner().AttachedLineAt(direction) != null) {
                        //We can move onto the next line, ie repeat the loop
                    }
                    else {
                        //The corner ends here - the player cannot leave the line
                        blocked = true;
                        //Debug.Log("CANT LEAVE LINE");
                    }
                }
            }
            /* The given direction is perpendicular - the player cannot leave the line */
            else {
                //Cant leave the line
                blocked = true;
                //Debug.Log("CANT LEAVE LINE");
            }
        }
    }

    private void MoveTowardsCorner(OrthogonalDirection direction, ref float distance) {
        /*
         * Move the player towards the corner of their current line in the given direction.
         * Only travel for up to the given amount of distance along the line.
         */
        float toCornerDistance = Mathf.Min(currentLine.DistanceToCornerFrom(gamePosition, direction), distance);

        /* Move the player and reduce the remaining distance */
        if(toCornerDistance > 0) {
            MovePlayer(direction, toCornerDistance);
            distance -= toCornerDistance;
        }
    }

    private void MovePlayer(OrthogonalDirection direction, float distance) {
        /*
         * Move the player's game position along the given direction for the given distance
         */

        gamePosition += (Vector3) LineCorner.DirectionToVector(direction)*distance;
    }

    private void ChangeCurrentLine(OrthogonalDirection direction) {
        /*
         * If the player is on the corner of their current line, change their current line
         * to the line connected to said corner's given direction.
         */
        LineCorner currentCorner = GetCorner();

        /* Make sure we are on a corner first */
        if(currentCorner != null) {

            /* Get the line attached onto the given direction */
            Line newLine = currentCorner.AttachedLineAt(direction);

            /* Assign the new current line to the player if it exists */
            if(newLine != null && newLine != currentLine) {
                Debug.Log("CHANGED CURRENT LINE");
                currentLine = newLine;
            }
        }
    }

    #endregion
    
}
