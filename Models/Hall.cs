using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace abp_conference.Models
{
    public class Hall
    {
        private static int Count {get; set; } = 0;
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string? Services { get; set; }
        public int BasePrice { get; set; }

        public Hall() {}
        
        public Hall(string name, int capacity, string? services, int basePrice)
        {
            Hall.Count++;
            this.Id = Count;
            this.Name = name;
            this.Capacity = capacity;
            this.Services = services;
            this.BasePrice = basePrice;
        }

        public static bool ValidateServiceFormat(string service)
        {
            int f;
            var toArray = service.Split(':');
            if (toArray == null || toArray.Length != 2) return false;

            var numerical = toArray[1].Trim();

            var result = int.TryParse(numerical, out f);

            return result;
        }

        public static bool ValidateAllServices(string allServices)
        {
            try
            {
                var toArray = allServices.Split(",");
                foreach (string s in toArray)
                {
                    if (Hall.ValidateServiceFormat(s) == false) return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetServiceName(string service)
        {
            return service.Split(":")[0].ToLower().Trim();
        }

        public static int getServicePrice(string service)
        {
            var numerical = service.Split(':')[1].Trim();
            int f = int.Parse(numerical);
            return f;
        }

        public Dictionary<string, int> GetServiceHashMap()
        {
            var result = new Dictionary<string, int>();
            if (this.Services == null) return result;

            foreach (string s in this.Services.Split(","))
            {
                var valid = Hall.ValidateServiceFormat(s);
                if (valid)
                {
                    result.Add(Hall.GetServiceName(s), Hall.getServicePrice(s));
                }
            }

            return result;
        }
        public static string GenerateServiceText(Dictionary<string, int> map)
        {
            string resultString = "";
            foreach(var service in map)
            {
                resultString += service.Key + ": " + service.Value + ", ";
            }
            resultString = resultString.Remove(resultString.Length - 2);

            return resultString;
        }

        public static Dictionary<string, int> GetServiceHashMap(string service)
        {
            var result = new Dictionary<string, int>();
            foreach(string s in service.Split(","))
            {
                var valid = Hall.ValidateServiceFormat(s);
                if (valid)
                {
                    result.Add(Hall.GetServiceName(s.Trim().ToLower()), Hall.getServicePrice(s));
                }
            }

            return result;
        }
    }
}
