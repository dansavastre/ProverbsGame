using System.Collections;
using System.Collections.Generic;

public class Proverb
{
    public string phrase;
    public List<string> keywords;
    public string meaning;
    public string example;
    public string image;
    public List<string> otherPhrases;
    public List<string> otherKeywords;
    public List<string> otherMeanings;
    public List<string> otherExamples;
    public string funFact;

    public Proverb(string phrase, List<string> keywords, string meaning, string example, string image, 
    List<string> otherPhrases, List<string> otherKeywords, List<string> otherMeanings, List<string> otherExamples) 
    {
        this.phrase = phrase;
        this.keywords = keywords;
        this.meaning = meaning;
        this.example = example;
        this.image = image;
        this.otherPhrases = otherPhrases;
        this.otherKeywords = otherKeywords;
        this.otherMeanings = otherMeanings;
        this.otherExamples = otherExamples;
    }
}
