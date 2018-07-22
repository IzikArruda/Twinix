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

    //get if its horizontal
};

/*
 * A line corner is an intersection that connects a set of lines.
 * 
 * A line corner can only be attached to a line from their ends - ie at their end or start position.
 */
public class LineCorner {

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

    public LineCorner(Vector3 cornerPos) {
        /*
         * Create a new empty line corner at the given position
         */

        position = cornerPos;
        up = null;
        right = null;
        down = null;
        left = null;
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
    
    #endregion
}
