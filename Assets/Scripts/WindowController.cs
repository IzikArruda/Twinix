using UnityEngine;
using System.Collections;

/* 
 * How the game is rendered relative to the window size.
 * 
 * True: The window's resolution does not effect how the game is displayed
 * Stretch: The game is stretched to ensure it covers the entire window's view
 * TrueRatioStretch: Retain the ratio of the game, but stretch for it to fit the window
 */
public enum ResolutionMode {
    NULL,
    True,
    Stretch,
    TrueRatioStretch
};

/*
 * Handle how the game reacts to window size changes
 */
public class WindowController : MonoBehaviour {

    #region Variables  --------------------------------------------------------- */

    #region Linked Scripts
    public LineDrawer lineDrawer;
    #endregion

    /* Main camera */
    public Camera mainCamera;

    /* Resolution variables */
    public static ResolutionMode currentResolutionMode = ResolutionMode.NULL;
    public static float windowWidth = -1;
    public static float windowHeight = -1;
    //How many pixels the screen is zoomed out. Used to ensure the edge lines are fully visible
    public static float edgeBufferSize = 10;

    #endregion


    #region Built-In Unity Functions  --------------------------------------------------------- */

    void Start () {

        /* Save the current window size */
        UpdateSavedWindowResolution();

        /* Start the game in the given rendering mode */
        ChangeCurrentWindowResolutionMode(ResolutionMode.Stretch);
    }

    void Update() {

        /* Catch when the window's resolution does not match the script's saved resolution */
        if(windowWidth != Screen.width || windowHeight != Screen.height) {
            UpdateSavedWindowResolution();

            /* Update how the lines are rendered with this new window size */
            lineDrawer.UpdateLineVertices();
        }
    }

    #endregion
    

    #region Resolution Functions ------------------------------------------------------- */

    private void ChangeCurrentWindowResolutionMode(ResolutionMode newMode) {
        /*
         * Change the current resolution mode to the given newMode. If we succesfully change 
         * the mode, inform the lineRenderer that the rendering mode has changed.
         */
         
        if(newMode != currentResolutionMode && newMode != ResolutionMode.NULL) {
            currentResolutionMode = newMode;

            /* Update how the lines are rendered with this new window size */
            lineDrawer.UpdateLineVertices();
        }
    }
    
    private void UpdateSavedWindowResolution() {
        /*
         * Called when the window's resolution has been changed.
         * Update the script's saved window sizes to reflect the new size
         */

        windowHeight = Screen.height;
        windowWidth = Screen.width;
        mainCamera.orthographicSize = windowHeight/2f;
    }

    #endregion
}
