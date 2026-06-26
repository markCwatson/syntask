using System.Text.Json;

var options = new JsonSerializerOptions
{
  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  PropertyNameCaseInsensitive = true
};

while (Console.ReadLine() is { } line)
{
  if (string.IsNullOrWhiteSpace(line))
  {
    continue;
  }

  try
  {
    var request = JsonSerializer.Deserialize<RpcRequest>(line, options)!;
    object? result = null;
    string? error = null;

    if (request.Method == "hover")
    {
      var hoverRequest = JsonSerializer.Deserialize<HoverRequest>(
          request.Params.GetRawText(), options)!;
      result = HoverService.GetHover(hoverRequest);
    }
    else
    {
      error = $"Unknown method: {request.Method}";
    }

    var response = new RpcResponse(request.Id, result, error);
    Console.WriteLine(JsonSerializer.Serialize(response, options));
  }
  catch (Exception ex)
  {
    Console.Error.WriteLine($"Error processing request: {ex.Message}");
  }
}
