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

    /* The gameObject that will hold the players in the game */
    private GameObject playerContainer;

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

    /* The players that will be used in the game */
    private static int playerCount = 2;
    private Player[] players;

    #endregion


    #region Constructors --------------------------------------------------------- */

    public GameController(GameObject container, float width, float height, WindowController linkedWindow) {
        /*
         * Create a new game with the given sizes
         */

        playerContainer = container;
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

        /* Create the players if they have not yet been created */
        if(players == null) { players = new Player[playerCount]; }
        for(int i = 0; i < players.Length; i++) {
            if(players[i] == null) {
                players[i] = new Player(playerContainer);
            }
        }

        /* Set the default controls for the each player */
        for(int i = 0; i < players.Length; i++) {
            players[i].SetupControls(i);
        }

        /* Set the sprites used by the players */
        for(int i = 0; i < players.Length; i++) {
            players[i].SetupSprite(windowController.GetSprite(i));
        }

        /* Add the players to the lineDrawer so they will render */
        lineDrawer.AddPlayers(players);
    }
    
    public void SetupGameArea() {
        /*
         * Create the lines and corners that make up the edges of the game area.
         */

        /* Create the edges of the game area that cover each edge of the screen */
        edges = new Line[13];
        edges[0] = new Line(0, 0, gameAreaX/2f, 0);
        edges[1] = new Line(gameAreaX/2f, 0, gameAreaX, 0);
        edges[2] = new Line(0, 0, 0, gameAreaY/2f);
        edges[3] = new Line(gameAreaX/2f, 0, gameAreaX/2f, 0.5f);
        edges[4] = new Line(gameAreaX, 0, gameAreaX, gameAreaY/2f);
        edges[5] = new Line(0, gameAreaY/2f, gameAreaX/2f, gameAreaY/2f);
        edges[6] = new Line(gameAreaX/2f, gameAreaY/2f, gameAreaX, gameAreaY/2f);
        edges[7] = new Line(0, gameAreaY/2f, 0, gameAreaY);
        edges[8] = new Line(gameAreaX/2f, gameAreaY/2f, gameAreaX/2f, gameAreaY);
        edges[9] = new Line(gameAreaX, gameAreaY/2f, gameAreaX, gameAreaY);
        edges[10] = new Line(0, gameAreaY, gameAreaX/2f, gameAreaY);
        edges[11] = new Line(gameAreaX/2f, gameAreaY, gameAreaX, gameAreaY);
        //extra bottom line
        edges[12] = new Line(gameAreaX/2f, 0.5f, gameAreaX/2f + 5, 0.5f);
        
        /* Create the corners of the game area which connect the edges */
        corners = new LineCorner[11];
        corners[0] = new LineCorner(new Vector2(0, 0));
        corners[1] = new LineCorner(new Vector2(gameAreaX/2f, 0));
        corners[2] = new LineCorner(new Vector2(gameAreaX, 0));
        corners[3] = new LineCorner(new Vector2(0, gameAreaY/2f));
        corners[4] = new LineCorner(new Vector2(gameAreaX/2f, gameAreaY/2f));
        corners[5] = new LineCorner(new Vector2(gameAreaX, gameAreaY/2f));
        corners[6] = new LineCorner(new Vector2(0, gameAreaY));
        corners[7] = new LineCorner(new Vector2(gameAreaX/2f, gameAreaY));
        corners[8] = new LineCorner(new Vector2(gameAreaX, gameAreaY));
        //bottom extra corner
        corners[9] = new LineCorner(new Vector2(gameAreaX/2f, 0.5f));
        corners[10] = new LineCorner(new Vector2(gameAreaX/2f + 5, 0.5f));

        /* Link the edges to the corners */
        corners[0].AddLine(edges[0]);
        corners[0].AddLine(edges[2]);
        corners[1].AddLine(edges[0]);
        corners[1].AddLine(edges[1]);
        corners[1].AddLine(edges[3]);
        corners[2].AddLine(edges[1]);
        corners[2].AddLine(edges[4]);
        corners[3].AddLine(edges[2]);
        corners[3].AddLine(edges[5]);
        corners[3].AddLine(edges[7]);
        corners[4].AddLine(edges[3]);
        corners[4].AddLine(edges[5]);
        corners[4].AddLine(edges[6]);
        corners[4].AddLine(edges[8]);
        corners[5].AddLine(edges[4]);
        corners[5].AddLine(edges[6]);
        corners[5].AddLine(edges[9]);
        corners[6].AddLine(edges[7]);
        corners[6].AddLine(edges[10]);
        corners[7].AddLine(edges[8]);
        corners[7].AddLine(edges[10]);
        corners[7].AddLine(edges[11]);
        corners[8].AddLine(edges[9]);
        corners[8].AddLine(edges[11]);
        //bottom extra corner
        corners[9].AddLine(edges[3]);
        corners[9].AddLine(edges[12]);
        corners[10].AddLine(edges[12]);

        /* Set the width and Initialize the meshes for each edge lines. */
        foreach(Line edge in edges) {
            edge.width = lineWidth;
            edge.GenerateVertices(this);
            edge.GenerateMesh();
        }

        /* Place the players onto their edges */
        for(int i = 0; i < players.Length; i++) {
            if(i == 0) {
                players[i].SetStartingLine(edges[3], 0.5f);
            }
            else {
                players[i].SetStartingLine(edges[12], 0.5f);
            }
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

        /* Move the player depending on their inputted direction */
        UpdatePlayerPositions();

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

    private void UpdatePlayerPositions() {
        /*
         * Look at the inputs of each player and update their position relative
         * to the directions they are inputting.
         */

        for(int i = 0; i < players.Length; i++) {
            
            /*
             * Check the player's inputs and send a request to move the player
             */
            /* Get the amount of distance the player will travel */
            float travelDistance = players[i].defaultMovementSpeed*Time.deltaTime;

            /* Get the two directions the player has as inputs */
            OrthogonalDirection primaryDirection = players[i].controls.GetPrimaryInput();
            OrthogonalDirection secondairyDirection = players[i].controls.GetSecondairyInput();
            /* The player has given a direction and a distance */
            if((primaryDirection != OrthogonalDirection.NULL || secondairyDirection != OrthogonalDirection.NULL) && travelDistance > 0) {

                /* Request the player to commit to the given movement */
                players[i].MovePlayerRequest(primaryDirection, secondairyDirection, ref travelDistance);
                //Debug.Log("remaining distance: " + travelDistance);
            }
        }
    }

    #endregion


    #region Helper Functions  --------------------------------------------------------- */

    public Vector3 GameToScreenPos(Vector3 gamePosition) {
        /*
         * Given a position in the game area, return a vector of it converted
         * to the proper position for the screen relative to the render method.
         */
        Vector3 screenPos = Vector3.zero;
        Vector3 centerOffset = Vector3.zero;
        float heightRatio = 1;
        float widthRatio = 1;
        
        /* Get values from the windowController that control how the game is rendered */
        float windowHeight = windowController.windowHeight - windowController.edgeBufferSize;
        float windowWidth = windowController.windowWidth - windowController.edgeBufferSize;
        
        /* Don't adjust the size of the line, just set the offset to center it */
        if(windowController.currentResolutionMode == ResolutionMode.True) {
            centerOffset = new Vector3(gameAreaX/2f, gameAreaY/2f);
        }
        /* Call for the line to stretch it's size relative to the screen's width and height */
        else if(windowController.currentResolutionMode == ResolutionMode.Stretch) {
            heightRatio = windowHeight/gameAreaY;
            widthRatio = windowWidth/gameAreaX;
            centerOffset = new Vector3(widthRatio*gameAreaX/2f, heightRatio*gameAreaY/2f);
        }
        /* Stretch to fit the screen while keeping the ratio intact */
        else if(windowController.currentResolutionMode == ResolutionMode.TrueRatioStretch) {
            float ratio = Mathf.Min(windowHeight/gameAreaY, windowWidth/gameAreaX);
            heightRatio = ratio;
            widthRatio = ratio;
            centerOffset = new Vector3(ratio*gameAreaX/2f, ratio*gameAreaY/2f);
        }
        /* Print an error if we are currently in a rendering mode not handled */
        else {
            Debug.Log("WARNING: Resolution mode " + windowController.currentResolutionMode + " is not handled by the line");
        }

        /* Update the given game position to be a position relative to the screen */
        screenPos = new Vector3(widthRatio*gamePosition.x, heightRatio*gamePosition.y) - centerOffset;

        return screenPos;
    }

    #endregion
}
