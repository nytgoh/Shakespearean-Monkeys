namespace Monkeys
{
    using Carter;
	using System.Net.Http;
	using static System.Console;
	using Carter.ModelBinding;
    using System.Linq;
    using System.Collections.Generic;
	using System;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
		
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
	
	using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
	

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCarter();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(builder => builder.MapCarter());
        }
    }
	
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
					webBuilder.UseStartup<Startup>();
					webBuilder.UseUrls(
						"http://localhost:8081");
				})
                .Build();

            host.Run();
			
        }
	
}

	static class SharedText 
	{
		public static string txt = null;
		public static int distance = 0;
	}

    public class HomeModule : CarterModule
    {
        public HomeModule()
        {
			
		Post("/try", async(req, res) => {
			var t = await req.Bind<TryRequest>();
			
			WriteLine(t);
			
			HttpClientHandler clientHandler = new HttpClientHandler();
			clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
			client = new HttpClient(clientHandler);
			
			GeneticAlgorithm (t);
		});
		}
		
		private Random _random = new Random ();
		public HttpClient client;
		private List<string> latestgen;
		private int length = 0;
        
		private double NextDouble () {
			lock (this) {
				return _random.NextDouble ();
			}
		}
			
		private int NextInt (int a, int b) {
			lock (this) {
				return _random.Next (a, b);
			}
		}
		
		int ProportionalRandom (int[] weights, int sum) {
			var val = NextDouble () * sum;
			
			for (var i = 0; i < weights.Length; i ++) {
				if (val < weights[i]) return i;
				
				val -= weights[i];
			}
			
			WriteLine ($"***** Unexpected ProportionalRandom Error");
			return 0;
		}
	
		/**
		 * Generates a random string. For production of the initial random generation.
		 */
		string getRandomString(int length){
			IEnumerable<string> ret = Enumerable.Range(1, length).Select(x => ((char)(NextInt(32,126))).ToString());
			return ret.Aggregate((a, b) => a + b);
		}
		

		/**
		 * Finds the length of a word with the given ID.
		 */
		async Task findLength(int id){
			string current = "*";
			int best = 999;
			length = 0;

			while (true){
				var data = new AssessRequest() {id=id, genomes = new List<string>{current}};
				var json = JsonConvert.SerializeObject(data);
				var content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
				var response = await client.PostAsync("http://localhost:8091/assess", content);
				var currassess = await response.Content.ReadAsAsync<AssessResponse>();
				
				var scores = currassess.scores;
				var topscore = scores.Min();
				
				if (topscore > best) return;
				current = current + "*";
				length += 1;
				
				best = topscore;
				
			}
		}


		/**
		 * Utilises the parameters given by the client to run the Shakespearean-Monkeys genetic algorithm.
		 */
		async void GeneticAlgorithm (TryRequest treq) {
			
			
            WriteLine ($"..... GeneticAlgorithm {treq}");
            
            var id = treq.id;
            var monkeys = treq.monkeys;
            length = treq.length;
			if (length == 0){await findLength(id);}
            var crossover = treq.crossover / 100.0 ;
            var mutation = treq.mutation / 100.0;
            var limit = treq.limit;
			
			if (monkeys % 2 != 0) monkeys += 1;
            if (limit == 0) limit = 1000;
			
            var topscore = int.MaxValue;
			var lowscore = 0;
			
            
			var newgen = new List<string>();
			var prevgen = new List<string>();
			
			string[] initcurr = new string[monkeys];
			
			List<string> currentgen = initcurr.Select(x => getRandomString(length)).ToList(); // initialise
			
			AssessResponse currassess = null;
			AssessResponse prevassess = null;
			
			int weightsum;
			int[] weights;
                        
            for (int loop = 0; loop < limit; loop ++) { // evolution loop
				
				var data = new AssessRequest() {id=id, genomes = currentgen};
				var json = JsonConvert.SerializeObject(data);
				var content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
				var response = await client.PostAsync("http://localhost:8091/assess", content);
				currassess = await response.Content.ReadAsAsync<AssessResponse>();
				
				if (prevassess != null && prevassess.scores.Min() < currassess.scores.Min()){ // previous better than new
					WriteLine("Reset");
					currassess = prevassess;
					currentgen = prevgen.ToList();
				}
				
				
				var scores = currassess.scores;
				topscore = scores.Min();
				lowscore = scores.Max();
				
				
				weights = scores.Select((score) => lowscore - score + 1).ToArray();
				weightsum = weights.Sum();
				
				var topdata = new TopRequest() {id=id, loop=loop, score=topscore, genome=currentgen.ElementAt(Array.IndexOf(scores.ToArray(),topscore))};
				var topjson = JsonConvert.SerializeObject(topdata);
				var topcontent = new StringContent(topjson, UnicodeEncoding.UTF8, "application/json");
				var topresponse = await client.PostAsync("http://localhost:"+id.ToString()+"/top", topcontent);
				
				latestgen = currentgen.ToList();
				if (topscore == 0) {break;}
                
				// create new generation
				for (int rep = 0; rep < monkeys/2; rep++){ // repeat loop
					
					// select 2 parents, create two children
					var p1 = currentgen[ProportionalRandom(weights, weightsum)];
					var p2 = currentgen[ProportionalRandom(weights, weightsum)];
					
					string c1;
					string c2;
					if (NextDouble() < crossover) {
						var crossID = NextInt(0,length-1);
						c1 = p1.Substring(0, crossID) + p2.Substring(crossID);
						c2 = p2.Substring(0, crossID) + p1.Substring(crossID);
					} else {
						c1 = p1;
						c2 = p2;
					}

					if (NextDouble() < mutation) {
						var toChange1 = NextInt(0, length);
						c1 = c1.Substring(0,toChange1) + ((char)(NextInt(32,126))).ToString() + c1.Substring(toChange1+1);
					}
					
					if (NextDouble() < mutation) {
						var toChange2 = NextInt(0, length);
						c2 = c2.Substring(0,toChange2) + ((char)(NextInt(32,126))).ToString() + c2.Substring(toChange2+1);
					}

					newgen.Add(c1);
					newgen.Add(c2);
				}
				prevassess = currassess;
				prevgen = currentgen.ToList();
				currentgen = newgen.ToList();
				newgen.Clear();
				
            }
        }
		
		
    }
	
	public class Data {
		public string text { get; set;}	
	}

	public class Number {
		public int number {get;set;}
	}
	
	
	public class TargetRequest {
        public int id { get; set; }
        public bool parallel { get; set; }
        public string target { get; set; }
        public override string ToString () {
            return $"{{{id}, {parallel}, \"{target}\"}}";
        }  
    }    

    public class TryRequest {
        public int id { get; set; }
        public bool parallel { get; set; }
        public int monkeys { get; set; }
        public int length { get; set; }
        public int crossover { get; set; }
        public int mutation { get; set; }
        public int limit { get; set; }
        public override string ToString () {
            return $"{{{id}, {parallel}, {monkeys}, {length}, {crossover}, {mutation}, {limit}}}";
        }
    }
    
    public class TopRequest {
        public int id { get; set; }
        public int loop { get; set; }
        public int score { get; set; }
        public string genome { get; set; }
        public override string ToString () {
            return $"{{{id}, {loop}, {score}, {genome}}}";
        }  
    }    
    
    public class AssessRequest {
        public int id { get; set; }
        public List<string> genomes { get; set; }
        public override string ToString () {
            return $"{{{id}, {genomes.Count}}}";
        }  
    }
    
    public class AssessResponse {
        public int id { get; set; }
        public List<int> scores { get; set; }
        public override string ToString () {
            return $"{{{id}, {scores.Count}}}";
        }  
    }
	
}
