namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Place")]
    public partial class Place
    {
        [GraphQL(Name = "name")]
        public string Name { get; set; }
        [GraphQL(Name = "vertexType")]
        public VertexType VertexType { get; set; }
        [GraphQL(Name = "lat")]
        public float Lat { get; set; }
        [GraphQL(Name = "lon")]
        public float Lon { get; set; }
        [GraphQL(Name = "arrivalTime")]
        public long ArrivalTime { get; set; }
        [GraphQL(Name = "departureTime")]
        public long DepartureTime { get; set; }
        [GraphQL(Name = "stop")]
        public Stop Stop { get; set; }
        [GraphQL(Name = "stopSequence")]
        public int? StopSequence { get; set; }
        [GraphQL(Name = "bikeRentalStation")]
        public BikeRentalStation BikeRentalStation { get; set; }
        [GraphQL(Name = "bikePark")]
        public BikePark BikePark { get; set; }
        [GraphQL(Name = "carPark")]
        public CarPark CarPark { get; set; }
    }
}