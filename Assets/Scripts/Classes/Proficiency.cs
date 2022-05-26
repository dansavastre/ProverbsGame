using System.Collections;
using System.Collections.Generic;

public class Proficiency
{
    public List<string> apprentice;
    public List<string> journeyman;
    public List<string> expert;
    public List<string> master;
    
    public Proficiency()
    {
        this.apprentice = new List<string>{};
        this.journeyman = new List<string>{};
        this.expert = new List<string>{};
        this.master = new List<string>{};
    }
}
