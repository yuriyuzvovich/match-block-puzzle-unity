using MatchPuzzle.Core.Interfaces;
using Newtonsoft.Json;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class NewtonsoftJsonParser : IJsonParser
    {
        public string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}