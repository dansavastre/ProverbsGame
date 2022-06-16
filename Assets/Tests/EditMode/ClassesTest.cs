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
                            otherPhrases, otherKeywords, otherMeanings, otherExamples, "funFact");
        Assert.AreEqual("proverb phrase", proverb.phrase);
        Assert.AreEqual("proverb meaning", proverb.meaning);
        Assert.AreEqual("proverb example", proverb.example);
        Assert.AreEqual("proverb image", proverb.image);
        Assert.AreEqual(keywords, proverb.keywords);
        Assert.AreEqual(otherKeywords, proverb.otherKeywords);
        Assert.AreEqual(otherPhrases, proverb.otherPhrases);
        Assert.AreEqual(otherMeanings, proverb.otherMeanings);
        Assert.AreEqual(otherExamples, proverb.otherExamples);
        Assert.AreEqual("funFact", proverb.funFact);
    }

    [Test]
    public void BucketTest()
    {
        Bucket bucket = new Bucket("key", 2, 0);
        Assert.AreEqual("key", bucket.key);
        Assert.AreEqual(2, bucket.stage);
        Assert.AreEqual(0, bucket.timestamp);
    }

    [Test]
    public void ProficiencyTest()
    {
        Bucket bucket1 = new Bucket("key1", 1, 0);
        Bucket bucket2 = new Bucket("key2", 2, 1);
        Bucket bucket3 = new Bucket("key3", 3, 2);
        Bucket bucket4 = new Bucket("key4", 4, 3);

        Proficiency proficiency = new Proficiency();
        proficiency.apprentice.Add(bucket1);
        proficiency.journeyman.Add(bucket2);
        proficiency.expert.Add(bucket3);
        proficiency.master.Add(bucket4);

        Assert.IsTrue(proficiency.apprentice.Contains(bucket1));
        Assert.IsTrue(proficiency.journeyman.Contains(bucket2));
        Assert.IsTrue(proficiency.expert.Contains(bucket3));
        Assert.IsTrue(proficiency.master.Contains(bucket4));
        
    }

}
