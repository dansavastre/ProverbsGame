struct ProverbsDictionary
{
    public string proverb;
    public string meaning;

    /// <summary>
    /// Constructor for the ProverbsDictionary struct.
    /// </summary>
    /// <param name="proverb">The phrase of the proverb.</param>
    /// <param name="meaning">The correct meaning of the proverb.</param>
    public ProverbsDictionary(string proverb, string meaning)
    {
        this.proverb = proverb;
        this.meaning = meaning;
    }
}