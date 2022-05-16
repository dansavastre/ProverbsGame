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
}