﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float gameAreaX;
    public float gameAreaY;
    public float gridSize;

    /* The width of the lines that are created in this game controller */
    private float lineWidth = 5;

    /* The lines and corners of the game area */
    private List<Line> lines;
    private Dictionary<Vector3, Corner> corners;

    /* Game state */
    private bool gameStarted = false;

    /* The players that will be used in the game */
    private static int playerCount = 2;
    private Player[] players;

    #endregion


    #region Constructors --------------------------------------------------------- */

    public GameController(GameObject container, WindowController linkedWindow) {
        /*
         * Create a new game with the given sizes
         */

        playerContainer = container;
        windowController = linkedWindow;
        lines = new List<Line>();
        corners = new Dictionary<Vector3, Corner>();

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
                players[i] = new Player(playerContainer, this);
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

        /* Load the desired level */
        LoadLevel(0);
        
        /* Set the width and Initialize the meshes for each lines. */
        foreach(Line line in lines) {
            RefreshLine(line);
        }

        /* Place the players onto random lines */
        for(int i = 0; i < players.Length; i++) {
            players[i].SetStartingLine(lines[Mathf.FloorToInt(Random.Range(0, lines.Count-1))], Random.Range(0f, 1f));
        }
    }

    #endregion
    

    #region Level Layout Functions  --------------------------------------------------------- */
    
    private void LinkLinesAndCorners() {
        /*
         * Connect all lines to their respective corners. This is done by searching
         * the dictionairy of corners using the start and end points of the line
         * as the keys for the corners they will use.
         */

        for(int i = 0; i < lines.Count; i++) {
            /* Add the start of the line */
            if(corners.ContainsKey(lines[i].start)) {
                corners[lines[i].start].AddLine(lines[i]);
            }
            else {
                Debug.Log("WARNING: A line's start position does not reach a corner");
            }

            /* Add the end of the line */
            if(corners.ContainsKey(lines[i].end)) {
                corners[lines[i].end].AddLine(lines[i]);
            }
            else {
                Debug.Log("WARNING: A line's end position does not reach a corner");
            }
        }
    }

    private void FillArraysWithSequence(Vector3[] sequence) {
        /*
         * Given a sequence of positions, create corners on each position and 
         * connect lines between the corners that are in a sequence. A sequence
         * is sepperated by a vector containing NaN values.
         */

        /* Create and add the lines from the given sequence into their list */
        NewCorner(sequence[0]); /* Create the first corner in the first sequence */
        for(int i = 1; i < sequence.Length; i++) {

            /* Create the corner of the current position in the sequence */
            if(sequence[i].x != float.NegativeInfinity) {
                NewCorner(sequence[i]);

                /* Create a line if the previous entry in the sequence exists */
                if(sequence[i-1].x != float.NegativeInfinity) {
                    AddLine(Line.NewLine(sequence[i-1], sequence[i]));
                }
            }
        }
    }

    public void LoadLevel(int levelNumber) {
        /*
         * Load the level defined by the given level number. A level is loaded by setting 
         * the game's playArea sizes and adding the lines and corners to their respective lists.
         */

        if(levelNumber == 1) {
            LoadLevel1();
        }

        /* Load the default, 4 corner level */
        else {
            LoadDefaultLevel();
        }
    }

    public void LoadLevel1() {
        /*
         * Level 1 is a basic square with a cross(+) through the middle.
         * 
         * Create an array of positions that represent the level's layout.
         * A sequence of non-NaN values indicate a sequence of lines.
         * When a null character is encountered, begin a new sequence with the next entry.
         */
        gameAreaX = 20;
        gameAreaY = 20;
        gridSize = 0.1f;
        Vector3[] sequence = {
            new Vector3(0, 0, 0),
            new Vector3(gameAreaX/2f, 0, 0),
            new Vector3(gameAreaX, 0, 0),
            new Vector3(gameAreaX, gameAreaY/2f, 0),
            new Vector3(gameAreaX, gameAreaY, 0),
            new Vector3(gameAreaX/2f, gameAreaY, 0),
            new Vector3(0, gameAreaY, 0),
            new Vector3(0, gameAreaY/2f, 0),
            new Vector3(0, 0, 0),
            new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
            new Vector3(gameAreaX/2f, 0, 0),
            new Vector3(gameAreaX/2f, gameAreaY/2f, 0),
            new Vector3(gameAreaX/2f, gameAreaY, 0),
            new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
            new Vector3(0, gameAreaY/2f, 0),
            new Vector3(gameAreaX/2f, gameAreaY/2f, 0),
            new Vector3(gameAreaX, gameAreaY/2f, 0),
        };

        /* Use the sequence to populate the corners and lines arrays */
        FillArraysWithSequence(sequence);
        
        /* Link all the lines to their respective corners */
        LinkLinesAndCorners();
    }

    public void LoadDefaultLevel() {
        /*
         * The default level is the base QUIX level, featuring 4 corners and 4 lines.
         */
        gameAreaX = 10;
        gameAreaY = 10;
        gridSize = 1f;
        Vector3[] sequence = {
            new Vector3(0, 0, 0),
            new Vector3(gameAreaX, 0, 0),
            new Vector3(gameAreaX, gameAreaY, 0),
            new Vector3(0, gameAreaY, 0),
            new Vector3(0, 0, 0),
        };

        /* Use the sequence to populate the corners and lines arrays */
        FillArraysWithSequence(sequence);

        /* Link all the lines to their respective corners */
        LinkLinesAndCorners();
    }
    
    private void NewCorner(float x, float y) {
        /*
         * Create a corner at the given coordinates.
         */
        Vector3 cornerPosition = new Vector3(x, y, 0);

        if(!corners.ContainsKey(cornerPosition)) {
            AddCorner(Corner.NewCorner(cornerPosition));
        }
        else {
            /////Debug.Log("WARNING: Trying to create a corner ontop of another corner");
        }
    }

    private void NewCorner(Vector3 cornerPosition) {
        /*
         * Create a corner at the given position
         */

        NewCorner(cornerPosition.x, cornerPosition.y);
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
            players[i].UpdateInputs();
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


    #region Line/Corner Update Functions  --------------------------------------------------------- */

    private void RefreshLine(Line line) {
        /*
         * Re-create the line to reflect a change in it's width and size
         */

        line.width = lineWidth;
        line.GenerateVertices(this);
        line.GenerateMesh();
    }

    public void AddLineCorner(Line newLine, Corner newCorner) {
        /*
         * Run the AddLine and AddCorner functions for the given line and corner
         */

        AddLine(newLine);
        AddCorner(newCorner);
    }

    public void AddLine(Line newLine) {
        /*
         * A new line is added to the game area. Add this new line to
         * this gameController's line tracker and the lineDrawer's lines.
         */

        RefreshLine(newLine);
        lines.Add(newLine);
        lineDrawer.AddLine(newLine);
    }

    public void AddCorner(Corner newCorner) {
        /*
         * A new corner is added to the game area. Add this new corner to
         * this gameController's corner tracker and the lineDrawer's corners.
         */
         
        corners.Add(newCorner.position, newCorner);
        lineDrawer.AddCorner(newCorner);
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

    public Line DoesLineCollide(Vector3 start, Vector3 end, List<Line> linesToAvoid) {
        /*
         * Given a start and end position that define a line, return a line 
         * that collides with the given line. 
         * 
         * NOTE: The line returned is not special other than it collided with the player.
         * There could be other lines that have also collided with the given line.
         * 
         * Do not check for collisions with any of the given lines in the linesToAvoid array.
         */
        Line collidedLine = null;
        Line tempLine = Line.NewLine(start, end);

        for(int i = 0; i < lines.Count && collidedLine == null; i++) {

            if(!DoesArrayContain(linesToAvoid, lines[i])) {
                if(LineCollide(tempLine, lines[i])) {
                    collidedLine = lines[i];
                }
            }
        }

        return collidedLine;
    }

    public bool LineCollide(Line line1, Line line2) {
        /*
         * Return true if the two given lines collide with each other.
         * All lines travel across only 1 axis, so the collision detection
         * is assuming that each line only moves along one axis.
         * 
         * Include the corners as a part of the lines.
         */
        bool collide = false;
        /* Get the directions of both lines */
        OrthogonalDirection line1Dir = line1.StartToEndDirection();
        OrthogonalDirection line2Dir = line2.StartToEndDirection();
        /* Get the start to end ranges of both lines */
        float line1Min = Mathf.Min(Corner.GetVectorAxisValue(line1.start, line1Dir), Corner.GetVectorAxisValue(line1.end, line1Dir));
        float line1Max = Mathf.Max(Corner.GetVectorAxisValue(line1.start, line1Dir), Corner.GetVectorAxisValue(line1.end, line1Dir));
        float line2Min = Mathf.Min(Corner.GetVectorAxisValue(line2.start, line2Dir), Corner.GetVectorAxisValue(line2.end, line2Dir));
        float line2Max = Mathf.Max(Corner.GetVectorAxisValue(line2.start, line2Dir), Corner.GetVectorAxisValue(line2.end, line2Dir));
        /* Get the flat axis value of each line */
        float line1Flat = Corner.GetVectorAxisValue(line1.start, Corner.NextDirection(line1Dir));
        float line2Flat = Corner.GetVectorAxisValue(line2.start, Corner.NextDirection(line2Dir));
        
        /* If the lines are parallel, they intercept if... */
        if(Corner.IsDirectionsParallel(line1Dir, line2Dir)) {
            /* Both lines have the same flat value */
            if(line1Flat == line2Flat) {
                /* and both line's max > the other's min and vice versa */
                if(line1Min <= line2Max && line1Max >= line2Min) {
                    collide = true;
                }
            }
        }

        /* If they are not on the same axis, check if both's flats are within both's ranges */
        else {
            if((line1Flat >= line2Min && line1Flat <= line2Max) && (line2Flat >= line1Min && line2Flat <= line1Max)) {
                collide = true;
            }
        }

        return collide;
    }

    public bool DoesArrayContain(List<Line> linesArray, Line line) {
        /*
         * Given a list of lines and a single line, return true
         * if the given single line is found in the given array.
         */
        bool found = false;

        if(linesArray != null) {
            for(int i = 0; i < linesArray.Count && !found; i++) {
                if(linesArray[i].Equals(line)) {
                    found = true;
                }
            }
        }

        return found;
    }

    #endregion
}
