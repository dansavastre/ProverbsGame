using System.Collections;
using System.Collections.Generic;

public class Player
{
    public string playerName;
    public string email;

    public Player(string playerName, string email)
    {
        this.playerName = playerName;
        this.email = email;
    }

    // Checks the equality of the Player object with another object
    public override bool Equals(object obj) 
    {
        var player = obj as Player;

        if (player == null)
            return false;
        if (player == this)
            return true;

        return this.playerName == player.playerName && this.email == player.email;
    }

    // Generates hashcode for a Player object
    public override int GetHashCode()
    {
        return this.playerName.GetHashCode() * 17 + this.email.GetHashCode();
    }
}