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
        
        /* Place the players onto the edges */
        for(int i = 0; i < players.Length; i++) {
            players[i].SetStartingLine(edges[3], 0.5f);
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






            /* Move the player up their line */
            //Pressing up will cause this order:
            //1. Check how much distance is between the players current position and the corner up the line
            //IDEA: Get a function for a line which returns the distance from the given position towards a given direction until it hits a corner
            //thius function can be used to help with recursive calls once we hit a corner

            //Check if the player is on their line
            if(i == 0) {
                if(players[i].currentLine.IsPointOnLine(players[i].gamePosition)) {
                    Debug.Log("Player is on the line");
                }
                else {
                    Debug.Log("Player is off the line");
                }

                //Print the distance that the player is from the top side
                Debug.Log(players[i].currentLine.DistanceToCornerFrom(players[i].gamePosition, OrthogonalDirection.Up));
            }





            Vector3 movement = Vector2.zero;
            float distance = 0;
            /* Move the player up */
            if(players[i].controls.up) {
                /* Get the distance the player can travel this frame */
                distance = Mathf.Min(players[i].defaultMovementSpeed, players[i].currentLine.DistanceToCornerFrom(players[i].gamePosition, OrthogonalDirection.Up));

                /* Get the direction the player will travel */
                movement = LineCorner.DirectionToVector(OrthogonalDirection.Up);

                /* Apply the movement to the player's position */
                players[i].SetPlayerPosition(players[i].gamePosition + movement*distance);
            }

            /* Move the player right */
            if(players[i].controls.right) {
                /* Get the distance the player can travel this frame */
                distance = Mathf.Min(players[i].defaultMovementSpeed, players[i].currentLine.DistanceToCornerFrom(players[i].gamePosition, OrthogonalDirection.Right));

                /* Get the direction the player will travel */
                movement = LineCorner.DirectionToVector(OrthogonalDirection.Right);

                /* Apply the movement to the player's position */
                players[i].SetPlayerPosition(players[i].gamePosition + movement*distance);
            }

            /* Move the player down */
            if(players[i].controls.down) {
                /* Get the distance the player can travel this frame */
                distance = Mathf.Min(players[i].defaultMovementSpeed, players[i].currentLine.DistanceToCornerFrom(players[i].gamePosition, OrthogonalDirection.Down));

                /* Get the direction the player will travel */
                movement = LineCorner.DirectionToVector(OrthogonalDirection.Down);

                /* Apply the movement to the player's position */
                players[i].SetPlayerPosition(players[i].gamePosition + movement*distance);
            }

            /* Move the player left */
            if(players[i].controls.left) {
                /* Get the distance the player can travel this frame */
                distance = Mathf.Min(players[i].defaultMovementSpeed, players[i].currentLine.DistanceToCornerFrom(players[i].gamePosition, OrthogonalDirection.Left));

                /* Get the direction the player will travel */
                movement = LineCorner.DirectionToVector(OrthogonalDirection.Left);

                /* Apply the movement to the player's position */
                players[i].SetPlayerPosition(players[i].gamePosition + movement*distance);
            }
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
