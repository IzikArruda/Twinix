using UnityEngine;
using System.Collections;


/*
 * The Line class that the line renderer will use to draw the lines
 */
public class Line {

    #region Variables  --------------------------------------------------------- */

    public float width;
    public Vector2 start;
    public Vector2 end;
    public Vector3[] vertices;
    public Mesh mesh;

    #endregion


    #region Line Creation Functions  --------------------------------------------------------- */

    public Line() {
        /*
         * Set the line's varaibles to their default
         */

        width = -1;
        start = Vector2.zero;
        end = Vector2.zero;
        vertices = new Vector3[4];
        mesh = new Mesh();
    }

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

    public static Line CreateLine() {
        /*
         * Create a new, empty line object
         */
         
        return new Line();
    }

    #endregion


    #region Mesh Creation Functions  --------------------------------------------------------- */

    public void GenerateVertices(float gameAreaX, float gameAreaY) {
        /*
         * Generate the vertices and their positions that will be used to render the line as a mesh.
         * Requires the start, end and width values to be set. Ther vertices are placed in an order
         * so that two triangles of [0, 1, 2] and [2, 3, 0] will form the line with a proper normal.
         * Depending on the current resolution mode and size of the gameArea, adjust the vertices.
         * 
         * A line requires access to a windowController's variables in order to properly place the 
         * vertices in relation to the window size and rendering method.
         */
        Vector2 lineExtraWidth = Vector2.zero;
        Vector2 lineExtraLength = Vector2.zero;
        Vector3 centerOffset = Vector3.zero;
        float heightRatio = 1;
        float widthRatio = 1;


        /* Get values from the windowController that control how the game is rendered */
        float windowHeight = WindowController.windowHeight - WindowController.edgeBufferSize;
        float windowWidth = WindowController.windowWidth - WindowController.edgeBufferSize;


        /* Don't adjust the size of the line, just set the offset to center it */
        if(WindowController.currentResolutionMode == ResolutionMode.True) {
            centerOffset = new Vector2(gameAreaX/2f, gameAreaY/2f);
        }
        /* Call for the line to stretch it's size relative to the screen's width and height */
        else if(WindowController.currentResolutionMode == ResolutionMode.Stretch) {
            heightRatio = windowHeight/gameAreaY;
            widthRatio = windowWidth/gameAreaX;
            centerOffset = new Vector2(widthRatio*gameAreaX/2f, heightRatio*gameAreaY/2f);
        }
        /* Stretch to fit the screen while keeping the ratio intact */
        else if(WindowController.currentResolutionMode == ResolutionMode.TrueRatioStretch) {
            float ratio = Mathf.Min(windowHeight/gameAreaY, windowWidth/gameAreaX);
            heightRatio = ratio;
            widthRatio = ratio;
            centerOffset = new Vector2(ratio*gameAreaX/2f, ratio*gameAreaY/2f);
        }
        /* Print an error if we are currently in a rendering mode not handled */
        else {
            Debug.Log("WARNING: Resolution mode " + WindowController.currentResolutionMode + " is not handled by the line");
        }

        
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
        


        /* Set the vertices using the line's extra width */
        vertices[0] = new Vector2(widthRatio*end.x, heightRatio*end.y) - lineExtraWidth + lineExtraLength;
        vertices[1] = new Vector2(widthRatio*end.x, heightRatio*end.y) + lineExtraWidth + lineExtraLength;
        vertices[2] = new Vector2(widthRatio*start.x, heightRatio*start.y) + lineExtraWidth - lineExtraLength;
        vertices[3] = new Vector2(widthRatio*start.x, heightRatio*start.y) - lineExtraWidth - lineExtraLength;

        /* Apply the center offset to each vertice */
        for(int i = 0; i < vertices.Length; i++) {
            vertices[i] -= centerOffset;
        }

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
}
