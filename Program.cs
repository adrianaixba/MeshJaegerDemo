using Jaeger;
using Jaeger.Samplers;
using System;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace JaegerDemo
{
    class ServiceA
    {
        private readonly ITracer tracer;
        private readonly ILogger<ServiceA> logger;
        
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

        // Service A function
        public void sayHello(string name)
        {
            var span = tracer.BuildSpan("saying-hello").Start();
            var activityId = new Guid().ToString();
            span.SetTag("hello-to", name);
            span.SetTag("activityId", activityId);
            var res = $"Hello, {name}";
            this.logger.LogInformation(res);
            Console.WriteLine(res);
            span.Finish();
        }
        static void Main(string[] args)
        {
            using (var loggerFactory = new LoggerFactory())
            {
                var name = args[0];
                // Initializes tracer for the service
                using (var tracer = InitTracer("ServiceA", loggerFactory))
                {
                    new ServiceA(tracer, loggerFactory).sayHello(name);
                }
            }
        }
    }
}
