namespace HotelSearch.Application.Search;

public interface IPromptParser
{
    ParsedPrompt Parse(string prompt);
}
