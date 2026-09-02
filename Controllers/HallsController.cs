using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using abp_conference.Context;
using abp_conference.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Build.Experimental.BuildCheck;

namespace abp_conference.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallsController : ControllerBase
    {
        private readonly ConferenceContext _context;

        public HallsController(ConferenceContext context)
        {
            _context = context;
        }

        // GET: api/Halls
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Hall>>> GetHalls()
        {
            return await _context.Halls.ToListAsync();
        }

        // GET: api/Halls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hall>> GetHall(int id)
        {
            var hall = await _context.Halls.FindAsync(id);

            if (hall == null)
            {
                return NotFound();
            }

            return hall;
        }

        // PUT: api/Halls/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutHall(int id, string? name, int? capacity, int? price, string? removeService, string? updateService, string? overrideAllServices)
        {
            if (!HallExists(id))
            {
                return NotFound();
            }

            var hall = (await GetHall(id)).Value;
            if (name != null)
            {
                hall.Name = name;
            }
            if (price != null)
            {
                hall.BasePrice = price.Value;
            }
            if (capacity != null)
            {
                hall.Capacity = capacity.Value;
            }
            if (removeService != null)
            {
                var services = hall.Services;
                bool wasElementDeleted = false;
                if (services != null)
                {
                    var serviceMap = hall.GetServiceHashMap();
                    foreach (var pair in serviceMap)
                    {
                        Console.WriteLine(pair.Key);
                        if (pair.Key.ToLower().Trim() == removeService.ToLower().Trim())
                        {
                            serviceMap.Remove(pair.Key);
                            wasElementDeleted = true;
                            break;
                        }
                    }

                    hall.Services = Hall.GenerateServiceText(serviceMap);
                    if (wasElementDeleted == false) return Content("Error: No Service Found!");
                }
            }
            if (updateService != null)
            {
                if (Hall.ValidateServiceFormat(updateService))
                {
                    var serviceMap = hall.GetServiceHashMap();
                    var formattedServiceName = Hall.GetServiceName(updateService);
                    var addServicePrice = Hall.getServicePrice(updateService);
                    if (serviceMap.ContainsKey(formattedServiceName) == true)
                    {
                        serviceMap[formattedServiceName] = addServicePrice;
                    }
                    else
                    {
                        serviceMap.Add(formattedServiceName, addServicePrice);
                    }
                    hall.Services = Hall.GenerateServiceText(serviceMap);
                }
                else
                {
                    return Content("Invalid Service data formatting!");
                }
            }
            if (overrideAllServices != null)
            {
                try
                {
                    if (Hall.ValidateAllServices(overrideAllServices) == false) {
                        throw new Exception();
                    }
                    var services = overrideAllServices.Split(',');
                    foreach (string service in services)
                    {
                        var success = Hall.ValidateServiceFormat(service);
                        if (success == false)
                        {
                            throw new Exception();
                        }
                    }

                    hall.Services = overrideAllServices;
                }
                catch (Exception e)
                {
                    return Content("Invalid Service data formatting!");
                }
            }

                await _context.SaveChangesAsync();

            return Content("Changes Updated successfully!");
        }

        // POST: api/Halls
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Add")]
        public async Task<ActionResult<Hall>> PostHall(string name = "Hall", int capacity = 50, string? services = null, int price = 2000)
        {
            var hall = new Hall(name, capacity, services, price);
            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHall", new { id = hall.Id }, "Hall Created Successfully!\nHall ID: " + (hall.Id).ToString());
        }

        // DELETE: api/Halls/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteHall(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
            {
                return NotFound();
            }

            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync();

            return StatusCode(200);
        }

        private bool HallExists(int id)
        {
            return _context.Halls.Any(e => e.Id == id);
        }
    }
}
