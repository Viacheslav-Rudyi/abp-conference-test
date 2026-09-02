using Microsoft.EntityFrameworkCore;
namespace abp_conference.Models
{
    /// <summary>
    /// Represents information about reservations <br/>
    /// <value>Id</value>: unique Identifier <br/>
    /// <value>HallId</value>: identifier of a conference Hall <br/>
    /// <value>BeginTime; EndTime</value>: a time frame of reservation <br/>
    /// <value>Active</value>: mark if reservation is active or cancelled <br/>
    /// <value>Services</value>: optional string of services and additional fees <br/>
    /// <value>TotalPrice; BaseFee; ServiceFee</value>: price for specific reservation <br/>
    /// </summary>
    public class Reservation
    {
        private static int Count = 0;
        public int Id { get; set; }
        public int HallId { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Active { get; set; }
        public string? Services { get; set; }
        public float TotalPrice => BaseFee + ServiceFee;
        public float BaseFee { get; set; }
        public float ServiceFee { get; set; }
        public Hall? Hall { get; set; }
        public Reservation() { }
        public Reservation(int hallId, Hall? hall, DateTime from, DateTime to, bool act = true, string? services = null)
        {
            Reservation.Count++;
            this.Id = Reservation.Count;
            this.HallId = hallId;
            this.Hall = hall;
            this.BeginTime = from;
            this.EndTime = to;
            this.Active = act;
            this.Services = services;

            this.BaseFee = this.CalculateBaseFee();
            this.ServiceFee = this.CalculateServiceFee();
        }

        /// <summary>
        /// Calculate Fee for hall reservation based on hourly price
        /// </summary>
        /// <returns></returns>
        public float CalculateBaseFee()
        {
            int beginTime = this.BeginTime.Hour;
            int endTime = this.EndTime.Hour;

            int baseFare = this.Hall.BasePrice;
            float total = 0;

            for (int i = beginTime; i < endTime; i++)
            {
                float multiplier = 1;
                if (i >= 18 && i < 23) multiplier = 0.8f;
                if (i >= 6 && i < 9) multiplier = 0.9f;
                if (i >= 12 && i < 14) multiplier = 1.15f;

                total += baseFare * multiplier;
            }
            return total;
        }

        /// <summary>
        /// Calculate price for additional Services
        /// </summary>
        /// <returns></returns>
        public float CalculateServiceFee()
        {
            try
            {
                int beginTime = this.BeginTime.Hour;
                int endTime = this.EndTime.Hour;

                int duration = endTime - beginTime;

                float total = 0;

                var serviceMap = this.Hall.GetServiceHashMap();
                var serviceArray = this.Services.Split(',');
                foreach (string s in serviceArray)
                {
                    total += serviceMap[s.ToLower().Trim()] * duration;
                }

                return total;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculate Fee for hall reservation AND additional services
        /// </summary>
        /// <returns></returns>
        public float CalculateTotalFee()
        {
            return this.CalculateBaseFee() + this.CalculateServiceFee();
        }
    }
}
