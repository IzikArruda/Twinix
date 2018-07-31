using UnityEngine;
using System.Collections;


/*
 * Potential states a player can be in during gameplay
 */
public enum PlayerStates {
    NULL,
    Travelling,
    PreDrawing,
    Drawing
};

/*
 * The player object which contains 
 */
public class Player {

    #region Variables  --------------------------------------------------------- */

    /* The gameController that created this player */
    public GameController gameController;

    /* The current state of the player */
    public PlayerStates state;

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

    public Player(GameObject playerContainer, GameController parentGameController) {
        /*
         * Create the player and their required objects. Any gameObjects created will
         * be placed in the given playerContainer.
         */

        gameController = parentGameController;
        state = PlayerStates.Travelling;
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

    public Corner GetCorner() {
        /*
         * Return the corner the player is currently positioned on.
         * Return null if they are not placed on a corner.
         */
        Corner currentCorner = null;

        if(gamePosition.Equals(currentLine.startCorner.position)) {
            currentCorner = currentLine.startCorner;
        }
        else if(gamePosition.Equals(currentLine.endCorner.position)) {
            currentCorner = currentLine.endCorner;
        }

        return currentCorner;
    }

    #endregion


    #region Input Functions --------------------------------------------------------- */

    public void UpdateInputs() {
        /*
         * Update the inputs of the player. This includes changing the state from 
         * travelling to pre-drawing if the player has pressed a button.
         */

        /* Update the directional and button inputs for this state */
        controls.UpdateInputs();

        /* Pressing button0 while travelling will put the player into the pre-drawing state */
        if(state == PlayerStates.Travelling && controls.GetButtonInput(0)) {
            state = PlayerStates.PreDrawing;
        }

        /* Not holding button0 while pre-drawing will return the player to the travelling state */
        else if(state == PlayerStates.PreDrawing && !controls.GetButtonInput(0)) {
            state = PlayerStates.Travelling;
        }
    }

    #endregion

    
    #region Moving Functions --------------------------------------------------------- */

    public void MovePlayerRequest(OrthogonalDirection primary, OrthogonalDirection secondairy, ref float distance) {
        /*
         * Move the player along their line. How they are moved is determined by the player's
         * state, the inputs they are using and their current line.
         * 
         * 
         */
        OrthogonalDirection direction = OrthogonalDirection.NULL;

        /*
         * Keep moving the player along the given direction until they run out of distance or are blocked
         */
        bool blocked = false;
        while(distance > 0 && !blocked) {
            
            /* Get the direction the player will move in relative to their state */
            direction = UpdatePlayerInputDirection(state, primary, secondairy);
            
            if(direction != OrthogonalDirection.NULL) {

                /*
                 * Travel in the given direction for up to the given distance or until 
                 * the player reaches a corner with no linked line in the given direction.
                 */
                if(state == PlayerStates.Travelling) {

                    /* The given direction is parallel to the current line */
                    if(currentLine.IsDirectionParallel(direction)) {

                        /* Scan ahead from the player's position to see how far the player is allowed to travel */
                        float travelDistance = 0;
                        currentLine.PredeterminePlayerMovement(this, gamePosition, direction, ref travelDistance, distance, true, ref blocked);
                        //Debug.Log("moving towards " + direction + " " + travelDistance);

                        /* Move the player by the amount of distance to travel and update the remaining distance */
                        MovePlayer(direction, travelDistance);
                        distance -= travelDistance;

                        /* We reached a corner if we still have distance to travel and are not blocked */
                        if(distance > 0 && !blocked) {

                            /* Check if either given directions will let the player pass the corner */
                            if(GetCorner().AttachedLineAt(primary) != null || GetCorner().AttachedLineAt(secondairy) != null) {
                                //We can move onto the next line, ie repeat the loop
                            }
                            else {
                                //The corner ends here - the player cannot leave the line
                                blocked = true;
                                Debug.Log("CANT LEAVE LINE");
                            }
                        }
                    }

                    /* The given direction does not follow the line - stop the player from moving */
                    else {
                        blocked = true;
                        Debug.Log("Cannot go further");
                    }
                }


                /*
                 * In the pre-drawing state, the direction controls where the player travels.
                 * If direction is equal to what their primary input is, ie it was unchanged,
                 * travel normally along the direction until a corner.
                 * 
                 * If direction is NOT equal to the primary input because it was changed from UpdatePlayerInputDirection(),
                 * then the player is trying to find the nearest grid mark to start drawing from
                 * 
                 * If direction does not match primary, that means we are trying to enter the drawing
                 * state but we are not on the grid.
                 */
                else if(state == PlayerStates.PreDrawing) {

                    /* Get the shortest distance between the remaining distance to travel and the distance to the corner */
                    float shortestDistance = Mathf.Min(currentLine.DistanceToCorner(gamePosition, direction), distance);
                    
                    /* If we are trying to find the nearest grid mark, update shortestDistance to reflect said distance */
                    if(direction != primary) {
                        //shortestDistance = Mathf.Min(shortestDistance, distanceToClosestGrid());
                    }

                    /* Move the player along the given direction for the shortest of the distances */
                    MovePlayer(direction, shortestDistance);
                    distance -= shortestDistance;
                }


                /*
                 * In the drawing state, just block the player for now.
                 */
                else if(state == PlayerStates.Drawing) {
                    blocked = true;
                }

            }

            else {
                /* The given direction is NULL - stop the player in their current position */
                blocked = true;
            }
        }
    }

    private void MovePlayer(OrthogonalDirection direction, float distance) {
        /*
         * Move the player's game position along the given direction for the given distance
         */

        gamePosition += Corner.DirectionToVector(direction)*distance;
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
        Corner currentCorner = GetCorner();
        OrthogonalDirection primaryDirection = dir1;
        OrthogonalDirection secondairyDirection = dir2;

        /* Make sure we are on a corner first */
        if(currentCorner != null) {
            
            /* Properly order the inputs if we are given two unique directions */
            if(dir1 != OrthogonalDirection.NULL && dir2 != OrthogonalDirection.NULL) {
                /* Current line is horizontal. */
                if(currentLine.IsHorizontal()) {
                    /* If the primary direction is also horizontal... */
                    if(Corner.HoriDirection(primaryDirection)) {
                        /* ...Swap it so the primary direction is now vertical */
                        primaryDirection = dir2;
                        secondairyDirection = dir1;
                    }
                }

                /* Current line is vertical. */
                else if(currentLine.IsVertical()) {
                    /* If the primary direction is also vertical... */
                    if(Corner.VertDirection(primaryDirection)) {
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

    private OrthogonalDirection UpdatePlayerInputDirection(PlayerStates givenState, 
            OrthogonalDirection primary, OrthogonalDirection secondairy) {
        /*
         * Return the direction the player will attempt to travel towards. This direction
         * is obtained by analyzing the given directions and player state.
         */
        OrthogonalDirection direction = OrthogonalDirection.NULL;

        /*
         * In the travelling state, the player will travel along their current line and favor
         * their primary inputted direction over their secondairy.
         */
        if(state == PlayerStates.Travelling) {

            /* If the player is on a corner, change their current line relative to their direction */
            ChangeCurrentLine(primary, secondairy);

            /* Get the direction that is parallel to the current line */
            if(currentLine.IsHorizontal()) {
                direction = Line.ReturnHorizontalDirection(primary, secondairy);
            }
            else if(currentLine.IsVertical()) {
                direction = Line.ReturnVerticalDirection(primary, secondairy);
            }

            /* If the player is not holding a direction parallel to the line, use their primairy input */
            if(direction == OrthogonalDirection.NULL) {
                direction = primary;
            }

            /*
             * If the direction to use is perpendicular to the current line, scan the nearby corners
             * to see if there is an attached line pointing in the given direction
             */
            if(currentLine.IsDirectionPerpendicular(direction)) {
                OrthogonalDirection newDirection = ScanForLineInDirection(direction);
                if(newDirection != OrthogonalDirection.NULL) {
                    direction = newDirection;

                    /* Update their current line if needed to reflect the change */
                    ChangeCurrentLine(newDirection, OrthogonalDirection.NULL);
                }
            }
        }

        /*
         * When in the PreDrawing state, the player is prepared to leave their current line
         * to start drawing their own line. The movement in this state aims to put the player
         * onto a nearby corner or onto one of the grid edges determined by the game's size.
         * 
         * Only use the primary input in this mode and get the grid size of the linked game controller.
         * Any input that points the player off their line will cause the player to enter the drawing state.
         */
        else if(state == PlayerStates.PreDrawing) {
            float grid = gameController.gridSize;

            /* Determine how to handle a non-null input */
            if(primary != OrthogonalDirection.NULL) {
                /* When on a corner... */
                if(GetCorner() != null) {
                    /* ...And inputting a direction that leads to a line... */
                    if(GetCorner().AttachedLineAt(primary) != null) {
                        /* ...Use the primary input to travel */
                        direction = primary;
                    }

                    /* ...And inputting a direction that leads off the corner... */
                    else {
                        /* ...Enter the drawing state */
                        state = PlayerStates.Drawing;
                        direction = UpdatePlayerInputDirection(state, primary, secondairy);
                    }
                }

                /* When on a line... */
                else {
                    /* ...And inputting a direction along said line... */
                    if(currentLine.IsDirectionParallel(primary)) {
                        /* ...Use primary input to travel */
                        direction = primary;
                    }

                    /* ...And inputting a direction perpendicular to said line... */
                    else {

                        //////CHECK IF THE PLAYER IS ON A GRID.
                        //////ELSE, SET DISTANCE TO CLOSEST GRID/CORNER
                        //////Make a "nearest grid" function that returns the direction to the nearest grid OR corner
                        /* ...Enter the drawing state */
                        state = PlayerStates.Drawing;
                        direction = UpdatePlayerInputDirection(state, primary, secondairy);
                    }
                }
            }
        }

        /*
         * When in the drawing state, dont do anything for now
         */
        else if(state == PlayerStates.Drawing) {
            Debug.Log("Drawing state");
            direction = OrthogonalDirection.NULL;
        }

        return direction;
    }

    #endregion


    #region Helper Functions --------------------------------------------------------- */

    private OrthogonalDirection ScanForLineInDirection(OrthogonalDirection direction) {
        /*
         * Scan ahead along the player's current line's both direction. 
         * Return the direction along the line that points towards the closest line.
         */
        
        /* Get the first lines from both the player's current line which are travelling along the given direction */
        Line dir1Line = GetClosestLineTowards(direction, Corner.NextDirection(direction));
        Line dir2Line = GetClosestLineTowards(direction, Corner.PreviousDirection(direction));
        
        /* If two lines were found, pick the direction towards the closest one */
        if(dir1Line != null && dir2Line != null) {

            /* Get the distance between each line */
            float tempDistance1 = (dir1Line.GetCornerInGivenDirection(Corner.OppositeDirection(direction)).position - gamePosition).magnitude;
            float tempDistance2 = (dir2Line.GetCornerInGivenDirection(Corner.OppositeDirection(direction)).position - gamePosition).magnitude;

            /* Set the direction to the closest corner */
            if(tempDistance1 < tempDistance2) {
                direction = Corner.NextDirection(direction);
            }
            else {
                direction = Corner.PreviousDirection(direction);
            }
        }

        /* Properly set the direction to reflect if one or none lines were close enough */
        else if(dir1Line != null || dir2Line != null) {
            if(dir1Line != null) {
                direction = Corner.NextDirection(direction);
            }
            else if(dir2Line != null) {
                direction = Corner.PreviousDirection(direction);
            }
        }

        return direction;
    }

    Line GetClosestLineTowards(OrthogonalDirection lineDirection, OrthogonalDirection playerDirection) {
        /*
         * Given a direction of a desired line and the direction of the player along their current line,
         * return the first line encountered along the playerDirection that is traveling towards lineDirection.
         */
        Line desiredLine = null;
        float minSnapDistance = 1;

        /* Get the first corner encountered along the player direction direction */
        float leftDistance = minSnapDistance;
        Corner leftCorner = currentLine.GetCornerInGivenDirection(playerDirection);
        while(leftCorner != null && desiredLine == null) {

            /* Get the distance from the player to this corner. If it's too far from the player, stop tracking */
            leftDistance = (leftCorner.position - gamePosition).magnitude;
            if(leftDistance < minSnapDistance) {
                /* Track the first line encountered that goes along the desired direction */
                desiredLine = leftCorner.AttachedLineAt(lineDirection);

                /* Get the next corner along the direction */
                if(leftCorner.AttachedLineAt(playerDirection) != null) {
                    leftCorner = leftCorner.AttachedLineAt(playerDirection).GetCornerInGivenDirection(playerDirection);
                }

                /* This was the last corner, so set the corner to null to stop the while loop */
                else {
                    leftCorner = null;
                }
            }

            /* Stop searching for the line if the corner is too far from the player */
            else {
                leftCorner = null;
            }
        }

        return desiredLine;
    }

    #endregion
}
