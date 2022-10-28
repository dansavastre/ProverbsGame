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
    /// <param name="phrase">String denoting the actual proverb.</param>
    /// <param name="keywords">List of keywords extracted from the proverb.</param>
    /// <param name="meaning">String denoting the meaning of the proverb.</param>
    /// <param name="example">String denoting an example using the proverb.</param>
    /// <param name="image">String denoting the path of the image accompanying the proverb.</param>
    /// <param name="otherPhrases">List of other possible phrases (all wrong).</param>
    /// <param name="otherKeywords">List of other possible keywords (all wrong).</param>
    /// <param name="otherMeanings">List of other possible meanings (all wrong).</param>
    /// <param name="otherExamples">List of other possible examples (all wrong).</param>
    /// <param name="funFact">String denoting a fun fact aboutt the proverb.</param>
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