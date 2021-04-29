namespace Fitness
{
	using Carter;
	using static System.Console;
	using Carter.ModelBinding;
    using Carter.Response;
    using System.Linq;
	using System;
    using System.Collections.Generic;

    public class HomeModule : CarterModule
    {
		static string goal = null; 
		static int host = 0;
		static bool par = false;
        public HomeModule()
        {
            /**
             * Initialises the target value information.
             */
            Post("/target", async(req, res) => {
				var inp = await req.Bind<TargetRequest>();
				goal = inp.target;
				host = inp.id;
				par = inp.parallel;
				
				WriteLine(inp);
			});

            /**
             * Responds to AssessRequests with the fitness values and ids.
             */
			Post("/assess", async(req, res) => {
				var inp = await req.Bind<AssessRequest>();
				var scores = inp.genomes.Select((g) => {
					var len = Math.Min(goal.Length, g.Length);
					var h = Enumerable .Range (0, len)  
						.Sum (i => Convert.ToInt32 (goal[i] != g[i]));
					h = h + Math.Max (goal.Length, g.Length) - len;
					return h;
				}) .ToList ();
		
				var num = new AssessResponse { id = inp.id, scores = scores };
				await res.AsJson(num);
				
			});
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
            return $"Target Request: {{{id}, {parallel}, \"{target}\"}}";
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
