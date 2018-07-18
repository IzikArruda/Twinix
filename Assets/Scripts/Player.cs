using UnityEngine;
using System.Collections;

/*
 * The player object which contains 
 */
public class Player {

    #region Variables  --------------------------------------------------------- */

    public PlayerControls controls;

    #endregion


    #region Constructors --------------------------------------------------------- */

    public Player() {
        /*
         * Create the player and their controls
         */

        controls = new PlayerControls();
    }

    #endregion
}
