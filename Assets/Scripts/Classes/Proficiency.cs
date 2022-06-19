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
    
    /// <summary>
    /// Constructor for the Proficiency class.
    /// </summary>
    public Proficiency()
    {
        this.apprentice = new List<Bucket>{};
        this.journeyman = new List<Bucket>{};
        this.expert = new List<Bucket>{};
        this.master = new List<Bucket>{};
    }
}
