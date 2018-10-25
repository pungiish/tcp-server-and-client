using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

    public class Game
    {
        private List<string> WordList;
        private string pickedWord;
        private string hiddenWord;
        private bool gameHasStarted = false;

        public Game()
        {
            WordList = new List<string>();
        }

        public string getWord()
        {
            return pickedWord;
        }

        public string getHiddenWord()
        {
            return hiddenWord;
        }

        public bool getGameHasStarted()
        {
            return gameHasStarted;
        }

        public void setGameHasStarted(bool started)
        {
            gameHasStarted = started;
        }

        public void gameStart()
        {
            Console.WriteLine("Making API Call...");
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri("https://od-api.oxforddictionaries.com/api/v1/wordlist/en/domains=computing");
                client.DefaultRequestHeaders.Add("app_id", "cf80a631");
                client.DefaultRequestHeaders.Add("app_key", "65aced6f01559436fe1efa6cbfbc0389");
                HttpResponseMessage response = client.GetAsync(client.BaseAddress).Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;
                //Console.WriteLine("Result: " + result);
                dynamic word = JsonConvert.DeserializeObject(result);
                foreach (var name in word.results)
                {
                    string wordname = name.word;
                    WordList.Add(wordname);
                }
            }
        }

        public void pickRandomWord()
        {
            Random r = new Random();
            int rInt = r.Next(0, WordList.Count);
            pickedWord = WordList[rInt].ToLower();
            char[] ch = pickedWord.ToCharArray();
            for (int i = 0; i < 3; i++)
            {
                rInt = r.Next(0, pickedWord.Length);
                ch[rInt] = '*';
            }
            hiddenWord = new string(ch);
        }
    }
