using System.Collections;
using System.Collections.Generic;

public class Proverb
{
    public string phrase;
    public List<string> keywords;
    public string meaning;
    public string example;
    public List<string> otherPhrases;
    public List<string> otherMeanings;
    public List<string> otherExamples;

    public Proverb(string phrase, List<string> keywords, string meaning, string example) 
    {
        this.phrase = phrase;
        this.keywords = keywords;
        this.meaning = meaning;
        this.example = example;
        this.otherPhrases = new List<string>{"ptest1", "ptest2", "ptest3"};
        this.otherMeanings = new List<string>{"mtest1", "mtest2", "mtest3"};
        this.otherExamples = new List<string>{"etest1", "etest2", "etest3"};
    }

    public void AddKeyword(string keyword)
    {
        keywords.Add(keyword);
    }
    
    public void AddOtherPhrase(string phrase)
    {
        otherPhrases.Add(phrase);
    }
    
    public void AddOtherMeaning(string meaning)
    {
        otherMeanings.Add(meaning);
    }
    
    public void AddOtherExample(string example)
    {
        otherExamples.Add(example);
    }

    public void RemoveKeyword(string keyword)
    {
        keywords.Remove(keyword);
    }

    public void RemoveOtherPhrase(string phrase)
    {
        keywords.Remove(phrase);
    }

    public void RemoveOtherMeaning(string meaning)
    {
        otherMeanings.Remove(meaning);
    }
    
    public void RemoveOtherExample(string example)
    {
        otherExamples.Remove(example);
    }
}