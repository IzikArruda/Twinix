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
    public Corner startCorner;
    public Vector3 end;
    public Corner endCorner;
    public Vector3[] vertices;
    public Mesh mesh;

    /* The list of players that are currently linked to this line */
    public List<Player> linkedPlayers;

    #endregion


    #region Constructors --------------------------------------------------------- */
    
    private Line(Vector3 startPos, Vector3 endPos) {
        /*
         * Create a line with the give start and end positions. Calculate the starting position.
         */

        width = -1;
        start = startPos;
        end = endPos;
        vertices = new Vector3[4];
        mesh = new Mesh();
        linkedPlayers = new List<Player>();
    }

    public static Line NewLine(Vector3 startPos, Vector3 endPos) {
        return new Line(startPos, endPos);
    }

    public static Line NewLine(float startX, float startY, float endX, float endY) {
        return new Line(new Vector3(startX, startY, 0), new Vector3(endX, endY, 0));
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
        if(IsVertical()) {
            lineExtraWidth = new Vector3(width/2f, 0);
            lineExtraLength = new Vector3(0, width/2f);
            /* Flip the mesh if needed */
            if(Corner.IsDirectionNegative(StartToEndDirection())) {
                lineExtraLength *= -1;
                lineExtraWidth *= -1;
            }
        }
        else if(IsHorizontal()) {
            lineExtraWidth = new Vector3(0, width/2f);
            lineExtraLength = new Vector3(width/2f, 0);
            /* Flip the mesh if needed */
            if(Corner.IsDirectionNegative(StartToEndDirection())) {
                lineExtraLength *= -1;
            }
            else {
                lineExtraWidth *= -1;
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
    
    public void AddCorner(Corner newCorner) {
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

    public void RemoveCorner(Corner savedCorner) {
        /*
         * Remove the given corner from this line
         */

        if(startCorner.Equals(savedCorner)) {
            startCorner = null;
        }
        else if(endCorner.Equals(savedCorner)) {
            endCorner = null;
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

    
    #region Line Functions  --------------------------------------------------------- */

    public Line SplitLine(Vector3 splitPosition, GameController gameController) {
        /*
         * Split the current line into two lines along with a corner.
         * The given position is where the line will be split.
         * The function will always shorten the current line 
         * from it's end position and keep it's start position unchanged.
         */
        Line newLine = null;
        Corner newCorner = null;

        /* Check if the given position is on this line, exclusing the corners */
        if(PointOnLine(splitPosition, false)) {

            /* Create a new line that goes from the given position to the current line's end */
            newLine = NewLine(splitPosition.x, splitPosition.y, end.x, end.y);

            /* Link the new line to the end corner of this line */
            newCorner = endCorner;
            newCorner.RemoveLine(this);
            newCorner.AddLine(newLine);

            /* Reposition the end point of this line to match the new line's starting position */
            end = splitPosition;

            /* Create a new corner that connects this line with the new line */
            newCorner = Corner.NewCorner(splitPosition);
            newCorner.AddLine(this);
            newCorner.AddLine(newLine);

            /* Re-create the vertices that make up the two lines */
            newLine.GenerateVertices(gameController);
            GenerateVertices(gameController);

            /* Add the line and corner to the gameController and it's lineDrawer */
            gameController.AddLine(newLine);
            gameController.AddCorner(newCorner);
        }

        if(linkedPlayers.Count > 0) {
            Debug.Log("Split line with players on it");
        }

        return newLine;
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
        Vector3 vectorDirection = Corner.DirectionToVector(direction);

        /* Get the distance needed to reach the corner */
        float distanceToCorner = DistanceToCorner(givenPositon, direction);

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
                Corner corner = GetCornerInGivenDirection(direction);
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
        float playerPos = 0;
        Vector3 collisionPosition = Vector3.zero;

        /* Calculate the proper min and max ranges of the two positions */
        float pos1 = Corner.GetVectorAxisValue(position1, StartToEndDirection());
        float pos2 = Corner.GetVectorAxisValue(position2, StartToEndDirection());
        float minRange = Mathf.Min(pos1, pos2);
        float maxRange = Mathf.Max(pos1, pos2);
        
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

        if(Corner.HoriDirection(dir1)) {
            horiDir = dir1;
        }
        else if(Corner.HoriDirection(dir2)) {
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

        if(Corner.VertDirection(dir1)) {
            vertDir = dir1;
        }
        else if(Corner.VertDirection(dir2)) {
            vertDir = dir2;
        }

        return vertDir;
    }
























    public Corner GetCornerInGivenDirection(OrthogonalDirection direction) {
        /*
         * Return the corner that the given direction points to.
         * If both corners are on the same position, return the start corner.
         */
        Corner corner = null;
        Vector3 cornerPosition = GetCornerPositionInGivenDirection(direction);

        /* Use GetCornerPositionInGivenDirection and compare the position of the corner */
        if(start.Equals(cornerPosition)) {
            corner = startCorner;
        }
        else {
            corner = endCorner;
        }

        return corner;
    }
    
    public Vector3 GetCornerPositionInGivenDirection(OrthogonalDirection direction) {
        /*
         * Return the position where the line ends at in the given direction
         */
        Vector3 position = Vector3.zero;
        Vector3 newPosition = Vector3.zero;
        float startPos, endPos;
        startPos = endPos = 0;
        
        /* If the given direction is a negative direction, flip the start and end values */
        bool flip = Corner.IsDirectionNegative(direction);

        /* Set the start and end values relative to their axis */
        startPos = Corner.GetVectorAxisValue(start, direction);
        endPos = Corner.GetVectorAxisValue(end, direction);

        /* Do the comparasion */
        if(flip ^ (startPos > endPos)) {
            newPosition = start;
        }
        else {
            newPosition = end;
        }
        
        return newPosition;
    }
    
    public bool IsDirectionParallel(OrthogonalDirection direction) {
        /*
         * Return whether the given direction is parallel to this line
         */
        bool parallel = false;

        if(Corner.HoriDirection(direction) && IsHorizontal() ||
                Corner.VertDirection(direction) && IsVertical()) {
            parallel = true;
        }

        return parallel;
    }

    public bool IsDirectionPerpendicular(OrthogonalDirection direction) {
        /*
         * Return whether the given direction is perpendicular (true) to this line
         */
        bool perpendicular = false;

        if(Corner.HoriDirection(direction) && IsVertical() ||
                Corner.VertDirection(direction) && IsHorizontal()) {
            perpendicular = true;
        }

        return perpendicular;
    }
    
    public bool PointOnLine(Vector3 position, bool includeCorners) {
        /*
         * Return whether the given position is on the current line or not.
         * The given boolean determines if we will include it's corners as part of the line.
         */
        bool onLine = false;
        OrthogonalDirection lineDirection = StartToEndDirection();
        float posRange = Corner.GetVectorAxisValue(position, lineDirection);
        float min = Mathf.Min(Corner.GetVectorAxisValue(start, lineDirection), Corner.GetVectorAxisValue(end, lineDirection));
        float max = Mathf.Max(Corner.GetVectorAxisValue(start, lineDirection), Corner.GetVectorAxisValue(end, lineDirection));
        //Use a NextDirection to get a direction perpendicular to the line
        float flat = Corner.GetVectorAxisValue(start, Corner.NextDirection(lineDirection));
        float posFlat = Corner.GetVectorAxisValue(position, Corner.NextDirection(lineDirection));

        /* Compare the position to the line (and it's corners if needed) */
        if(posFlat == flat && ((posRange > min && posRange < max) || (includeCorners && (posRange == min || posRange == max)))) {
            onLine = true;
        }

        return onLine;
    }
    
    public float DistanceToCorner(Vector3 position, OrthogonalDirection direction) {
        /*
         * Given a direction along this line and a position on the line,
         * return the distance require to reach the given position to the corner.
         * 
         * If either the given position is not on the line or the given direction
         * is not parallel to the line, print an error statement.
         */
        float distance = 0;

        if(PointOnLine(position, true) && IsDirectionParallel(direction)) {
            distance = (position - GetCornerPositionInGivenDirection(direction)).magnitude;
        }
        else {
            Debug.Log("WARNING: A given position or direction do not reflect the current line");
        }

        return distance;
    }

    public float DistanceToClosestGrid(Vector3 position, OrthogonalDirection direction, float gridSize, bool includeCorders) {
        /*
         * Given a position and direction on this line, return the distance to the closest grid mark.
         * The given boolean controls whether we should include nearby corners.
         */
        float pos = 0;
        float dir = 0;

        /* Get the values we want from the positions and direction depending on the line's orientation */
        if(IsHorizontal()) {
            pos = position.x;
            dir = Corner.DirectionToVector(direction).x;
        }
        else if(IsVertical()) {
            pos = position.y;
            dir = Corner.DirectionToVector(direction).y;
        }
        float remainder = pos % (gridSize);

        /* Ensure the given direction and position are actually on the line */
        if(IsDirectionPerpendicular(direction)) {
            Debug.Log("WARNING: Given direction is perpendicular to this line");
        }
        if(!PointOnLine(position, true)) {
            Debug.Log("WARNING: Given position is not on this line");
        }
    
        /* Change the nearest grid mark to match the orientation of the direction */
        if(dir > 0) {
            remainder = gridSize - remainder;
        }

        /* Check if the nearest corner is encountered before the grid mark */
        if(includeCorders) {
            remainder = Mathf.Min(remainder, DistanceToCorner(position, direction));
        }

        return remainder;
    }


















    public OrthogonalDirection StartToEndDirection() {
        /*
         * Return the direction gotten by going from the start position towards the end position.
         * This function is used to get the direction the line is heading towards.
         */
        OrthogonalDirection lineDirection = OrthogonalDirection.NULL;

        if(IsHorizontal()) {
            if(start.x < end.x) {
                lineDirection = OrthogonalDirection.Right;
            }
            else if(start.x > end.x) {
                lineDirection = OrthogonalDirection.Left;
            }
        }

        else if(IsVertical()) {
            if(start.y < end.y) {
                lineDirection = OrthogonalDirection.Up;
            }
            else if(start.y > end.y) {
                lineDirection = OrthogonalDirection.Down;
            }
        }

        return lineDirection;
    }
    
    #endregion
}
