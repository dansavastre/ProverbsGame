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

    /**
     * Method that checks the equality of the Player object with another object.
     */
    public override bool Equals(object obj) {
        if (obj == null)
            return false;
        if (obj.GetType().Equals(this.GetType()))
            return false;
        if (obj == this)
            return true;
        
        Player other = (Player)obj;
        return this.playerName == other.playerName 
            && this.email == other.email;
    }
}