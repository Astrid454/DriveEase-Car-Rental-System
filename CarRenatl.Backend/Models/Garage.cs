using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Backend.Models
{
    public class Garage
    {

        public int GarageId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
    }
}
