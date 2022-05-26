using System;
using System.Collections;
using System.Collections.Generic;

public class Bucket
{
    public int count;
    public DateTime lastAnswered;
    public long utcTimestamp;

    public Bucket(int count)
    {
        this.count = count;
    }
}
