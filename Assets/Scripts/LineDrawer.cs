using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * Sends requests directly to the graphics class to draw meshes created to look like lines.
 */
public class LineDrawer : MonoBehaviour {

    #region Variables  --------------------------------------------------------- */

    /* Empty quaternion to use for each line's rotation */
    private Quaternion lineQuat;

    /* The list of lines that need to be drawn */
    private List<Line> lines;

    /* Game area sizes */
    private float gameAreaX;
    private float gameAreaY;

    /* Material used for each line rendered */
    public Material lineMaterial;

    #endregion


    #region Built-In Unity Functions ------------------------------------------------------ */

    void Start () {
        /*
         * Initialize the lines list and some variables used by this script
         */
        lines = new List<Line>();
        lineQuat = Quaternion.Euler(0, 0, 0);
    }
	
	void Update () {
        /*
         * Draw each line in the lines list onto the screen using the DrawLine function.
         * Depending on the current resolution mode, alter the lines before rendering them
         */
        Line line;
        
        for(int i = 0; i < lines.Count; i++) {
            line = lines[i];

            
            DrawLine(line);
        }
	}

    #endregion


    #region Set/Get Functions ------------------------------------------------------------- */



    #endregion


    #region Outside Called Functions ------------------------------------------------------------- */

    public void NewGameArea(float gameAreaWidth, float gameAreaHeight, Line[] edges) {
        /*
         * A new game area is set up, so save it's sizes and add it's edges to be rendered
         */

        gameAreaX = gameAreaWidth;
        gameAreaY = gameAreaHeight;

        for(int i = 0; i < edges.Length; i++) {
            lines.Add(edges[i]);
        }
    }


    public void UpdateLineVertices() {
        /*
         * Called when the lines need to reset their vertices due to a change 
         * in the window size or resolution rendering method.
         */

        if(lines != null) {
            foreach(Line line in lines) {
                line.GenerateVertices(gameAreaX, gameAreaY);
                line.GenerateMesh();
            }
        }
    }

    #endregion

    
    #region Drawing Functions ------------------------------------------------------------- */

    private void DrawLine(Line line) {
        /*
         * Draw the given line onto the screen using the graphics class.
         * Alter the line's size to adjust relative to the window's resolution.
         */
        
        /* Draw the mesh of the line */
        Graphics.DrawMesh(line.mesh, Vector3.zero, lineQuat, lineMaterial, 0);
    }

    #endregion
}
