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

    public Proverb(string phrase, List<string> keywords, string meaning, string example, 
    List<string> otherPhrases, List<string> otherMeanings, List<string> otherExamples) 
    {
        this.phrase = phrase;
        this.keywords = keywords;
        this.meaning = meaning;
        this.example = example;
        this.otherPhrases = otherPhrases;
        this.otherMeanings = otherMeanings;
        this.otherExamples = otherExamples;
    }
}
