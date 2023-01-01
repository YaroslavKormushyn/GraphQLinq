namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    public static class QueryExtensions
    {
        [GraphQL(Name = "openingHours")]
        public static List<LocalTimeSpanDate> OpeningHours(this BikePark bikePark, List<string> dates)
        {
            return bikePark.OpeningHours;
        }

        [GraphQL(Name = "openingHours")]
        public static List<LocalTimeSpanDate> OpeningHours(this CarPark carPark, List<string> dates)
        {
            return carPark.OpeningHours;
        }

        [GraphQL(Name = "stoptimes")]
        public static List<Stoptime> Stoptimes(this DepartureRow departureRow, long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            return departureRow.Stoptimes;
        }

        [GraphQL(Name = "tripsForDate")]
        public static List<Trip> TripsForDate(this Pattern pattern, string serviceDay, string serviceDate)
        {
            return pattern.TripsForDate;
        }

        [GraphQL(Name = "stopTimesForPattern")]
        public static List<Stoptime> StopTimesForPattern(this Stop stop, string id, long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            return stop.StopTimesForPattern;
        }

        [GraphQL(Name = "transfers")]
        public static List<StopAtDistance> Transfers(this Stop stop, int maxDistance)
        {
            return stop.Transfers;
        }

        [GraphQL(Name = "stoptimesForServiceDate")]
        public static List<StoptimesInPattern> StoptimesForServiceDate(this Stop stop, string date, bool omitNonPickups, bool omitCanceled)
        {
            return stop.StoptimesForServiceDate;
        }

        [GraphQL(Name = "stoptimesForPatterns")]
        public static List<StoptimesInPattern> StoptimesForPatterns(this Stop stop, long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            return stop.StoptimesForPatterns;
        }

        [GraphQL(Name = "stoptimesWithoutPatterns")]
        public static List<Stoptime> StoptimesWithoutPatterns(this Stop stop, long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            return stop.StoptimesWithoutPatterns;
        }

        [GraphQL(Name = "departureStoptime")]
        public static Stoptime DepartureStoptime(this Trip trip, string serviceDate)
        {
            return trip.DepartureStoptime;
        }

        [GraphQL(Name = "arrivalStoptime")]
        public static Stoptime ArrivalStoptime(this Trip trip, string serviceDate)
        {
            return trip.ArrivalStoptime;
        }

        [GraphQL(Name = "stoptimesForDate")]
        public static List<Stoptime> StoptimesForDate(this Trip trip, string serviceDay, string serviceDate)
        {
            return trip.StoptimesForDate;
        }
    }
}