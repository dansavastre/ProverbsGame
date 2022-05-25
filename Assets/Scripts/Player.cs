using System.Collections;
using System.Collections.Generic;

public class Player
{
    public string playerName;
    public string email;
    public string proficiency;

    public Player(string playerName, string email, string proficiency)
    {
        this.playerName = playerName;
        this.email = email;
        this.proficiency = proficiency;
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
            && this.email == other.email 
            && this.proficiency == other.proficiency;
    }
}