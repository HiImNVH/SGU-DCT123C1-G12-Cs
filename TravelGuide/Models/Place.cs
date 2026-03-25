using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelGuide.Models
{

   
        public class Place
        {
            public string location_name { get; set; }
            public string city { get; set; }
            public string description { get; set; }
            public string image_url { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
