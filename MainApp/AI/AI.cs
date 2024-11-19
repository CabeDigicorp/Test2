using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Charts.Designer.Native;
using DevExpress.CodeParser;
using DevExpress.XtraRichEdit.Import.OpenXml;
using FastReport.AdvMatrix;
using Newtonsoft.Json;
using SharpToken;
using Telerik.Windows.Documents.Selection;
using Xbim.Tessellator;

namespace MainApp.AI
{
    internal class AI
    {
        //static string EMBEDDING_MODEL = "text-embedding-ada-002";
        static string EMBEDDING_MODEL = "text-embedding-3-large";
        static string COMPLETIONS_MODEL = "gpt-4";
        //static string API_KEY_old = "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";//alessandro.uliana@digicorp.it
        static string API_KEY = "sk-proj-Is1eOzJDP7asTsBk6f3lT3BlbkFJVMXj5a5BXRvH3PZoQhgk";//alessandro.uliana@digicorp.it
        //static string API_KEY_programmer = "sk-proj-IDAeVJfMWFb4qsl8uoPeT3BlbkFJNCaNikahQGDmP8R4dRhh";//programmer@digicorp.it
        static string EMBEDDING_FILE_PATH = "D:\\Temp\\text-embedding-3-large.txt";
        static int ROW_SELECTED_COUNT = 3;
        static string CUSTOMDATA_FILE_PATH = "D:\\Temp\\Lombardia2024_100.csv";//"D:\\Temp\\friuli2023_cap.CSV";


        public static async Task<string> Run(string query)
        {
            Cursor.Current = Cursors.WaitCursor;
            
            //await CreateEmbedding();

            var answare = await UseEmbedding(query);

            //MessageBox.Show("End", "AI");

            Cursor.Current = Cursors.Default;

            return answare;

        }

        public static async Task CreateEmbedding()
        {
            
            var list = LeggiRigheCSV(CUSTOMDATA_FILE_PATH);

            await CreateEmbedding(list);

        }

        public static async Task<string> CreateEmbedding(string query)
        {
            Cursor.Current = Cursors.WaitCursor;

            query = query.Replace('\t', '¦');
            query = query.Replace("\r\n", "\r");

            List<string> list = query.Split('\r').ToList();

            string ret = await CreateEmbedding(list);

            Cursor.Current = Cursors.Default;

            return ret;

        }

        public static async Task<string> UseEmbedding(string query)
        {

            Dictionary<string, List<double>> embs = ReadEmbedding().ToDictionary(item => item.Key, item => item.Value.embedding);

            var keys = (await OrderBySimilarity(query, embs)).Take(ROW_SELECTED_COUNT).Select(item => item.Item2).ToHashSet();



            //string answare = await Answare(query, keys);

            return string.Join("\n", keys);

        }

        private static async Task<string> Answare(string query, HashSet<string> keys)
        {
            //string filePath = SelezionaFile();
            var list = LeggiRigheCSV(CUSTOMDATA_FILE_PATH);

            List<string> rowsSelected = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                string row = list[i];
                string key = GetKey(row);

                if (keys.Contains(key))
                    rowsSelected.Add(row);
            }

            string prompt = ConstructPrompt(query, rowsSelected);

            string answare = await GetChatbotAnsware(prompt);

            return answare;
        }

        public static async Task<string> GetChatbotAnsware(string context)
        {
            string result = "Error";

            try
            {

                string apiUrl = "https://api.openai.com/v1/chat/completions";


                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestBody = new
                {
                    //model = "gpt-3.5-turbo",
                    //model = "gpt-3.5-turbo",
                    model = COMPLETIONS_MODEL,
                    messages = new[]
                    {
                    //new { role = "user", content = "Sei un chatbot GDPR, rispondi alla domanda solo utilizzando il contesto fornito. Se non sei in grado di rispondere alla domanda utilizzando il contesto fornito, dì \"Non lo so\"" },
                    new { role = "user", content = context },

                },
                    temperature = 0.7
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);


                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic returnedData = JsonConvert.DeserializeObject(responseBody);

                result = returnedData.choices[0].message.content;

                //result = await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
            }

            return result;


        }


