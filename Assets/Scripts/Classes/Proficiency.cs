using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Proficiency
{
    public List<Bucket> apprentice;
    public List<Bucket> journeyman;
    public List<Bucket> expert;
    public List<Bucket> master;
    
    public Proficiency()
    {
        this.apprentice = new List<Bucket>{};
        this.journeyman = new List<Bucket>{};
        this.expert = new List<Bucket>{};
        this.master = new List<Bucket>{};
    }

    // public void AddApprentice(string proverb)
    // {
    //     apprentice.Add(proverb);
    // }
    
    // public void AddJourneyman(string proverb)
    // {
    //     journeyman.Add(proverb);
    // }

    // public void AddExpert(string proverb)
    // {
    //     expert.Add(proverb);
    // }

    // public void AddMaster(string proverb)
    // {
    //     master.Add(proverb);
    // }
    
    // public void RemoveApprentice(string proverb)
    // {
    //     apprentice.Remove(proverb);
    // }
    
    // public void RemoveJourneyman(string proverb)
    // {
    //     journeyman.Remove(proverb);
    // }

    // public void RemoveExpert(string proverb)
    // {
    //     expert.Remove(proverb);
    // }

    // public void RemoveMaster(string proverb)
    // {
    //     master.Remove(proverb);
    // }
}