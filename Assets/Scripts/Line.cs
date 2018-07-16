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

    public static float defaultWidth = -1;

    #endregion


    #region Line Creation Functions  --------------------------------------------------------- */

    public Line() {
        /*
         * Set the line's varaibles to their default
         */

        width = defaultWidth;
        start = Vector2.zero;
        end = Vector2.zero;
        vertices = null;
        mesh = null;
    }

    public Line(float startX, float startY, float endX, float endY) {
        /*
         * Create a line with the give start and end positions. Calculate the starting position.
         */

        width = -1;
        start = new Vector2(startX, startY);
        end = new Vector2(endX, endY);
        vertices = null;
        mesh = null;
    }

    public static Line CreateLine() {
        /*
         * Create a new, empty line object
         */
         
        return new Line();
    }

    #endregion


    #region Mesh Creation Functions  --------------------------------------------------------- */

    public void GenerateVertices() {
        /*
         * Generate the vertices and their positions that will be used to render the line as a mesh.
         * Requires the start, end and width values to be set. Ther vertices are placed in an order
         * so that two triangles of [0, 1, 2] and [2, 3, 0] will form the line with a proper normal.
         */
        vertices = new Vector3[4];
        Vector2 lineWidth = Vector2.zero;

        /* Apply the line's width to either the Y or X axis, depending on what axis the line is on */
        if(start.x == end.x) {
            lineWidth = new Vector2(width/2f, 0);
            /* Flip the mesh if needed */
            if(start.y < end.y) {
                lineWidth *= -1;
            }
        }
        else if(start.y == end.y) {
            lineWidth = new Vector2(0, width/2f);
            /* Flip the mesh if needed */
            if(start.x > end.x) {
                lineWidth *= -1;
            }
        }
        else {
            Debug.Log("WARNING: Line is not directly horizontal or vertical");
        }

        /* Set the vertices using the line's extra width */
        vertices[0] = new Vector2(end.x, end.y) - lineWidth;
        vertices[1] = new Vector2(end.x, end.y) + lineWidth;
        vertices[2] = new Vector2(start.x, start.y) + lineWidth;
        vertices[3] = new Vector2(start.x, start.y) - lineWidth;
    }

    public void GenerateMesh() {
        /*
         * Generate the mesh used to represent the line. 
         */
        mesh = new Mesh();

        /* Set the triangles of the mesh */
        int[] triangles = { 0, 1, 2, 2, 3, 0 };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    #endregion
}
