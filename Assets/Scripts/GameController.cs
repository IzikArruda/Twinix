using UnityEngine;
using System.Collections;

/*
 * Control how the game plays and how the play area is initialized.
 * 
 * A game area is meassured in pixels using its gameArea values.
 */
public class GameController {

    #region Variables  --------------------------------------------------------- */

    #region Linked Scripts
    private LineDrawer lineDrawer;
    public WindowController windowController;
    #endregion

    /* How long the game has been running for */
    private float tick = 0;

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
    private Player[] players;

    #endregion


    #region Constructors --------------------------------------------------------- */

    public GameController(float width, float height, WindowController linkedWindow) {
        /*
         * Create a new game with the given sizes
         */
         
        gameAreaX = width;
        gameAreaY = height;
        windowController = linkedWindow;

        /* Create the lineDrawer for this game */
        lineDrawer = new LineDrawer(this);

        /* run setup functions for the game */
        SetupPlayers();
        SetupGameArea();
    }

    #endregion
    
    
    #region Setup Functions  --------------------------------------------------------- */
    
    public void SetupPlayers() {
        /*
         * Initialize the players that will be used in the game
         */
        KeyCode[][] defaultKeyCodes = new KeyCode[2][];

        /* Create the players if they have not yet been created */
        if(players == null) { players = new Player[playerCount]; }
        for(int i = 0; i < players.Length; i++) {
            if(players[i] == null) {
                players[i] = new Player();
            }
        }

        /* Set the default controls for the player's controls */
        defaultKeyCodes[0] = new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.S, KeyCode.A, KeyCode.Space };
        defaultKeyCodes[1] = new KeyCode[] { KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightControl };

        /* Assign each player a set of default keys */
        for(int i = 0; i < Mathf.Min(defaultKeyCodes.Length, playerCount); i++) {
            /* Assign the movement keys to the player */
            players[i].controls.SetMovementKeys(defaultKeyCodes[i][0], defaultKeyCodes[i][1], defaultKeyCodes[i][2], defaultKeyCodes[i][3]);

            /* Assign the extra buttons to the player */
            for(int j = 4; j < defaultKeyCodes[i].Length; j++) {
                players[i].controls.SetExtraButtonKey(j-4, defaultKeyCodes[i][j]);
            }
        }
    }
    
    public void SetupGameArea() {
        /*
         * Create the lines and corners that make up the edges of the game area.
         */

        /* Create the edges of the game area that cover each edge of the screen */
        edges = new Line[4];
        edges[0] = new Line(0, gameAreaY, gameAreaX, gameAreaY);
        edges[1] = new Line(gameAreaX, gameAreaY, gameAreaX, 0);
        edges[2] = new Line(gameAreaX, 0, 0, 0);
        edges[3] = new Line(0, 0, 0, gameAreaY);

        /* Set the width and Initialize the meshes for each edge lines */
        foreach(Line edge in edges) {
            edge.width = lineWidth;
            edge.GenerateVertices(this);
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

        
        /* Give the lineDrawer the new edges and corners of the game area */
        lineDrawer.NewGameArea(edges, corners);
    }

    #endregion


    #region LineDrawer Functions  --------------------------------------------------------- */

    public void UpdateLineVertices() {
        /*
         * Recalculate the vertices that form the game area
         */

        if(lineDrawer!= null) {
            lineDrawer.UpdateLineVertices();
        }
    }

    #endregion


    #region Game Update Functions  --------------------------------------------------------- */

    public void UpdateGame() {
        /*
         * The main call to update the game from the current state to the next
         */

        /* Update the player inputs */
        UpdatePlayerInputs();

        /* Debug to see if the inputs are working */
        if(players[0].controls.up) {
            Debug.Log("P1 up is pressed");
        }
        if(players[1].controls.up) {
            Debug.Log("P2 up is pressed");
        }

        /* Draw the objects to the screen */
        lineDrawer.DrawAll();
        tick++;
    }

    private void UpdatePlayerInputs() {
        /*
         * Update the inputs of each playerController used in this game
         */

        for(int i = 0; i < players.Length; i++) {
            players[i].controls.UpdateInputs();
        }
    }

    #endregion

    
    #region Helper Functions  --------------------------------------------------------- */

    public Vector2 GameToScreenPos(Vector2 gamePosition) {
        /*
         * Given a position in the game area, return a vector of it converted
         * to the proper position for the screen relative to the render method.
         */
        Vector2 screenPos = Vector2.zero;
        Vector2 centerOffset = Vector2.zero;
        float heightRatio = 1;
        float widthRatio = 1;
        
        /* Get values from the windowController that control how the game is rendered */
        float windowHeight = windowController.windowHeight - windowController.edgeBufferSize;
        float windowWidth = windowController.windowWidth - windowController.edgeBufferSize;
        
        /* Don't adjust the size of the line, just set the offset to center it */
        if(windowController.currentResolutionMode == ResolutionMode.True) {
            centerOffset = new Vector2(gameAreaX/2f, gameAreaY/2f);
        }
        /* Call for the line to stretch it's size relative to the screen's width and height */
        else if(windowController.currentResolutionMode == ResolutionMode.Stretch) {
            heightRatio = windowHeight/gameAreaY;
            widthRatio = windowWidth/gameAreaX;
            centerOffset = new Vector2(widthRatio*gameAreaX/2f, heightRatio*gameAreaY/2f);
        }
        /* Stretch to fit the screen while keeping the ratio intact */
        else if(windowController.currentResolutionMode == ResolutionMode.TrueRatioStretch) {
            float ratio = Mathf.Min(windowHeight/gameAreaY, windowWidth/gameAreaX);
            heightRatio = ratio;
            widthRatio = ratio;
            centerOffset = new Vector2(ratio*gameAreaX/2f, ratio*gameAreaY/2f);
        }
        /* Print an error if we are currently in a rendering mode not handled */
        else {
            Debug.Log("WARNING: Resolution mode " + windowController.currentResolutionMode + " is not handled by the line");
        }

        /* Update the given game position to be a position relative to the screen */
        screenPos = new Vector2(widthRatio*gamePosition.x, heightRatio*gamePosition.y) - centerOffset;

        return screenPos;
    }

    #endregion
}
