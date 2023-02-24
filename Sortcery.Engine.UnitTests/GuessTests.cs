using System.Text.Json;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class GuessTests
{
    [Test]
    public void Guess_Deserialize()
    {
        var json = 
            """"
            {
                "title": "The Simpsons",
                "season": 1,
                "episode": 1,
                "episode_title": "Simpsons Roasting on an Open Fire",
                "source": "HDTV",
                "video_codec": "h264",
                "video_profile": "",
                "type": "episode"
            }
            """";
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseJsonNamingPolicy.Instance
        };
        var guess = JsonSerializer.Deserialize<Guess>(json, options);
        
        var expected = new Guess("episode", "The Simpsons")
        {
            Season = 1,
            Episode = 1,
            EpisodeTitle = "Simpsons Roasting on an Open Fire",
            Source = "HDTV",
            VideoCodec = "h264",
            VideoProfile = ""
        };
        Assert.That(guess, Is.EqualTo(expected));
    }
}