using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ClassesTest
{
    [Test]
    public void PlayerTestConstructor()
    {
        Player player = new Player("playerName", "playerEmail");
        Assert.AreEqual("playerName", player.playerName);
        Assert.AreEqual("playerEmail", player.email);
    }

    [Test]
    public void ProverbTest()
    {
        List<string> keywords = new List<string>{};
        List<string> otherPhrases = new List<string>{};
        List<string> otherKeywords = new List<string>{};
        List<string> otherMeanings = new List<string>{};
        List<string> otherExamples = new List<string>{};
        
        keywords.Add("keyword");
        keywords.Add("word");
        keywords.Add("key");
        
        otherPhrases.Add("other phrase 1");
        otherPhrases.Add("other phrase 2");
        otherPhrases.Add("other phrase 3");

        otherKeywords.Add("other keyWord 1");
        otherKeywords.Add("other keyWord 2");
        otherKeywords.Add("other keyWord 3");
        
        otherMeanings.Add("other meaning 1");
        otherMeanings.Add("other meaning 2");
        otherMeanings.Add("other meaning 3");
        
        otherExamples.Add("other example 1");
        otherExamples.Add("other example 2");
        otherExamples.Add("other example 3");
        
        Proverb proverb = new Proverb("proverb phrase", keywords, "proverb meaning", "proverb example", "proverb image", 
                            otherPhrases, otherKeywords, otherMeanings, otherExamples);
        Assert.AreEqual("proverb phrase", proverb.phrase);
        Assert.AreEqual("proverb meaning", proverb.meaning);
        Assert.AreEqual("proverb example", proverb.example);
        Assert.AreEqual("proverb image", proverb.image);
        Assert.AreEqual(keywords, proverb.keywords);
        Assert.AreEqual(otherKeywords, proverb.otherKeywords);
        Assert.AreEqual(otherPhrases, proverb.otherPhrases);
        Assert.AreEqual(otherMeanings, proverb.otherMeanings);
        Assert.AreEqual(otherExamples, proverb.otherExamples);
    }

    [Test]
    public void BucketTest()
    {
        Bucket bucket = new Bucket("key", 2, 0);
        Assert.AreEqual("key", bucket.key);
        Assert.AreEqual(2, bucket.stage);
        Assert.AreEqual(0, bucket.timestamp);
    }
}
