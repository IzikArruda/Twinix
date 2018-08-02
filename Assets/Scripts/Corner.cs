using UnityEngine;
using System.Collections;


/*
 * The orthogonal directions that lines can connect to a corner
 */
public enum OrthogonalDirection {
    NULL,
    Up,
    Right,
    Down,
    Left
};

/*
 * A line corner is an intersection that connects a set of lines.
 * 
 * A line corner can only be attached to a line from their ends - ie at their end or start position.
 */
public class Corner {

    #region Variables  --------------------------------------------------------- */

    /* The connected lines at the corner */
    public Line up;
    public Line right;
    public Line down;
    public Line left;

    /* The position of the connection */
    public Vector3 position;

    #endregion


    #region Constructors  --------------------------------------------------------- */

    private Corner(Vector3 cornerPosition) {
        /*
         * Create a new empty corner corner at the given position
         */

        position = cornerPosition;
        up = null;
        right = null;
        down = null;
        left = null;
    }

    public static Corner NewCorner(float posX, float posY) {
        return new Corner(new Vector3(posX, posY, 0));
    }

    public static Corner NewCorner(Vector3 cornerPosition) {
        return new Corner(cornerPosition);
    }
    
    #endregion


    #region Line Connection Functions  --------------------------------------------------------- */

    public bool AddLine(Line newLine) {
        /*
         * Add the given line to this corner. 
         * 
         * Get the OrthogonalDirection the line is to this corner and call
         * the AddLine(Line, OrthogonalDirection) function.
         */
        bool lineAdded = false;

        /* Get the orthogonal direction of the line to this corner */
        OrthogonalDirection orthogonalDirection = LineSide(newLine);

        /* Try to add the line to this corner */
        lineAdded = AddLine(newLine, orthogonalDirection);

        return lineAdded;
    }

    public bool AddLine(Line newLine, OrthogonalDirection orthDir) {
        /*
         * Add the given line to this corner. Return true if it's added, false if not.
         */
        bool lineAdded = false;

        switch(orthDir) {
            case OrthogonalDirection.Up:
                lineAdded = SetLine(ref up, newLine);
                break;
            case OrthogonalDirection.Right:
                lineAdded = SetLine(ref right, newLine);
                break;
            case OrthogonalDirection.Down:
                lineAdded = SetLine(ref down, newLine);
                break;
            case OrthogonalDirection.Left:
                lineAdded = SetLine(ref left, newLine);
                break;
            default:
                lineAdded = false;
                break;
        }

        return lineAdded;
    }

    private bool SetLine(ref Line savedLine, Line newLine) {
        /*
         * Set the new given line to the corner's saved line.
         * Return true if it was able to be set, false if not.
         * 
         * When a corner is assigned a line, also set the line's corner
         */
        bool lineSet = false;

        if(savedLine == null) {
            savedLine = newLine;
            newLine.AddCorner(this);
            lineSet = true;
        }

        return lineSet;
    }

    public bool RemoveLine(Line savedLine) {
        /*
         * Unlink the given line from this corner. Return true if the line was removed.
         */
        bool removed = false;

        /* Remove the given line if it is linked to this corner */
        if(up != null && up.Equals(savedLine)) {
            up = null;
            removed = true;
        }
        else if(right != null && right.Equals(savedLine)) {
            right = null;
            removed = true;
        }
        else if(down != null && down.Equals(savedLine)) {
            down = null;
            removed = true;
        }
        else if(left != null && left.Equals(savedLine)) {
            left = null;
            removed = true;
        }

        if(removed) {
            savedLine.RemoveCorner(this);
        }

        return removed;
    }
    
    public bool LinkLineStart(Line line, OrthogonalDirection direction) {
        /*
         * Link the given line's start corner to this corner along
         * the given direction. Return true if the connection was made.
         */
        bool connected = false;

        /* Make sure the line's start corner is on the same position as this corner */
        if(!position.Equals(line.start)) {
            Debug.Log("WARNING: This corner does not connect to the given line's start corner");
            connected = false;
        }
        
        /* Set this corner's saved line to the given line */
        if(AddLine(line, direction)) {

        }
        else {
            Debug.Log("WARNING: This corner already has a line linked to the given direction");
            connected = false;
        }

        return connected;
    }

    #endregion


    #region Helper Functions  --------------------------------------------------------- */

    public OrthogonalDirection LineSide(Line line) {
        /*
         * Return which side of this corner the given line is connected to.
         */
        Vector3 direction = Vector3.zero;
        OrthogonalDirection orthogonalDirection = OrthogonalDirection.NULL;
        
        /* The line hits the corner in it's start point */
        if(position.Equals(line.start)) {
            direction = line.end - line.start;
        }
        /* The line hits the corner in it's end point */
        else if(position.Equals(line.end)) {
            direction = line.start - line.end;
        }
        /* The line's start/end points do not connect to this corner */
        else {

        }
        
        /* Get the orthoganal direction of the corner that the line is connected to */
        direction.Normalize();
        if(direction.Equals(Vector3.up)) {
            orthogonalDirection = OrthogonalDirection.Up;
        }
        else if(direction.Equals(Vector3.right)) {
            orthogonalDirection = OrthogonalDirection.Right;
        }
        else if(direction.Equals(Vector3.down)) {
            orthogonalDirection = OrthogonalDirection.Down;
        }
        else if(direction.Equals(Vector3.left)) {
            orthogonalDirection = OrthogonalDirection.Left;
        }
        else if(direction.Equals(Vector3.zero)){
            Debug.Log("WARNING: Trying to add a line with 0 length");
        }
        
        return orthogonalDirection;
    }

