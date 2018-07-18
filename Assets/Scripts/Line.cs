﻿using UnityEngine;
using System.Collections;


/*
 * The Line class that the line renderer will use to draw the lines.
 * Each line has a start and end point, each ending with a corner.
 */
public class Line {

    #region Variables  --------------------------------------------------------- */

    public float width;
    public Vector2 start;
    public LineCorner startCorner;
    public Vector2 end;
    public LineCorner endCorner;
    public Vector3[] vertices;
    public Mesh mesh;

    #endregion


    #region Constructors --------------------------------------------------------- */
    
    public Line(float startX, float startY, float endX, float endY) {
        /*
         * Create a line with the give start and end positions. Calculate the starting position.
         */

        width = -1;
        start = new Vector2(startX, startY);
        end = new Vector2(endX, endY);
        vertices = new Vector3[4];
        mesh = new Mesh();
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
            lineExtraWidth = new Vector2(width/2f, 0);
            lineExtraLength = new Vector2(0, width/2f);
            /* Flip the mesh if needed */
            if(start.y > end.y) {
                lineExtraWidth *= -1;
                lineExtraLength *= -1;
            }
        }
        else if(start.y == end.y/* Vertical */) {
            lineExtraWidth = new Vector2(0, width/2f);
            lineExtraLength = new Vector2(width/2f, 0);
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


    #region Helper Functions  --------------------------------------------------------- */

    public void DistanceToCornerFrom(Vector3 givenPositon, OrthogonalDirection direction) {
        /*
         * Return the amount of in-game distance from the given point along 
         * the line towards the given direction until it reaches the corner of the line.
         * 
         * If the given point is not on the line or the direction does not follow it, print an error.
         */
         
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

    public bool IsPointOnLine(Vector3 point) {
        /*
         * Return true if the given point is on this line
         */
        bool onLine = false;
        bool lineFlipped = false;

        if(IsHorizontal()) {
            if(point.y == start.y) {
                lineFlipped = start.x < end.x;
                if(point.x >= (lineFlipped ? start.x : end.x) && point.x <= (lineFlipped ? end.x : start.x)) {
                    onLine = true;
                }

                else {
                    //The point is not within the start or end points
                }
            }
            else {
                //Point is not on the same horizontal axis
            }
        }

        else if(IsVertical()) {
            if(point.x == start.x) {
                lineFlipped = start.y < end.y;
                if(point.y >= (lineFlipped ? start.y : end.y) && point.y <= (lineFlipped ? end.y : start.y)) {
                    onLine = true;
                }
                else {
                    //The point is not within the start or end points
                }
            }
            else {
                //Point is not on the same vertical axis
            }
        }

        else {
            Debug.Log("WARNING: Line is not vertical or horizontal");
        }

        return onLine;
    }

    #endregion
}
