using Microsoft.EntityFrameworkCore;
using abp_conference.Models;
namespace abp_conference.Context
{
    public class ConferenceContext : DbContext
    {
        public ConferenceContext(DbContextOptions<ConferenceContext> options) : base(options)
        {
        }

        public DbSet<Hall> Halls { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
    }
}