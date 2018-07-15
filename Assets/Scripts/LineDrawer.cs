using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * The Line class that the 
 */
public class Line {
    public Vector2 start;
    public Vector2 end;
}

/*
 * Sends requests directly to the graphics class to draw meshes created to look like lines.
 */
public class LineDrawer : MonoBehaviour {
    #region Variables  --------------------------------------------------------- */

    /* Screen resolution values */
    private ResolutionMode resolutionMode;
    private float windowWidth;
    private float windowHeight;

    /* The list of lines that need to be drawn */
    private List<Line> lines;

    /* Game area sizes */
    private float gameAreaX;
    private float gameAreaY;

    #endregion


    #region Built-In Unity Functions ------------------------------------------------------ */

    void Start () {
        /*
         * Initialize the lines list
         */
        lines = new List<Line>();

        /* Add lines in a square. For now, we assume the "game area" is a 100x100 square, with the bottom left corner being the origin */
        gameAreaX = 100;
        gameAreaY = 100;
        Line newLine1 = new Line();
        newLine1.start = new Vector2(100, 100);
        newLine1.end = new Vector2(0, 100);
        lines.Add(newLine1);
        Line newLine2 = new Line();
        newLine2.start = new Vector2(0, 100);
        newLine2.end = new Vector2(0, 0);
        lines.Add(newLine2);
        Line newLine3 = new Line();
        newLine3.start = new Vector2(0, 0);
        newLine3.end = new Vector2(100, 0);
        lines.Add(newLine3);
        Line newLine4 = new Line();
        newLine4.start = new Vector2(100, 0);
        newLine4.end = new Vector2(100, 100);
        lines.Add(newLine4);
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


    #region Drawing Functions ------------------------------------------------------------- */

    private void DrawLine(Line line) {
        /*
         * Draw the given line onto the screen using the graphics class.
         * Alter the line's size to adjust relative to the window's resolution.
         */
        Vector2 LStart = line.start;
        Vector2 LEnd = line.end;
        Vector2 centerOffset = new Vector2(gameAreaX/2f, gameAreaY/2f);

        /* Don't adjust the size of the line, just set the offset to center it */
        if(resolutionMode == ResolutionMode.True) {
            centerOffset = new Vector2(gameAreaX/2f, gameAreaY/2f);
        }

        /* The resolution mode calls for the line to change it's size relative to the screen */
        if(resolutionMode == ResolutionMode.Stretch) {
            float heightRatio = windowHeight/gameAreaY;
            float widthRatio = windowWidth/gameAreaX;
            LStart = new Vector2(LStart.x*widthRatio, LStart.y*heightRatio);
            LEnd = new Vector2(LEnd.x*widthRatio, LEnd.y*heightRatio);     
            centerOffset = new Vector2(widthRatio*gameAreaX/2f, heightRatio*gameAreaY/2f);
        }

        /* Stretch to fit the screen while keeping the ratio intact */
        else if(resolutionMode == ResolutionMode.TrueRatioStretch) {
            float ratio = Mathf.Min(windowHeight/gameAreaY, windowWidth/gameAreaX);
            LStart = new Vector2(LStart.x*ratio, LStart.y*ratio);
            LEnd = new Vector2(LEnd.x*ratio, LEnd.y*ratio);     
            centerOffset = new Vector2(ratio*gameAreaX/2f, ratio*gameAreaY/2f);
        }


        /* Re-position the line so that it's centered in the camera */
        LStart -= centerOffset;
        LEnd -= centerOffset;
        

        //Note: for now, keep it simple and simply use debug.drawLine call
        Debug.DrawLine(LStart, LEnd);
    }

    #endregion
}
