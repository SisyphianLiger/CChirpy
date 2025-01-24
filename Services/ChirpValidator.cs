
public interface IChirpValidator
{
    bool isValid(string chirp);
    string cleanedBody(string? chirp);
}

public class ChirpValidator : IChirpValidator
{

    private List<string> listOfBannedWords = new List<string> 
    { 
        "kerfuffle", 
        "sharbert", 
        "fornax" 
    };

    public bool isValid(string chirp)
    {
        return chirp.Length <= 140;
    }

    public string cleanedBody(string? chirp) {

        if (chirp == null) {
            return "";
        }

        var data = chirp.Split(" ")
                        .Select(word => listOfBannedWords.Contains(word.ToLower()) ? "****" : word);

        return string.Join(" ", data);
    }
}
