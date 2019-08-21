using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using OpenTracing.Propagation;

namespace ServiceB.Controllers
{
    [Route("api/serviceB")]
    public class ServiceBController : Controller
    {
        private readonly ITracer tracer;
        private readonly WebClient webClient = new WebClient();

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
            //var response = ""; 
            using (var scope = tracer.BuildSpan("build-response").StartActive(true))
            {
                tracer.ActiveSpan.SetBaggageItem("server", "ServiceB");
                string response = $"Hello, {name}!";
                tracer.ActiveSpan.SetBaggageItem("response", response);
                // Debug.WriteLine("received request from " + tracer.ActiveSpan.GetBaggageItem("client").ToString());
                return response;
            }
        }
    }
}