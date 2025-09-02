using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace storage;

public record Card(int Id, string Title, string Content);

public class Cards(ILogger<Cards> logger)
{
    private readonly ILogger<Cards> _logger = logger;

    [Function("cards")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
    [BlobInput("test/cards.json")] Card[] cards)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        _logger.LogInformation("Returning {count} cards", cards.Length);
        return new OkObjectResult(cards);
    }
}
