using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace storage;

public record Card(int Id, string Title, string Content);

public record CardInput(string Title, string Content);

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

    [Function("addCard")]
    public async Task<AddCardResponse> AddCardAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "card")] HttpRequest req,
        [BlobInput("test/cards.json")] Card[] cards)
    {
        // Parse incoming card (without Id)
        CardInput? newCard = null;
        try
        {
            newCard = await JsonSerializer.DeserializeAsync<CardInput>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return new AddCardResponse { HttpResponse = new BadRequestObjectResult("Invalid JSON body.") };
        }
        if (newCard is null || string.IsNullOrWhiteSpace(newCard.Title) || string.IsNullOrWhiteSpace(newCard.Content))
        {
            return new AddCardResponse { HttpResponse = new BadRequestObjectResult("Title and Content are required.") };
        }

        // Assign new sequential Id
        int newId = (cards?.Length ?? 0) == 0 ? 1 : cards!.Max(c => c.Id) + 1;
        var cardToAdd = new Card(newId, newCard.Title, newCard.Content);

        // Append to cards array
        var updatedCards = (cards ?? []).ToList();
        updatedCards.Add(cardToAdd);

        // Serialize to stream

        _logger.LogInformation("Added new card with Id {id}", newId);
        return new AddCardResponse
        {
            HttpResponse = new OkObjectResult(cardToAdd),
            OutputBlob = [.. updatedCards]
        };
    }

    public class AddCardResponse
    {
        [BlobOutput("test/cards.json")]
        public Card[]? OutputBlob { get; set; }
        [HttpResult]
        public IActionResult? HttpResponse { get; set; }
    }
}
