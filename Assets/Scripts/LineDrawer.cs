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

    /* Screen resolution values */
    private ResolutionMode resolutionMode;
    private float windowWidth;
    private float windowHeight;

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

    public void SetResolutionMode(ResolutionMode newMode) {
        /*
         * Set the current resolution mode used by the renderer
         */
        resolutionMode = newMode;
    }

    public void SetWindowResolution(float height, float width) {
        /*
         * Set the saved window resolution to the given sizes
         */
        windowHeight = height;
        windowWidth = width;
    }

    #endregion


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


    #region Drawing Functions ------------------------------------------------------------- */

    private void DrawLine(Line line) {
        /*
         * Draw the given line onto the screen using the graphics class.
         * Alter the line's size to adjust relative to the window's resolution.
         */
        Vector2 centerOffset = new Vector2(gameAreaX/2f, gameAreaY/2f);

        /* Don't adjust the size of the line, just set the offset to center it */
        if(resolutionMode == ResolutionMode.True) {
            centerOffset = new Vector2(gameAreaX/2f, gameAreaY/2f);
        }

        /* The resolution mode calls for the line to change it's size relative to the screen */
        if(resolutionMode == ResolutionMode.Stretch) {
            float heightRatio = windowHeight/gameAreaY;
            float widthRatio = windowWidth/gameAreaX;  
            centerOffset = new Vector2(widthRatio*gameAreaX/2f, heightRatio*gameAreaY/2f);
        }

        /* Stretch to fit the screen while keeping the ratio intact */
        else if(resolutionMode == ResolutionMode.TrueRatioStretch) {
            float ratio = Mathf.Min(windowHeight/gameAreaY, windowWidth/gameAreaX);
            centerOffset = new Vector2(ratio*gameAreaX/2f, ratio*gameAreaY/2f);
        }
        
        /* Draw the mesh of the line */
        Graphics.DrawMesh(line.mesh, -centerOffset, lineQuat, lineMaterial, 0);
    }

    #endregion
}
