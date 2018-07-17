using UnityEngine;
using System.Collections;

/*
 * Control how the game plays and how the play area is initialized.
 * 
 * A game area is meassured in pixels using its gameArea values.
 */
public class GameController : MonoBehaviour {

    #region Variables  --------------------------------------------------------- */

    #region Linked Scripts
    public LineDrawer lineDrawer;
    #endregion

    /* The sizes of the game area */
    private float gameAreaX;
    private float gameAreaY;

    /* The width of the lines that are created in this game controller */
    private float lineWidth = 5;

    /* The edges and corners of the game area */
    private Line[] edges;
    private LineCorner[] corners;

    /* Game state */
    private bool gameStarted = false;

    /* The player controllers that will be used in the game */
    private static int playerCount = 2;
    private PlayerController[] playerControllers;

    #endregion


    #region Built-In Unity Functions ------------------------------------------------------ */

    void Update() {
        /*
         * Initialize the game area if it has not yet started
         */

        if(!gameStarted) {
            SetupPlayerControllers();
            SetupGameArea();
            gameStarted = true;
        }

        /* Main game update call */
        UpdateGame();
    }

    #endregion


    #region Setup Functions  --------------------------------------------------------- */

    public void SetupPlayerControllers() {
        /*
         * Initialize the playerControllers that will be used in the game
         */
        KeyCode[][] defaultKeyCodes = new KeyCode[2][];

        /* Create the controllers if they have not yet been created */
        if(playerControllers == null) { playerControllers = new PlayerController[playerCount]; }
        for(int i = 0; i < playerControllers.Length; i++) {
            if(playerControllers[i] == null) {
                playerControllers[i] = new PlayerController();
            }
        }

        /* Set the default controls for the player controllers */
        defaultKeyCodes[0] = new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.S, KeyCode.A, KeyCode.Space };
        defaultKeyCodes[1] = new KeyCode[] { KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightControl };

        /* Assign each player a set of default keys */
        for(int i = 0; i < Mathf.Min(defaultKeyCodes.Length, playerCount); i++) {
            /* Assign the movement keys to the player */
            playerControllers[i].SetMovementKeys(defaultKeyCodes[i][0], defaultKeyCodes[i][1], defaultKeyCodes[i][2], defaultKeyCodes[i][3]);

            /* Assign the extra buttons to the player */
            for(int j = 4; j < defaultKeyCodes[i].Length; j++) {
                playerControllers[i].SetExtraButtonKey(j-4, defaultKeyCodes[i][j]);
            }
        }
    }
    
    public void SetupGameArea() {
        /*
         * Create the lines and corners that make up the edges of the game area.
         */

        /* Set the sizes of the game area */
        gameAreaX = 100;
        gameAreaY = 100;

        /* Create the edges of the game area that cover each edge of the screen */
        edges = new Line[4];
        edges[0] = new Line(0, gameAreaY, gameAreaX, gameAreaY);
        edges[1] = new Line(gameAreaX, gameAreaY, gameAreaX, 0);
        edges[2] = new Line(gameAreaX, 0, 0, 0);
        edges[3] = new Line(0, 0, 0, gameAreaY);

        /* Set the width and Initialize the meshes for each edge lines */
        foreach(Line edge in edges) {
            edge.width = lineWidth;
            edge.GenerateVertices(gameAreaX, gameAreaY);
            edge.GenerateMesh();
        }

        /* Set the corners of the game area. This assumes each edge is set up to connect in order */
        corners = new LineCorner[edges.Length];
        for(int i = 0; i < corners.Length; i++) {
            corners[i] = new LineCorner(edges[i].start);
        }

        /* Link the edges to the corners */
        corners[0].AddLine(edges[0]);
        corners[0].AddLine(edges[edges.Length-1]);
        for(int i = 1; i < corners.Length; i++) {
            corners[i].AddLine(edges[i]);
            corners[i].AddLine(edges[i-1]);
        }
        

        /* Set up the line drawer to render the game area properly */
        lineDrawer.NewGameArea(gameAreaX, gameAreaY, edges, corners);
    }

    #endregion



    #region Game Update Functions  --------------------------------------------------------- */

    private void UpdateGame() {
        /*
         * The main call to update the game from the current state.
         */

        /* Update the player inputs */
        UpdatePlayerInputs();

        /* Debug to sere if the inputs are working */
        if(playerControllers[0].up) {
            Debug.Log("P1 up is pressed");
        }
        if(playerControllers[1].up) {
            Debug.Log("P2 up is pressed");
        }
    }

    private void UpdatePlayerInputs() {
        /*
         * Update the inputs of each playerController used in this game
         */

        for(int i = 0; i < playerControllers.Length; i++) {
            playerControllers[i].UpdateInputs();
        }
    }

    #endregion
}
