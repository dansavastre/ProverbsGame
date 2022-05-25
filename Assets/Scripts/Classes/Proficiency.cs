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

    public void AddApprentice(string proverb)
    {
        apprentice.Add(proverb);
    }
    
    public void AddJourneyman(string proverb)
    {
        journeyman.Add(proverb);
    }

    public void AddExpert(string proverb)
    {
        expert.Add(proverb);
    }

    public void AddMaster(string proverb)
    {
        master.Add(proverb);
    }
    
    public void RemoveApprentice(string proverb)
    {
        apprentice.Remove(proverb);
    }
    
    public void RemoveJourneyman(string proverb)
    {
        journeyman.Remove(proverb);
    }

    public void RemoveExpert(string proverb)
    {
        expert.Remove(proverb);
    }

    public void RemoveMaster(string proverb)
    {
        master.Remove(proverb);
    }
}
