using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using abp_conference.Context;
using abp_conference.Models;
using NuGet.Protocol;

namespace abp_conference.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ConferenceContext _context;
        private HallsController _hallsController;

        public ReservationsController(ConferenceContext context, HallsController hallsController)
        {
            _context = context;
            _hallsController = hallsController;
        }

        [HttpGet("AvailableHalls")]
        public async Task<ActionResult<IEnumerable<Hall>>> GetAvailableHalls(DateOnly date, TimeOnly from, TimeOnly to, int minCapacity)
        {
            var reservationList = (await GetReservations()).Value;
            var reservationBegins = date.ToDateTime(from);
            var reservationEnds = date.ToDateTime(to);
            var occupied = reservationList.Where(res =>
                res.BeginTime.CompareTo(reservationEnds) < 1 &&
                reservationBegins.CompareTo(res.EndTime) < 1
            ).Select(x => x.HallId);

            Console.WriteLine(occupied);

            var availiableHalls = (await _hallsController.GetHalls()).Value.Where(hall => occupied.Contains(hall.Id) == false && hall.Capacity >= minCapacity);
            
            return availiableHalls.ToArray();
        }

        [HttpPost("MakeReservation")]
        public async Task<ActionResult<Reservation>> MakeReservation(int hallId, DateOnly date, TimeOnly time, int duration, string[] services = null)
        {
            services = services ?? new string[0];
            if (time.Hour + duration > 23) return BadRequest("Cannot book after hour 23");

            var available = (await GetAvailableHalls(date, time, time.AddHours(duration), 0)).Value;

            var hallExists = await _context.Halls.FindAsync(hallId);
            if (hallExists == null)
            {
                return BadRequest("Hall not found");
            }

            var hall = (await _hallsController.GetHall(hallId)).Value;
            if (available.Contains(hall) == false) return BadRequest("Selected hall is unavailable");

            var reservation = new Reservation(hallId, hall, new DateTime(date, time), new DateTime(date, time.AddHours(duration)), true, string.Join(',', services).ToLower());
            
            var createdId = (await PostReservation(reservation));

            var result = ReservationExists(reservation.Id);
            if (result == false) return Content("Error!!");

            var serviceAvailability = "\n";
            var hallServices = hall.GetServiceHashMap();
            foreach (var s in services)
            {
                var service = s.ToLower().Trim();
                if (hallServices.ContainsKey(service) == false)
                {
                    serviceAvailability = "\nSome services are not available at this hall\n";
                    break;
                }
            }


            return Content($"Reservation Successful!{serviceAvailability}{reservation.ToJson()}\nTotal price: {reservation.TotalPrice} || [{reservation.BaseFee} + {reservation.ServiceFee}]");
        }
        

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations.ToListAsync();
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // POST: api/Reservations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        private async Task<int> PostReservation(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation.Id;
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
