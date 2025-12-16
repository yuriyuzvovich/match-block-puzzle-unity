using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class UnityJsonParser : IJsonParser
    {
        public string Serialize<T>(T data)
        {
            return JsonUtility.ToJson(data);
        }

        public T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}