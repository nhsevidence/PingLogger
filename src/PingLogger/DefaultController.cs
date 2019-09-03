using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace PingLogger
{
  public class DefaultController : Controller
  {
    public IActionResult Index()
    {
      Log.Information("Logging a ping");

      return Content("Logged a ping");
    }
  }
}