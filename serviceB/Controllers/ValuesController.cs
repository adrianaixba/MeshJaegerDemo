using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;

namespace ServiceB.Controllers
{
    [Route("api/serviceB")]
    public class ServiceBController : Controller
    {
        private readonly ITracer tracer;

        public ServiceBController(ITracer tracer)
        {
            this.tracer = tracer;
        }

        // GET: api/serviceB
        [HttpGet]
        public string Get()
        {
            return "Hello!";
        }

        // GET: api/serviceB/name
        [HttpGet("{name}", Name = "BuildResponse")]
        public string Get(string name)
        {
            using (var scope = tracer.BuildSpan("build-response").StartActive(true))
            {
                scope.Span.SetBaggageItem("server", "ServiceB");
                var response = $"Hello, {name}!";
                scope.Span.SetBaggageItem("response", response);
                Debug.WriteLine("received request from " + scope.Span.GetBaggageItem("client").ToString());
                return response;
            }
        }
    }
}