        private static string ConstructPrompt(string query, List<string> rowsSelected)
        {
            string prompt = string.Empty;
            prompt += "Articoli: \n";

            prompt += string.Join("\n", rowsSelected);

            prompt += "\n\n --- \n\n + " + query;



            return prompt;
        }

        static async Task<string> CreateEmbedding(List<string> list)
        {

            try
            {

                var encoding = GptEncoding.GetEncodingForModel(COMPLETIONS_MODEL);
                Dictionary<string, Embedding> dict = new Dictionary<string, Embedding>();
                for (int i = 0; i < list.Count; i++)
                {
                    string row = list[i];
                    if (string.IsNullOrEmpty(row))
                        continue;

                    string key = GetKey(row);
                    var encoded = encoding.Encode(row);
                    if (encoded.Count < 5000)
                    {
                        Embedding emb = await GetEmbedding(API_KEY, row, EMBEDDING_MODEL);
                        if (emb != null)
                        {
                            dict.Add(key, emb);
                        }
                    }
                }

                string jsonString = JsonConvert.SerializeObject(dict);

                File.WriteAllText(EMBEDDING_FILE_PATH, jsonString);

                

            }
            catch (Exception exc)
            {
            
            }

            return "Created embedding file";
        }

        private static string GetKey(string row)
        {
            //return row.Substring(0, row.IndexOf(";"));
            return row.Substring(0, row.IndexOf("¦"));
        }

        static async Task<Embedding> GetEmbedding(string apiKey, string inputText, string modelName)
        {

            Embedding emb = null;

            try
            {

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string requestUrl = "https://api.openai.com/v1/embeddings";
                string requestBody = $"{{\"input\": \"{inputText}\", \"model\": \"{modelName}\"}}";

                using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                using var response = await httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {

                    string responseContent = await response.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<EmbeddingsResponse>(responseContent);

                    emb = resObj.data.FirstOrDefault();

                }

            }
            catch (Exception exc)
            {

            }

            return emb;
        }

        public static List<string> LeggiRigheCSV(string percorsoFile)
        {
            List<string> righe = new List<string>();
            using (StreamReader sr = new StreamReader(percorsoFile))
            {
                string riga;
                while ((riga = sr.ReadLine()) != null)
                {
                    //string[] campi = riga.Split(',');
                    righe.Add(riga);
                }
            }
            return righe;
        }

        public static string SelezionaFile()
        {
            string percorsoFile = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Tutti i file (*.*)|*.*";
                openFileDialog.Title = "Seleziona un file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    percorsoFile = openFileDialog.FileName;
                }
            }

            return percorsoFile;
        }

        public static Dictionary<string, Embedding> ReadEmbedding()
        {
            string jsonString = File.ReadAllText(EMBEDDING_FILE_PATH);
            Dictionary<string, Embedding> dict = JsonConvert.DeserializeObject<Dictionary<string, Embedding>>(jsonString);

            return dict;
        }


        public static double CalculateSimilarity(List<double> x, List<double> y)
        {
            return x.Zip(y, (a, b) => a * b).Sum();
        }

        public async static Task<List<Tuple<double, string>>> OrderBySimilarity(string query, Dictionary<string, List<double>> contexts)
        {
            // Ottieni l'embedding della query
            Embedding queryEmbedding = await GetEmbedding(API_KEY, query, EMBEDDING_MODEL);

            // Calcola la similarità tra l'embedding della query e gli embeddings dei documenti
            var documentSimilarities = contexts.Select(context =>
                new Tuple<double, string>(
                    CalculateSimilarity(queryEmbedding.embedding, context.Value),
                    context.Key
                )).ToList();

            // Ordina i contesti in base alla similarità in ordine decrescente
            documentSimilarities.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            return documentSimilarities;
        }


    }

    class DocumentSection
    {
        public int Tokens { get; set; }
        public string Content { get; set; }
    }

    public class EmbeddingsResponse
    {
        public string Object { get; set; }
        public List<Embedding> data { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
    }

    public class Embedding
    {
        public string Object { get; set; }
        public List<double> embedding { get; set; }
        public int index { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
    }

}
