

using System;

namespace DialogDemoBot
{
    public class BookingData
    {
        public string EmployeeId { get; set; }
        public int AssetId { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan TimeFrom { get; set; }
        public TimeSpan TimeTo { get; set; }
    }
}
