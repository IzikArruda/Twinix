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
         
        ChangeCurrentLine(startingLine);
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

    #endregion


    #region Moving Functions --------------------------------------------------------- */

    public void MovePlayerRequest(OrthogonalDirection dir1, OrthogonalDirection dir2, ref float distance) {
        /*
         * Move the player along their line in the given direction for up to the given distance or
         * they reach a corner with no linked line in the given direction.
         */
        OrthogonalDirection direction = OrthogonalDirection.NULL;

        /*
         * Keep moving the player along the given direction until they run out of distance or are blocked
         */
        bool blocked = false;
        while(distance > 0 && !blocked) {
            
            /* If the player is on a corner, change their current line relative to their direction */
            ChangeCurrentLine(dir1, dir2);

            /* Get the direction that is parallel to the current line */
            if(currentLine.IsHorizontal()) {
                direction = Line.ReturnHorizontalDirection(dir1, dir2);
            }
            else if(currentLine.IsVertical()) {
                direction = Line.ReturnVerticalDirection(dir1, dir2);
            }
            
            /* The given direction is parallel to the current line */
            if(LineCorner.HoriDirection(direction) && currentLine.IsHorizontal() ||
                    LineCorner.VertDirection(direction) && currentLine.IsVertical()) {

                /* Scan ahead from the player's position to see how far the player is allowed to travel */
                float travelDistance = 0;
                currentLine.PredeterminePlayerMovement(this, gamePosition, direction, ref travelDistance, distance, true, ref blocked);

                /* Move the player by the amount of distance to travel and update the remaining distance */
                MovePlayer(direction, travelDistance);
                distance -= travelDistance;
                
                
                /* We reached a corner if we still have distance to travel and are not blocked */
                if(distance > 0 && !blocked) {
                    
                    /* Check if either given directions will let the player pass the corner */
                    if(GetCorner().AttachedLineAt(dir1) != null || GetCorner().AttachedLineAt(dir2) != null) {
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

    private void MovePlayer(OrthogonalDirection direction, float distance) {
        /*
         * Move the player's game position along the given direction for the given distance
         */

        gamePosition += LineCorner.DirectionToVector(direction)*distance;
    }

    private void ChangeCurrentLine(OrthogonalDirection dir1, OrthogonalDirection dir2) {
        /*
         * If the player is on the corner of their current line, change their current line
         * to one of the lines connected to the corner they are currently on.
         * 
         * The line they will switch to is determined by the given inputs and the direction
         * of their current line (if they have given two inputs).
         * 
         * When on a corner and holding two sepperate axis as inputs, the player will swap
         * to the line perpendicular to their current line. This is shown in the example bellow:
         * 
         * When moving up towards a 4 way corner and holding up+right, the player, once reaching
         * the corner, will swap to the corner's right line. This is because if the player
         * wanted to not take the turn and continue upwards, they would have only held the up key.
         */
        LineCorner currentCorner = GetCorner();
        OrthogonalDirection primaryDirection = dir1;
        OrthogonalDirection secondairyDirection = dir2;

        /* Make sure we are on a corner first */
        if(currentCorner != null) {
            
            /* Properly order the inputs if we are given two unique directions */
            if(dir1 != OrthogonalDirection.NULL && dir2 != OrthogonalDirection.NULL) {
                /* Current line is horizontal. */
                if(currentLine.IsHorizontal()) {
                    /* If the primary direction is also horizontal... */
                    if(LineCorner.HoriDirection(primaryDirection)) {
                        /* ...Swap it so the primary direction is now vertical */
                        primaryDirection = dir2;
                        secondairyDirection = dir1;
                    }
                }

                /* Current line is vertical. */
                else if(currentLine.IsVertical()) {
                    /* If the primary direction is also vertical... */
                    if(LineCorner.VertDirection(primaryDirection)) {
                        /* ...Swap it so the primary direction is now horizontal */
                        primaryDirection = dir2;
                        secondairyDirection = dir1;
                    }
                }
            }


            /* Swap to the line at the primary direction */
            if(currentCorner.AttachedLineAt(primaryDirection) != null) {
                ChangeCurrentLine(currentCorner.AttachedLineAt(primaryDirection));
            }

            /* Swap to the line at the secondairy direction if the primary is not connected */
            else if(currentCorner.AttachedLineAt(secondairyDirection) != null) {
                ChangeCurrentLine(currentCorner.AttachedLineAt(secondairyDirection));
            }
        }
    }

    private void ChangeCurrentLine(Line newLine) {
        /*
         * Change the player's current line to the given line
         */

        if(newLine != null && newLine != currentLine) {
            
            /* Un-link the player from their previous line */
            if(currentLine != null) {
                currentLine.UnlinkPlayer(this);
            }

            /* Link the player to their new line */
            if(newLine != null) {
                newLine.LinkPlayer(this);
            }

            Debug.Log("CHANGED CURRENT LINE");
            currentLine = newLine;
        }
    }

    #endregion
    
}
