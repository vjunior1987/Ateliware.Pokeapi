﻿using Newtonsoft.Json;
using Pokeapi.Utils;

namespace Pokeapi.Services
{
    /// <summary>
    /// Using service architecture to implement the API call in a separate class, to comply with SOLID principles for better software design
    /// </summary>
    public class PokemonService : IPokemonService
    {
        private readonly string _url;
        private readonly IHttpClientWrapper _client;
        public PokemonService(IHttpClientWrapper client)
        {
            _url = "https://pokeapi.co/api/v2/pokemon/";
            _client = client;
        }

        /// <summary>
        /// This constructor was included in order to mock the httpClient and pass a custom url for testing.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        public PokemonService(IHttpClientWrapper client, string url)
        {
            _url = url;
            _client = client;
        }

        /// <summary>
        /// This method will compare two pokémon to determine which has the most HP
        /// </summary>
        /// <param name="poke1"></param>
        /// <param name="poke2"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> GetStrongerPokemonNameAsync(string poke1, string poke2)
        {
            if (poke1 == poke2)
            {
                return $"Both pokémons are equal";
            }

            var poke1Hp = 0;
            var poke2Hp = 0;

            var poke1Result = await _client.GetAsync($"{_url}{poke1.ToLowerInvariant().Trim()}");
            var poke2Result = await _client.GetAsync($"{_url}{poke2.ToLowerInvariant().Trim()}");

            if (poke1Result.IsSuccessStatusCode && poke2Result.IsSuccessStatusCode)
            {
                var poke1Str = await poke1Result.Content.ReadAsStringAsync();
                var poke2Str = await poke2Result.Content.ReadAsStringAsync();

                var pokemon1 = JsonConvert.DeserializeObject<dynamic>(poke1Str);
                var pokemon2 = JsonConvert.DeserializeObject<dynamic>(poke2Str);

                if (pokemon1?.stats != null && pokemon1.stats[0].base_stat != null)
                {
                    poke1Hp = Convert.ToInt32(pokemon1.stats[0].base_stat);
                }
                if (pokemon2?.stats != null && pokemon2.stats[0].base_stat != null)
                {
                    poke2Hp = Convert.ToInt32(pokemon2.stats[0].base_stat);
                }
                if (poke1Hp == poke2Hp)
                {
                    return $"It is a tie between {poke1} and {poke2} with {poke1Hp}HP!";
                }
                return poke1Hp > poke2Hp ? $"The stronger pokémon is {poke1} with {poke1Hp}HP" : $"The stronger pokémon is {poke2} with {poke2Hp}HP";
            }

            if (poke1Result.StatusCode == System.Net.HttpStatusCode.NotFound) throw new KeyNotFoundException($"Could not find pokémon {poke1}");
            if (poke2Result.StatusCode == System.Net.HttpStatusCode.NotFound) throw new KeyNotFoundException($"Could not find pokémon {poke2}");

            throw new InvalidOperationException("Unknown error when retrieving Pokémon data");
        }
    }
}
