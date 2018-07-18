﻿using UnityEngine;
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
public class LineCorner {

    #region Variables  --------------------------------------------------------- */

    /* The connected lines at the corner */
    public Line up;
    public Line right;
    public Line down;
    public Line left;

    /* The position of the connection */
    public Vector2 position;

    #endregion
    

    #region Constructors  --------------------------------------------------------- */

    public LineCorner(Vector2 cornerPos) {
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
        Vector2 direction = Vector2.zero;
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
        if(direction.Equals(Vector2.up)) {
            orthogonalDirection = OrthogonalDirection.Up;
        }
        else if(direction.Equals(Vector2.right)) {
            orthogonalDirection = OrthogonalDirection.Right;
        }
        else if(direction.Equals(Vector2.down)) {
            orthogonalDirection = OrthogonalDirection.Down;
        }
        else if(direction.Equals(Vector2.left)) {
            orthogonalDirection = OrthogonalDirection.Left;
        }
        
        return orthogonalDirection;
    }

    #endregion
}
