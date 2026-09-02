using Microsoft.EntityFrameworkCore;
namespace abp_conference.Models
{
    public class Service
    {
        private static int Count = 0;
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }

        public Service(string name, int price)
        {
            Service.Count++;
            this.Id = Service.Count;
            this.Name = name;
            this.Price = price;
        }
    }
}
