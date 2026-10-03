using var client = new HttpClient();

while (true)
{
    Console.WriteLine("""
    
    1 - Fire and forget
    2 - Delayed job
    3 - Recurring job
    4 - Continuation pipeline
    0 - Exit
    """);

    var choice = Console.ReadLine();

    if (choice == "0")
        break;

    var endpoint = choice switch
    {
        "1" => "fire-and-forget",
        "2" => "delayed",
        "3" => "recurring",
        "4" => "pipeline",
        _ => null
    };

    if (endpoint == null)
        continue;

    var response = await client.PostAsync(
        $"http://localhost:5193/{endpoint}", null);

    Console.WriteLine(await response.Content.ReadAsStringAsync());
}