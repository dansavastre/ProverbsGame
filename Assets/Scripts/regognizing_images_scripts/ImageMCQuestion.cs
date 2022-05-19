using UnityEngine;

public class ImageMCQuestion : Question
{
    private Texture2D image = new Texture2D(2, 2);

    public ImageMCQuestion(string text, Answer[] answers, byte[] imageBytes)
    {
        this.text = text;
        this.answers = answers;
        this.image.LoadImage(imageBytes);
    }

    public Texture2D Image
    {
        get => image;
        set => image = value;
    }

    public string Text
    {
        get => text;
        set => text = value;
    }

    public Answer[] Answers
    {
        get => answers;
        set => answers = value;
    }
}
