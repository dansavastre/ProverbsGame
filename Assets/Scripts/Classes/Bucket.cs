using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Bucket
{
    public string key;
    public int stage;
    public long timestamp;

    /// <summary>
    /// Constructor for the Bucket class.
    /// </summary>
    /// <param name="key">string denoting the key of the bucket</param>
    /// <param name="stage">index of the stage that the bucket is currently in</param>
    /// <param name="timestamp">timestamp of the last time the bucket was accessed</param>
    public Bucket(string key, int stage, long timestamp)
    {
        this.key = key;
        this.stage = stage;
        this.timestamp = timestamp;
    }

    /// <summary>
    /// Checks the equality of the Bucket object with another object.
    /// </summary>
    /// <param name="obj">the object that this Bucket is compared to</param>
    /// <returns>whether or not the 2 objects are equal</returns>
    public override bool Equals(object obj)
    {
        var bucket = obj as Bucket;

        if (bucket == null)
            return false;
        if (bucket == this)
            return true;

        return this.key.Equals(bucket.key);
    }

    /// <summary>
    /// Generates hash code for a Bucket object.
    /// </summary>
    /// <returns>integer denoting the hash code for the Bucket object</returns>
    public override int GetHashCode()
    {
        return this.key.GetHashCode();
    }
}