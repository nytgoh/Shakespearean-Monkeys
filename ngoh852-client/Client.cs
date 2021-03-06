namespace Client
{
    using Carter;
    using Carter.ModelBinding;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;
    using static System.Console;

    public class HomeModule : CarterModule
    {
        static async void Exit()
        {
            await Task.Delay(1000);
            Environment.Exit(0);
        }

        public HomeModule()
        {
            Post("/top", async (req, res) =>
            {
                var top = await req.Bind<TopRequest>();
                WriteLine($"===== {top.loop} {top.score}\r\n{top.genome}");
                if (top.score == 0) Exit();
                return;
            });
        }

        /**
         * Posts the given JSON request (Target) to the Fitness server hosted on 8091.
         */
        public static async void PostTarget(TargetRequest t)
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri("http://localhost:8091/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            WriteLine($"..... POST /target send {t}");
            var hrm = await client.PostAsJsonAsync("/target", t);
            hrm.EnsureSuccessStatusCode();
            return;
        }

        /**
         * Posts the given JSON request (Target) to the Fitness server hosted on 8081.
         */
        public static async void PostTry(TryRequest t)
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri("http://localhost:8081/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            WriteLine($"..... POST /try send {t}");
            var hrm = await client.PostAsJsonAsync("/try", t);
            hrm.EnsureSuccessStatusCode();
            return;
        }

        /**
         * Starts the process, sending the Target information to the Fitness server 
         * and sending the Try information to the Monkeys and telling them to try figure the target out.
         */
        public static async void Start(int port)
        {
            await Task.Delay(1000);

            var line1 = Console.ReadLine()?.Trim();
            var line2 = Console.ReadLine()?.Trim();

            // ======================== CHANGE "abcd" into your target word of choice ==================================
            var targetjson = string.IsNullOrEmpty(line1) ? "{\"id\":0, \"target\": \"abcd\"}" : line1;

            // The number of monkeys, mutations and crossovers may be altered as well for further tweaking
            var tryjson = string.IsNullOrEmpty(line2) ? "{\"id\": 0, \"parallel\": true, \"monkeys\": 10, \"length\": 0, \"crossover\": 80, \"mutation\": 20 }" : line2;

            var target = JsonSerializer.Deserialize<TargetRequest>(targetjson);
            var trie = JsonSerializer.Deserialize<TryRequest>(tryjson);

            target.id = port;
            trie.id = port;

            Console.WriteLine($"..... target: {target}");
            Console.WriteLine($"..... try: {trie}");

            PostTarget(target);
            PostTry(trie);
        }
    }

    public class TargetRequest
    {
        public int id { get; set; }
        public bool parallel { get; set; }
        public string target { get; set; }
        public override string ToString()
        {
            return $"{{{id}, {parallel}, \"{target}\"}}";
        }
    }

    public class TryRequest
    {
        public int id { get; set; }
        public bool parallel { get; set; }
        public int monkeys { get; set; }
        public int length { get; set; }
        public int crossover { get; set; }
        public int mutation { get; set; }
        public int limit { get; set; }
        public override string ToString()
        {
            return $"{{{id}, {parallel}, {monkeys}, {length}, {crossover}, {mutation}, {limit}}}";
        }
    }

    public class TopRequest
    {
        public int id { get; set; }
        public int loop { get; set; }
        public int score { get; set; }
        public string genome { get; set; }
        public override string ToString()
        {
            return $"{{{id}, {loop}, {score}, {genome}}}";
        }
    }


    namespace Client
    {
        using Carter;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;

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
    }

    namespace Client
    {
        using Microsoft.AspNetCore.Hosting;
        using Microsoft.Extensions.Hosting;
        using Microsoft.Extensions.Logging;

        public class Program
        {
            public static void Main(string[] args)
            {

                var port = 0;
                if (args.Length > 0 && int.TryParse(args[0], out port)) {; }
                else { port = 8101; }

                var urls = new[] { $"http://localhost:{port}" };

                var host = Host.CreateDefaultBuilder(args)

                    .ConfigureLogging(logging =>
                    {
                        logging
                            .ClearProviders()
                            .AddConsole()
                            .AddFilter(level => level >= LogLevel.Warning);
                    })

                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseUrls(urls);
                    })

                    .Build();


                System.Console.WriteLine($"..... starting on {string.Join(", ", urls)}");

                HomeModule.Start(port);
                host.Run();
            }
        }
    }
}

