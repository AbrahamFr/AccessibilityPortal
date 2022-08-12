using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ComplianceSheriff.Demo
{
    [Route("rest/[controller]")]
    public class DemoController : Controller
    {
        public DemoController(/* DI can go here */)
        {

        }

        [AllowAnonymous]
        [HttpGet("hello-world")]
        public async Task<string> HelloWorld(CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            return "Hello World";
        }
    }
}
