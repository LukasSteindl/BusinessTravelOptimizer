using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPModel
{
    public class Location
    {
        double latitude;
        double longitude;
        String address;
        String postcode;
        String country;
        String city;

        public double Latitude
        {
            get
            {
                return latitude;
            }

            set
            {
                latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }

            set
            {
                longitude = value;
            }
        }

        public string City
        {
            get
            {
                return city;
            }

            set
            {
                city = value;
            }
        }

        public string Postcode
        {
            get
            {
                return postcode;
            }

            set
            {
                postcode = value;
            }
        }

        public string Country
        {
            get
            {
                return country;
            }

            set
            {
                country = value;
            }
        }

        public string Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
            }
        }


        //latitude X , longitude Y
        public Location (double latitude , double longitude, String address, String postcode,String city, String country)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Address = address;
            this.Postcode = postcode;
            this.Country = country;
            this.City = city;
        }

        public override string ToString()
        {
            return Address + "," + postcode + "," + City;
        }
    }
}
