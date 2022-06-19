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

    /// <summary>
    /// Constructor for the Proverb class.
    /// </summary>
    /// <param name="phrase">string denoting the actual proverb</param>
    /// <param name="keywords">list of keywords extracted from the proverb</param>
    /// <param name="meaning">string denoting the meaning of the </param>
    /// <param name="example">string denoting an example using the proverb</param>
    /// <param name="image">string denoting the path of the image accompanying the proverb</param>
    /// <param name="otherPhrases">list of other possible phrases (all wrong)</param>
    /// <param name="otherKeywords">list of other possible keywords (all wrong)</param>
    /// <param name="otherMeanings">list of other possible meanings (all wrong)</param>
    /// <param name="otherExamples">list of other possible examples (all wrong)</param>
    /// <param name="funFact">string denoting a fun fact aboutt the proverb</param>
    public Proverb(string phrase, List<string> keywords, string meaning, string example, string image, 
    List<string> otherPhrases, List<string> otherKeywords, List<string> otherMeanings, List<string> otherExamples, string funFact) 
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
        this.funFact = funFact;
    }
}
