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
}
