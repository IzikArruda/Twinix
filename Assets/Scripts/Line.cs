using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * The Line class that the line renderer will use to draw the lines.
 * Each line has a start and end point, each ending with a corner.
 */
public class Line {

    #region Variables  --------------------------------------------------------- */

    public float width;
    public Vector3 start;
    public LineCorner startCorner;
    public Vector3 end;
    public LineCorner endCorner;
    public Vector3[] vertices;
    public Mesh mesh;

    /* The list of players that are currently linked to this line */
    public List<Player> linkedPlayers;

    #endregion


    #region Constructors --------------------------------------------------------- */
    
    public Line(float startX, float startY, float endX, float endY) {
        /*
         * Create a line with the give start and end positions. Calculate the starting position.
         */

        width = -1;
        start = new Vector3(startX, startY);
        end = new Vector3(endX, endY);
        vertices = new Vector3[4];
        mesh = new Mesh();
        linkedPlayers = new List<Player>();
    }
    
    #endregion


    #region Mesh Creation Functions  --------------------------------------------------------- */

    public void GenerateVertices(GameController gameController) {
        /*
         * Generate the vertices and their positions that will be used to render the line as a mesh.
         * Requires the start, end and width values to be set. Ther vertices are placed in an order
         * so that two triangles of [0, 1, 2] and [2, 3, 0] will form the line with a proper normal.
         * 
         * A line requires access to a windowController's variables in order to properly place the 
         * vertices in relation to the window size and rendering method. This is done
         * by accessing the given gameController's GameToScreenPos helper function.
         */
        Vector3 lineExtraWidth = Vector3.zero;
        Vector3 lineExtraLength = Vector3.zero;


        /* Convert each vertice of the line from game position to screen positions */
        vertices[0] = gameController.GameToScreenPos(end);
        vertices[1] = gameController.GameToScreenPos(end);
        vertices[2] = gameController.GameToScreenPos(start);
        vertices[3] = gameController.GameToScreenPos(start);


        /* Apply the line's width to either the Y or X axis, depending on what axis the line is on */
        if(start.x == end.x/* Horizontal */) {
            lineExtraWidth = new Vector3(width/2f, 0);
            lineExtraLength = new Vector3(0, width/2f);
            /* Flip the mesh if needed */
            if(start.y > end.y) {
                lineExtraWidth *= -1;
                lineExtraLength *= -1;
            }
        }
        else if(start.y == end.y/* Vertical */) {
            lineExtraWidth = new Vector3(0, width/2f);
            lineExtraLength = new Vector3(width/2f, 0);
            /* Flip the mesh if needed */
            if(start.x < end.x) {
                lineExtraWidth *= -1;
            }
            else {
                lineExtraLength *= -1;
            }
        }
        else {
            Debug.Log("WARNING: Line is not directly horizontal or vertical");
        }
        
        /* Put the desired distance between the vertices to add width to the lines.
         * Line width is not effected and does not effect game area size. */
        vertices[0] += -lineExtraWidth + lineExtraLength;
        vertices[1] += lineExtraWidth + lineExtraLength;
        vertices[2] += lineExtraWidth - lineExtraLength;
        vertices[3] += -lineExtraWidth - lineExtraLength;

        /* Assign the vertices to the mesh */
        if(mesh != null) {
            mesh.vertices = vertices;
        }
    }

