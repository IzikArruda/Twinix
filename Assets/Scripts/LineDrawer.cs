using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Sends requests directly to the graphics class to draw meshes created to look like lines.
 * A line drawer is linked to a GamesController and will draw all the components used by it.
 */
public class LineDrawer {

    #region Variables  --------------------------------------------------------- */

    #region Linked Scripts
    private GameController gameController;
    #endregion

    /* Empty quaternion to use for each line's rotation */
    private Quaternion lineQuat;

    /* The list of lines, corners and players that need to be drawn */
    private List<Line> lines;
    private List<Corner> corners;
    private List<Player> players;

    /* Material used for each line rendered */
    public Material lineMaterial;

    #endregion


    #region Constructors ------------------------------------------------------ */

    public LineDrawer(GameController gameCont) {
        /*
         * Initialize the lines and corner lists and some variables used by this script
         */
        lines = new List<Line>();
        corners = new List<Corner>();
        players = new List<Player>();
        lineQuat = Quaternion.Euler(0, 0, 0);
        lineMaterial = null;
        gameController = gameCont;
    }

    #endregion

    
    #region Outside Called Functions ------------------------------------------------------------- */

    public void NewGameArea(List<Line> edges, Dictionary<Vector3, Corner>.ValueCollection edgeCorner) {
        /*
         * A new game area is set up, so save it's sizes and add it's edges/corners to be rendered
         */

        for(int i = 0; i < edges.Count; i++) {
            lines.Add(edges[i]);
        }

        foreach(Corner corner in edgeCorner) {
            corners.Add(corner);
        }
        for(int i = 0; i < edgeCorner.Count; i++) {
            //
        }
    }

    public void AddPlayers(Player[] playersToAdd) {
        /*
         * Add the given array of players to the list of players to render
         */

        for(int i = 0; i < playersToAdd.Length; i++) {
            players.Add(playersToAdd[i]);
        }
    }

    public void UpdateLineVertices() {
        /*
         * Called when the lines need to reset their vertices due to a change 
         * in the window size or resolution rendering method.
         */

        if(lines != null) {
            foreach(Line line in lines) {
                line.GenerateVertices(gameController);
                line.GenerateMesh();
            }
        }
    }

    #endregion


    #region Drawing Functions ------------------------------------------------------------- */

    public void DrawAll() {
        /*
         * Draw all the required objects
         */

        for(int i = 0; i < lines.Count; i++) {
            DrawLine(lines[i]);
        }
        for(int i = 0; i < corners.Count; i++) {
            DrawCorner(corners[i]);
        }
        for(int i = 0; i < players.Count; i++) {
            DrawPlayer(players[i]);
        }

    }

    private void DrawLine(Line line) {
        /*
         * Draw the given line onto the screen using the graphics class.
         * Alter the line's size to adjust relative to the window's resolution.
         */
        
        /* Draw the mesh of the line */
        Graphics.DrawMesh(line.mesh, Vector3.zero, lineQuat, lineMaterial, 0);
    }

    private void DrawCorner(Corner corner) {
        /*
         * Draw the corner using the debug.drawLine function for now 
         * as corners will require a bit of complexity to properly render
         */

        /* 
         * To draw a corner, take each side which has a line connected to it and draw a line from their edge
         */
        if(corner.up != null) { DrawCornerPart(corner, corner.up, Vector2.up); }
        if(corner.right != null) { DrawCornerPart(corner, corner.right, Vector2.right); }
        if(corner.down != null) { DrawCornerPart(corner, corner.down, Vector2.down); }
        if(corner.left != null) { DrawCornerPart(corner, corner.left, Vector2.left); }
    }

    private void DrawCornerPart(Corner corner, Line cornerLine, Vector2 lineDirection) {
        /*
         * Draw a part of the corner using the given line and the direction of the line.
         */
        Vector2 vert1, vert2, center;
        float lineWidth;
        
        /* Get the start or end vertices of the line, depending on which one's linked to the corner */
        if(cornerLine.end.Equals(corner.position)) {
            vert1 = cornerLine.vertices[0];
            vert2 = cornerLine.vertices[1];
        }
        else {
            vert1 = cornerLine.vertices[2];
            vert2 = cornerLine.vertices[3];
        }

        /* Get the width of the line and offset the vert positions */
        lineWidth = (vert1 - vert2).magnitude;
        vert1 += lineDirection*lineWidth;
        vert2 += lineDirection*lineWidth;

        center = (vert1 + vert2)/2f - lineDirection*lineWidth/2f;
        Debug.DrawLine(vert1, vert2);
        Debug.DrawLine(center, vert1);
        Debug.DrawLine(center, vert2);
    }
    
    private void DrawPlayer(Player player) {
        /*
         * Render the player's sprite using thier game position converted to the screen.
         */
        Vector3 playerPos = gameController.GameToScreenPos(player.gamePosition);

        Debug.DrawLine(playerPos + Vector3.up, playerPos + Vector3.down);
        Debug.DrawLine(playerPos + Vector3.left, playerPos + Vector3.right);

        /* Set the size of the player's sprite */
        player.SetSpritesize(20);

        /* Place the player's sprite at the given player position */
        player.SetSpritePositon(playerPos);
    }

    #endregion
}
