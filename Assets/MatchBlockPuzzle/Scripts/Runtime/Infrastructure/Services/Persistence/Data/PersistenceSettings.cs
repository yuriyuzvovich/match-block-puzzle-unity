using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Configures how persistence serializes and stores game data.
    /// </summary>
    [CreateAssetMenu(fileName = "PersistenceSettings", menuName = "MatchPuzzle/Persistence Settings")]
    public class PersistenceSettings : ScriptableObject
    {
        [Header("JSON Parser")]
        public ParserSelectionMode Parser = ParserSelectionMode.NewtonsoftJson;

        public IJsonParser ResolveParser()
        {
            switch (Parser)
            {
                case ParserSelectionMode.JsonUtility:
                    return new UnityJsonParser();
                case ParserSelectionMode.NewtonsoftJson:
                    return new NewtonsoftJsonParser();
                default:
                    break;
            }

            return new NewtonsoftJsonParser();
        }
    }
}