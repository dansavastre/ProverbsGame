using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Bucket
{
    public string key;
    public int stage;
    public long timestamp;

    public Bucket(string key, int stage, long timestamp)
    {
        this.key = key;
        this.stage = stage;
        this.timestamp = timestamp;
    }

    // Checks the equality of the Bucket object with another object
    public override bool Equals(object obj)
    {
        var bucket = obj as Bucket;

        if (bucket == null)
            return false;
        if (bucket == this)
            return true;

        return this.key.Equals(bucket.key);
    }

    // Generates hashcode for a Bucket object
    public override int GetHashCode()
    {
        return this.key.GetHashCode();
    }
}