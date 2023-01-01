namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "InputFilters")]
    public partial class InputFilters
    {
        [GraphQL(Name = "stops")]
        public List<string> Stops { get; set; }
        [GraphQL(Name = "routes")]
        public List<string> Routes { get; set; }
        [GraphQL(Name = "bikeRentalStations")]
        public List<string> BikeRentalStations { get; set; }
        [GraphQL(Name = "bikeParks")]
        public List<string> BikeParks { get; set; }
        [GraphQL(Name = "carParks")]
        public List<string> CarParks { get; set; }
    }
}