    static public bool HoriDirection(OrthogonalDirection direction) {
        /*
         * Return true if the given direction is horizontal (left, right)
         */

        return (direction == OrthogonalDirection.Left || direction == OrthogonalDirection.Right);
    }

    static public bool VertDirection(OrthogonalDirection direction) {
        /*
         * Return true if the given direction is vertical (up, down)
         */

        return (direction == OrthogonalDirection.Up || direction == OrthogonalDirection.Down);
    }

    static public Vector3 DirectionToVector(OrthogonalDirection direction) {
        /*
         * Convert the given orthogonal direction to it's Vector equivalent
         */
        Vector3 directionVector = new Vector3(0, 0, 0);

        if(direction == OrthogonalDirection.Up) {
            directionVector = Vector3.up;
        }
        else if(direction == OrthogonalDirection.Right) {
            directionVector = Vector3.right;
        }
        else if(direction == OrthogonalDirection.Down) {
            directionVector = Vector3.down;
        }
        else if(direction == OrthogonalDirection.Left) {
            directionVector = Vector3.left;
        }

        return directionVector;
    }

    public Line AttachedLineAt(OrthogonalDirection direction) {
        /*
         * Return the line attached to this corner at the given direction
         */
        Line attachedLine = null;

        if(direction == OrthogonalDirection.Up) {
            attachedLine = up;
        }
        else if(direction == OrthogonalDirection.Right) {
            attachedLine = right;
        }
        else if(direction == OrthogonalDirection.Down) {
            attachedLine = down;
        }
        else if(direction == OrthogonalDirection.Left) {
            attachedLine = left;
        }

        return attachedLine;
    }

    static public OrthogonalDirection NextDirection(OrthogonalDirection direction) {
        /*
         * Return the "next" direction in the sequence from the given direction.
         */

        if(direction == OrthogonalDirection.Up) {
            direction = OrthogonalDirection.Right;
        }

        else if(direction == OrthogonalDirection.Right) {
            direction = OrthogonalDirection.Down;
        }

        else if(direction == OrthogonalDirection.Down) {
            direction = OrthogonalDirection.Left;
        }

        else if(direction == OrthogonalDirection.Left) {
            direction = OrthogonalDirection.Up;
        }

        return direction;
    }

    static public OrthogonalDirection PreviousDirection(OrthogonalDirection direction) {
        /*
         * Return the "previous" direction in the sequence from the given direction.
         */

        if(direction == OrthogonalDirection.Up) {
            direction = OrthogonalDirection.Left;
        }

        else if(direction == OrthogonalDirection.Right) {
            direction = OrthogonalDirection.Up;
        }

        else if(direction == OrthogonalDirection.Down) {
            direction = OrthogonalDirection.Right;
        }

        else if(direction == OrthogonalDirection.Left) {
            direction = OrthogonalDirection.Down;
        }

        return direction;
    }

    static public OrthogonalDirection OppositeDirection(OrthogonalDirection direction) {
        /*
         * Return the opposite direction of the given direction
         */

        if(direction == OrthogonalDirection.Up) {
            direction = OrthogonalDirection.Down;
        }

        else if(direction == OrthogonalDirection.Right) {
            direction = OrthogonalDirection.Left;
        }

        else if(direction == OrthogonalDirection.Down) {
            direction = OrthogonalDirection.Up;
        }

        else if(direction == OrthogonalDirection.Left) {
            direction = OrthogonalDirection.Right;
        }

        return direction;
    }

    static public bool IsDirectionsPerpendicular(OrthogonalDirection dir1, OrthogonalDirection dir2) {
        /*
         * Return true if dir2 is perpendicular to dir1
         */
        bool perpendicular = false;

        if(dir1.Equals(NextDirection(dir2)) || dir1.Equals(PreviousDirection(dir2))) {
            perpendicular = true;
        }

        return perpendicular;
    }

    static public bool IsDirectionNegative(OrthogonalDirection direction) {
        /*
         * Return whether the given direction goes along the negative axis.
         * ie, up is along the positive Y axis, but down is along the negative Y axis.
         */
        bool negativeDirection = false;

        if(direction.Equals(OrthogonalDirection.Down) ||
                direction.Equals(OrthogonalDirection.Left)) {
            negativeDirection = true;
        }

        return negativeDirection;
    }

    static public bool IsDirectionsOpposites(OrthogonalDirection dir1, OrthogonalDirection dir2) {
        /*
         * Return true if the two given directions are opposites
         */
        bool oppositeDirections = false;

        if(dir1.Equals(OppositeDirection(dir2))) {
            oppositeDirections = true;
        }

        return oppositeDirections;
    }

    static public float GetVectorAxisValue(Vector3 vector, OrthogonalDirection axis) {
        /*
         * Given a vector and an axis represented as an orthogonal direction,
         * return the value of vector's given axis. 
         * i.e.: a vector of (0, 1, 2) and a given axis of UP or DOWN would return 1.
         */
        float axisValue = 0;

        if(HoriDirection(axis)) {
            axisValue = vector.x;
        }
        else if(VertDirection(axis)) {
            axisValue = vector.y;
        }

        return axisValue;
    }

    #endregion
}
