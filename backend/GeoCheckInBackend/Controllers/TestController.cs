namespace GeoCheckInBackend.Controllers;

using GeoCheckInBackend.Data;
using GeoCheckInBackend.Models;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("API is working!");
    }
}