namespace HSL
{
    using GraphQLinq;
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    public class QuerySubQueryContext : SubQueryContext
    {
        public QuerySubQueryContext(GraphContext context) : base(context)
        {
        }

        [GraphQL(Name = "openingHours")]
        public GraphCollectionQuery<LocalTimeSpanDate> OpeningHours(List<string> dates)
        {
            var parameterValues = new object[] { dates };
            return BuildCollectionQuery<LocalTimeSpanDate>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(OpeningHours)));
        }

        [GraphQL(Name = "stoptimes")]
        public GraphCollectionQuery<Stoptime> Stoptimes(long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            var parameterValues = new object[] { startTime, timeRange, numberOfDepartures, omitNonPickups, omitCanceled };
            return BuildCollectionQuery<Stoptime>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(Stoptimes)));
        }

        [GraphQL(Name = "tripsForDate")]
        public GraphCollectionQuery<Trip> TripsForDate(string serviceDay, string serviceDate)
        {
            var parameterValues = new object[] { serviceDay, serviceDate };
            return BuildCollectionQuery<Trip>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(TripsForDate)));
        }

        [GraphQL(Name = "stopTimesForPattern")]
        public GraphCollectionQuery<Stoptime> StopTimesForPattern(string id, long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            var parameterValues = new object[] { id, startTime, timeRange, numberOfDepartures, omitNonPickups, omitCanceled };
            return BuildCollectionQuery<Stoptime>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(StopTimesForPattern)));
        }

        [GraphQL(Name = "transfers")]
        public GraphCollectionQuery<StopAtDistance> Transfers(int maxDistance)
        {
            var parameterValues = new object[] { maxDistance };
            return BuildCollectionQuery<StopAtDistance>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(Transfers)));
        }

        [GraphQL(Name = "stoptimesForServiceDate")]
        public GraphCollectionQuery<StoptimesInPattern> StoptimesForServiceDate(string date, bool omitNonPickups, bool omitCanceled)
        {
            var parameterValues = new object[] { date, omitNonPickups, omitCanceled };
            return BuildCollectionQuery<StoptimesInPattern>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(StoptimesForServiceDate)));
        }

        [GraphQL(Name = "stoptimesForPatterns")]
        public GraphCollectionQuery<StoptimesInPattern> StoptimesForPatterns(long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            var parameterValues = new object[] { startTime, timeRange, numberOfDepartures, omitNonPickups, omitCanceled };
            return BuildCollectionQuery<StoptimesInPattern>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(StoptimesForPatterns)));
        }

        [GraphQL(Name = "stoptimesWithoutPatterns")]
        public GraphCollectionQuery<Stoptime> StoptimesWithoutPatterns(long startTime, int timeRange, int numberOfDepartures, bool omitNonPickups, bool omitCanceled)
        {
            var parameterValues = new object[] { startTime, timeRange, numberOfDepartures, omitNonPickups, omitCanceled };
            return BuildCollectionQuery<Stoptime>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(StoptimesWithoutPatterns)));
        }

        [GraphQL(Name = "departureStoptime")]
        public GraphItemQuery<Stoptime> DepartureStoptime(string serviceDate)
        {
            var parameterValues = new object[] { serviceDate };
            return BuildItemQuery<Stoptime>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(Stoptime)));
        }

        [GraphQL(Name = "arrivalStoptime")]
        public GraphItemQuery<Stoptime> ArrivalStoptime(string serviceDate)
        {
            var parameterValues = new object[] { serviceDate };
            return BuildItemQuery<Stoptime>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(Stoptime)));
        }

        [GraphQL(Name = "stoptimesForDate")]
        public GraphCollectionQuery<Stoptime> StoptimesForDate(string serviceDay, string serviceDate)
        {
            var parameterValues = new object[] { serviceDay, serviceDate };
            return BuildCollectionQuery<Stoptime>(parameterValues, GetType().GetGraphQLNameFromMethod(nameof(StoptimesForDate)));
        }
    }
}