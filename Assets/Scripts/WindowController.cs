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
    public GameController gameController;
    #endregion

    /* Main camera */
    public Camera mainCamera;

    /* Resolution variables */
    public ResolutionMode currentResolutionMode = ResolutionMode.NULL;
    public float windowWidth = -1;
    public float windowHeight = -1;
    //How many pixels the screen is zoomed out. Used to ensure the edge lines are fully visible
    public float edgeBufferSize = 10;

    #endregion


    #region Built-In Unity Functions  --------------------------------------------------------- */

    void Start () {
        /*
         * Setup the window and it's values, then create a game
         */
        
        /* Save the current window size */
        UpdateSavedWindowResolution();

        /* Start the game in the given rendering mode */
        ChangeCurrentWindowResolutionMode(ResolutionMode.Stretch);

        /* Create a basic game area once the window is setup */
        gameController = new GameController(100, 100, this);
    }

    void Update() {

        /* Catch when the window's resolution does not match the script's saved resolution */
        if(windowWidth != Screen.width || windowHeight != Screen.height) {
            UpdateSavedWindowResolution();

            /* Update how the lines are rendered with this new window size */
            gameController.UpdateLineVertices();
        }


        /* Update the game to the next step */
        gameController.UpdateGame();
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
            if(gameController!= null) {
                gameController.UpdateLineVertices();
            }
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
