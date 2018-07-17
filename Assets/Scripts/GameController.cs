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

    /* The edges of the game area */
    private Line[] edges;

    /* Game state */
    private bool gameStarted = false;

    #endregion

    
    #region Built-In Unity Functions ------------------------------------------------------ */

    void Update() {
        /*
         * Initialize the game area if it has not yet started
         */

        if(!gameStarted) {
            InitializeGameArea();
            gameStarted = true;
        }
    }

    #endregion


    #region Game Area Functions  --------------------------------------------------------- */

    public void InitializeGameArea() {
        /*
         * Create the edges of the game area
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

        
        /* Set up the line drawer to render the game area properly */
        lineDrawer.NewGameArea(gameAreaX, gameAreaY, edges);
    }

    #endregion
}