    public void GenerateMesh() {
        /*
         * Generate the mesh used to represent the line. 
         */
        

        /* Set the triangles of the mesh */
        int[] triangles = { 0, 1, 2, 2, 3, 0 };

        /* Set the normals of the mesh */
        Vector3[] normals = new Vector3[vertices.Length];
        for(int i = 0; i < normals.Length; i++) {
            normals[i] = Vector3.forward;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles; 
        mesh.normals = normals;
    }

    #endregion

    
    #region Corner Functions  --------------------------------------------------------- */
    
    public void AddCorner(LineCorner newCorner) {
        /*
         * Add the given corner to this line object. 
         */

        /* The newCorner is on the line's start position */
        if(start.Equals(newCorner.position)) {
            if(startCorner == null) {
                startCorner = newCorner;
            }
            else {
                Debug.Log("WARNING: Trying to add a corner to a line's start, but already have a start corner assigned");
            }
        }

        /* The newCorner is on the line's end position */
        else if(end.Equals(newCorner.position)) {
            if(endCorner == null) {
                endCorner = newCorner;
            }
            else {
                Debug.Log("WARNING: Trying to add a corner to a line's end, but already have a end corner assigned");
            }
        }

        /* The newCorner is not on one of the line's ends */
        else {
            Debug.Log("WARNING: Trying to add a corner onto a spot not on the edge of a line");
        }
    }

    #endregion

    
    #region Linked Player Functions  --------------------------------------------------------- */

    public void LinkPlayer(Player player) {
        /*
         * Add the given player to the currently linked players if they are not yet added
         */
         
        if(!linkedPlayers.Contains(player)) {
            linkedPlayers.Add(player);
        }
    }

    public void UnlinkPlayer(Player player) {
        /*
         * Remove the linked player from the line's current linked player list
         */
         
        linkedPlayers.Remove(player);
    }

    #endregion


    #region Helper Functions  --------------------------------------------------------- */

    public void PredeterminePlayerMovement(Player player, Vector3 givenPositon, OrthogonalDirection direction, 
            ref float currentDistanceTravelled, float maxtravelDistance, bool applyPlayerSizeBuffer, ref bool blocked) {
        /*
         * Travel along this line in the given direction from the given position for the given 
         * maxTravelDistance. The given directioin is assumed to be parallel to the line.
         * 
         * Set the blocked boolean to true if the path along the line collides with a player.
         * 
         * If we travel along the line and reach a corner, recursively call this function
         * along each of the corner's attached lines.
         * 
         * currentDistanceTravelled is how much distance the player has travelled so far.
         */
        Vector3 collisionPosition = Vector3.zero;
        /* Remove the buffer distance if needed */
        float playerSizeBuffer = 2f;
        if(!applyPlayerSizeBuffer) { playerSizeBuffer = 0; }

        /* Convert the given direction into a vector3 */
        Vector3 vectorDirection = LineCorner.DirectionToVector(direction);

        /* Get the distance needed to reach the corner */
        float distanceToStart = Vector3.Scale((start - givenPositon), vectorDirection).x + Vector3.Scale((start - givenPositon), vectorDirection).y;
        float distanceToEnd = Vector3.Scale((end - givenPositon), vectorDirection).x + Vector3.Scale((end - givenPositon), vectorDirection).y;
        float distanceToCorner = Mathf.Max(distanceToStart, distanceToEnd);

        /* Use the smaller distance between the remaining distance and the distance to the corner */
        float distanceOnCurrentLine = maxtravelDistance - currentDistanceTravelled;

        /* Prevent the player from moving past the corner if this is a non-recursive call */
        if(applyPlayerSizeBuffer) {
            distanceOnCurrentLine = Mathf.Min(maxtravelDistance - currentDistanceTravelled, distanceToCorner);
        }

        /* Check if there is a player between the given position and the buffered distance */
        collisionPosition = CheckIfPlayerWithinRange(givenPositon, givenPositon + vectorDirection*(distanceOnCurrentLine + playerSizeBuffer), player, ref blocked);
        
        /* If there was a collision, update the distance to reflect the distance to the collision */
        if(blocked) {
            distanceOnCurrentLine = (givenPositon - collisionPosition).magnitude;
            /* Remove the buffer from the distance travelled. Do not let the distance reach bellow 0 */
            distanceOnCurrentLine = Mathf.Max(distanceOnCurrentLine - playerSizeBuffer, 0);
            currentDistanceTravelled += distanceOnCurrentLine;
        }

        /* There was no collision meet along this path */
        else {
            /* If we have reached past the corner, check it's attachedLines */
            if((distanceOnCurrentLine + playerSizeBuffer) > distanceToCorner) {

                /* Get the corner the player can reach */
                LineCorner corner = GetCornerInGivenDirection(direction);
                float distancePastCorner = (distanceOnCurrentLine + playerSizeBuffer) - distanceToCorner;
                float tempDistance = currentDistanceTravelled;
                bool tempBlocked = false;

                /* Scan each line attached to the corner to see if there is a player in the way */
                if(corner.up != null && !corner.up.Equals(this)) {
                    tempBlocked = false;
                    tempDistance = currentDistanceTravelled;
                    corner.up.PredeterminePlayerMovement(player, corner.position, OrthogonalDirection.Up,
                        ref tempDistance, distancePastCorner, false, ref tempBlocked);
                    if(tempBlocked) {
                        blocked = true;
                        if(distancePastCorner > tempDistance) { distancePastCorner = tempDistance; }
                        Debug.Log("UP DIRECTION HIT PLAYER");
                    }
                }
                if(corner.right != null && !corner.right.Equals(this)) {
                    tempBlocked = false;
                    tempDistance = currentDistanceTravelled;
                    corner.right.PredeterminePlayerMovement(player, corner.position, OrthogonalDirection.Right,
                            ref tempDistance, distancePastCorner, false, ref tempBlocked);
                    if(tempBlocked) {
                        blocked = true;
                        if(distancePastCorner > tempDistance) { distancePastCorner = tempDistance; }
                        Debug.Log("RIGHT DIRECTION HIT PLAYER");
                    }
                }
                if(corner.down != null && !corner.down.Equals(this)) {
                    tempBlocked = false;
                    tempDistance = currentDistanceTravelled;
                    corner.down.PredeterminePlayerMovement(player, corner.position, OrthogonalDirection.Down,
                            ref tempDistance, distancePastCorner, false, ref tempBlocked);
                    if(tempBlocked) {
                        blocked = true;
                        if(distancePastCorner > tempDistance) { distancePastCorner = tempDistance; }
                        Debug.Log("DOWN DIRECTION HIT PLAYER");
                    }
                }
                if(corner.left != null && !corner.left.Equals(this)) {
                    tempBlocked = false;
                    tempDistance = currentDistanceTravelled;
                    corner.left.PredeterminePlayerMovement(player, corner.position, OrthogonalDirection.Left,
                            ref tempDistance, distancePastCorner, false, ref tempBlocked);
                    if(tempBlocked) {
                        blocked = true;
                        if(distancePastCorner > tempDistance) { distancePastCorner = tempDistance; }
                        Debug.Log("LEFT DIRECTION HIT PLAYER");
                    }
                }

                /* We ran into a player and so need to reduce the given distance */
                if(blocked) {
                    distanceOnCurrentLine -= playerSizeBuffer - distancePastCorner;
                }
            }

            /* Update the travel distance to reflect how far along the line we travelled */
            currentDistanceTravelled += distanceOnCurrentLine;
            if(currentDistanceTravelled < 0) { currentDistanceTravelled = 0; }
        }
    }
    
    private Vector3 CheckIfPlayerWithinRange(Vector3 position1, Vector3 position2, Player player, ref bool playerCollided) {
        /*
         * Check if there is a player between the two given positions.
         * If there is a player within the range, change the given boolean.
         * Return the position of where the collision occured.
         */
        float minRange = 0, maxRange = 0, playerPos = 0;
        Vector3 collisionPosition = Vector3.zero;

        /* Calculate the proper min and max ranges of the two positions */
        if(IsHorizontal()) {
            minRange = Mathf.Min(position1.x, position2.x);
            maxRange = Mathf.Max(position1.x, position2.x);
        }
        else if(IsVertical()) {
            minRange = Mathf.Min(position1.y, position2.y);
            maxRange = Mathf.Max(position1.y, position2.y);
        }

        for(int i = 0; i < linkedPlayers.Count; i++) {

            /* Ignore the player if they are at the same position as the given player */
            if(!linkedPlayers[i].gamePosition.Equals(player.gamePosition)) {

                /* Get the player's position */
                if(IsHorizontal()) {
                    playerPos = linkedPlayers[i].gamePosition.x;
                }
                else if(IsVertical()) {
                    playerPos = linkedPlayers[i].gamePosition.y;
                }

                /* Check if the player's position is within the range (inclusive. The players will get stuck if inside eachother) */
                if(playerPos >= minRange && playerPos <= maxRange) {
                    collisionPosition = linkedPlayers[i].gamePosition;
                    playerCollided = true;
                }
            }
        }

        return collisionPosition;
    }

    public bool IsHorizontal() {
        /*
         * Return true if the line is completely horizontal
         */

        return (start.y == end.y);
    }
    
    public bool IsVertical() {
        /*
         * Return true if the line is completely vertical
         */

        return (start.x == end.x);
    }
    
    public static OrthogonalDirection ReturnHorizontalDirection(OrthogonalDirection dir1, OrthogonalDirection dir2) {
        /*
         * Given two directions, return the direction that points in a horizontal direction.
         * Return the NULL direction if neither of them are horizontal.
         */
        OrthogonalDirection horiDir = OrthogonalDirection.NULL;

        if(LineCorner.HoriDirection(dir1)) {
            horiDir = dir1;
        }
        else if(LineCorner.HoriDirection(dir2)) {
            horiDir = dir2;
        }

        return horiDir;
    }

    public static OrthogonalDirection ReturnVerticalDirection(OrthogonalDirection dir1, OrthogonalDirection dir2) {
        /*
         * Given two directions, return the direction that points in a vertical direction.
         * Return the NULL direction if neither of them are vertical.
         */
        OrthogonalDirection vertDir = OrthogonalDirection.NULL;

        if(LineCorner.VertDirection(dir1)) {
            vertDir = dir1;
        }
        else if(LineCorner.VertDirection(dir2)) {
            vertDir = dir2;
        }

        return vertDir;
    }

    public LineCorner GetCornerInGivenDirection(OrthogonalDirection direction) {
        /*
         * Return the corner that the given direction points to
         */
        LineCorner corner = null;

        /* If the line is horizontal... */
        if(IsHorizontal()) {
            /* ... And we want the corner on the right... */
            if(direction == OrthogonalDirection.Right) {
                /* ... Return the corner with the larger x position */
                if(startCorner.position.x > endCorner.position.x) {
                    corner = startCorner;
                }
                else {
                    corner = endCorner;
                }
            }
            /* ... And we want the corner on the left... */
            else if(direction == OrthogonalDirection.Left) {
                /* ... Return the corner with the smaller x position */
                if(startCorner.position.x < endCorner.position.x) {
                    corner = startCorner;
                }
                else {
                    corner = endCorner;
                }
            }
        }

        /* If the line is vertical... */
        else if(IsVertical()) {
            /* ... And we want the corner on the top... */
            if(direction == OrthogonalDirection.Up) {
                /* ... Return the corner with the larger y position */
                if(startCorner.position.y > endCorner.position.y) {
                    corner = startCorner;
                }
                else {
                    corner = endCorner;
                }
            }
            /* ... And we want the corner on the bottom... */
            else if(direction == OrthogonalDirection.Down) {
                /* ... Return the corner with the smaller y position */
                if(startCorner.position.y < endCorner.position.y) {
                    corner = startCorner;
                }
                else {
                    corner = endCorner;
                }
            }
        }

        return corner;
    }
    
    public bool IsDirectionParallel(OrthogonalDirection direction) {
        /*
         * Return whether the given direction is parallel to this line
         */
        bool parallel = false;

        if(LineCorner.HoriDirection(direction) && IsHorizontal() ||
                LineCorner.VertDirection(direction) && IsVertical()) {
            parallel = true;
        }

        return parallel;
    }

    public bool IsDirectionPerpendicular(OrthogonalDirection direction) {
        /*
         * Return whether the given direction is perpendicular (true) to this line
         */
        bool perpendicular = false;

        if(LineCorner.HoriDirection(direction) && IsVertical() ||
                LineCorner.VertDirection(direction) && IsHorizontal()) {
            perpendicular = true;
        }

        return perpendicular;
    }

    #endregion
}
