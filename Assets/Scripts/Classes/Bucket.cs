using System;
using System.Collections;
using System.Collections.Generic;

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

    public override bool Equals(object obj)
    {
        var bucket = obj as Bucket;

        if (bucket == null)
        {
            return false;
        }

        return this.key.Equals(bucket.key);
    }
}
