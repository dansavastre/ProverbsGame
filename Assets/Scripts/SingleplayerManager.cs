using Firebase;
using Firebase.Database;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class SingleplayerManager : MonoBehaviour
{
    // UI elements
    [SerializeField] protected TextMeshProUGUI questionText;
    [SerializeField] protected TextMeshProUGUI resultText;
    [SerializeField] protected GameObject nextQuestionButton;

    // Stores information fetched from the database
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    protected DatabaseReference dbReference;
    protected Proverb nextProverb;
    protected string currentType;
    protected string currentKey;

    // Variables
    protected Question currentQuestion;
    private Random random;
    
    // Start is called before the first frame update
    protected void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;

        random = new Random();
        GetNextKey();
        nextQuestionButton.SetActive(false);
    }

    // Display the feedback after the player answers the question
    protected void DisplayFeedback(bool correct)
    {
        if (correct) 
        {
            resultText.text = "Correct!";
            UpdateProficiency();
            SessionManager.RightAnswer();
        }
        else 
        {
            resultText.text = "Incorrect!";
            SessionManager.WrongAnswer();
        }
        nextQuestionButton.SetActive(true);
    }

    // Load the next question
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        switch (currentType)
        {
            case "apprentice":
                SceneManager.LoadScene("RecognizeImage");
                break;
            case "journeyman":
                SceneManager.LoadScene("MultipleChoice");
                break;
            case "expert":
                SceneManager.LoadScene("FillBlanks");
                break;
            case "master":
                SceneManager.LoadScene("MultipleChoice");
                break;
            default:
                string json = JsonUtility.ToJson(newProficiency);
                dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
    }

    // Get the key for the next proverb in the session in chronological order
    protected void GetNextKey()
    {
        if (playerProficiency.apprentice.Count > 0)
        {
            currentKey = playerProficiency.apprentice.ElementAt(random.Next(0, playerProficiency.apprentice.Count));
            currentType = "apprentice";
        }
        else if (playerProficiency.journeyman.Count > 0)
        {
            currentKey = playerProficiency.journeyman.ElementAt(random.Next(0, playerProficiency.journeyman.Count));
            currentType = "journeyman";
        }
        else if (playerProficiency.expert.Count > 0)
        {
            currentKey = playerProficiency.expert.ElementAt(random.Next(0, playerProficiency.expert.Count));
            currentType = "expert";
        }
        else if (playerProficiency.master.Count > 0)
        {
            currentKey = playerProficiency.master.ElementAt(random.Next(0, playerProficiency.master.Count));
            currentType = "master";
        }
        else
        {
            Debug.Log("Session complete.");
            currentKey = null;
            currentType = "none";
        }
    }

    // Update the player proficiency into a new object
    // TODO I think having only 1 wrong answer and moving back isn't really fair. As a should have, we should improve this.
    // TODO Maybe using a HashSet is better, as removing from a list based on element takes much longer
    protected void UpdateProficiency()
    {
        switch (currentType)
        {
            case "apprentice":
                playerProficiency.apprentice.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.journeyman.Add(currentKey);
                    Debug.Log(currentKey + " moved to journeyman!");
                } else 
                {
                    newProficiency.apprentice.Add(currentKey);
                    Debug.Log(currentKey + " stayed in apprentice...");
                }
                break;
            case "journeyman":
                playerProficiency.journeyman.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.expert.Add(currentKey);
                    Debug.Log(currentKey + " moved to expert!");
                } else 
                {
                    newProficiency.apprentice.Add(currentKey);
                    Debug.Log(currentKey + " moved to apprentice...");
                }
                break;
            case "expert":
                playerProficiency.expert.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.master.Add(currentKey);
                    Debug.Log(currentKey + " moved to master!");
                } else 
                {
                    newProficiency.journeyman.Add(currentKey);
                    Debug.Log(currentKey + " moved to journeyman...");
                }
                break;
            case "master":
                playerProficiency.master.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.master.Add(currentKey);
                    Debug.Log(currentKey + " stayed in master!");
                } else 
                {
                    newProficiency.expert.Add(currentKey);
                    Debug.Log(currentKey + " moved to expert...");
                }
                break;
            default:
                Debug.Log("Invalid type.");
                break;
        }
    }
}
