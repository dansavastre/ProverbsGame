using System.Collections;
using System.Collections.Generic;

public class Player
{
    public string playerName;
    public string email;

    /// <summary>
    /// Constructor for the Player class.
    /// </summary>
    /// <param name="playerName">string denoting the name of the player</param>
    /// <param name="email">string denoting the email of the player</param>
    public Player(string playerName, string email)
    {
        this.playerName = playerName;
        this.email = email;
    }

    /// <summary>
    /// Checks the equality of the Player object with another object.
    /// </summary>
    /// <param name="obj">the object that this Player is compared to</param>
    /// <returns>whether or not the 2 objects are equal</returns>
    public override bool Equals(object obj) 
    {
        var player = obj as Player;

        if (player == null)
            return false;
        if (player == this)
            return true;

        return this.playerName == player.playerName && this.email == player.email;
    }

    /// <summary>
    /// Generates hash code for a Player object.
    /// </summary>
    /// <returns>integer denoting the hash code for the Player object</returns>
    public override int GetHashCode()
    {
        return this.playerName.GetHashCode() * 17 + this.email.GetHashCode();
    }
}