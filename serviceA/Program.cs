using Jaeger;
using Jaeger.Samplers;
using System;
using Microsoft.Extensions.Logging;
using OpenTracing;
using System.Net;
using OpenTracing.Propagation;
using System.Collections.Generic;

namespace JaegerDemo
{
    class ServiceA
    {
        private readonly ITracer tracer;
        private readonly ILogger<ServiceA> logger;
        private readonly WebClient webClient = new WebClient();
        
        // Initializes the tracer
        private static Tracer InitTracer(string serviceName, ILoggerFactory loggerFactory)
        {
            var samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
                .WithType(ConstSampler.Type)
                .WithParam(1);

            var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
                .WithLogSpans(true);

            return (Tracer)new Configuration(serviceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration)
                .GetTracer();
        }

        public ServiceA(ITracer tracer, ILoggerFactory loggerFactory)
        {
            this.tracer = tracer;
            this.logger = loggerFactory.CreateLogger<ServiceA>();
        }

        public void printResponse(string response)
        {
            using (var scope = tracer.BuildSpan("print-response").StartActive(true))
            {               
                Console.WriteLine(response);
                logger.LogInformation(response);
                //scope.Span.Log("WriteLine");
            }
        }

        // Service A function
        public string sayHello(string name)
        {
            // builds onto scope a child span of the current active span
            using (var scope = tracer.BuildSpan("get-response").StartActive(true))
            {
                scope.Span.SetBaggageItem("client", "serviceA");
                string url = $"http://localhost:8002/api/serviceB/{name}";
                var activityId = new Guid().ToString();
                // passing the current context over the http request
                var dictionary = new Dictionary<string, string>();
                tracer.Inject(scope.Span.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));
                foreach (var entry in dictionary)
                    webClient.Headers.Add(entry.Key, entry.Value);

                var response = webClient.DownloadString(url);
                return response;
               // printResponse(response);
            }
                
        }
        static void Main(string[] args)
        {
            using (var loggerFactory = new LoggerFactory())
            {
                var name = args[0];
                // Initializes tracer for the service
                using (var tracer = InitTracer("ServiceA", loggerFactory))
                {
                    using (var scope = tracer.BuildSpan("say-hello").StartActive(true))
                    {
                        ServiceA runService = new ServiceA(tracer, loggerFactory);
                        var res = runService.sayHello(name);
                        runService.printResponse(res);
                    }
                }
            }
        }
    }
}
