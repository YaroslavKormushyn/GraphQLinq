namespace HSL
{
    using GraphQLinq;
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;
    using System.Net.Http;

    public partial class QueryContext : GraphContext
    {
        public QueryContext() : this("https://api.digitransit.fi/routing/v1/routers/finland/index/graphql")
        {
        }

        public QueryContext(string baseUrl) : base(baseUrl, "")
        {
            SubQueryContext = new QuerySubQueryContext(this);
        }

        public QueryContext(HttpClient httpClient) : base(httpClient)
        {
            SubQueryContext = new QuerySubQueryContext(this);
        }

        public GraphItemQuery<Node> Node(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Node>(parameterValues, "node");
        }

        public GraphCollectionQuery<Feed> Feeds()
        {
            var parameterValues = new object[] { };
            return BuildCollectionQuery<Feed>(parameterValues, "feeds");
        }

        public GraphCollectionQuery<Agency> Agencies()
        {
            var parameterValues = new object[] { };
            return BuildCollectionQuery<Agency>(parameterValues, "agencies");
        }

        public GraphCollectionQuery<TicketType> TicketTypes()
        {
            var parameterValues = new object[] { };
            return BuildCollectionQuery<TicketType>(parameterValues, "ticketTypes");
        }

        public GraphItemQuery<Agency> Agency(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Agency>(parameterValues, "agency");
        }

        public GraphCollectionQuery<Stop> Stops(List<string> ids, List<string> feeds, string name, int? maxResults)
        {
            var parameterValues = new object[] { ids, feeds, name, maxResults };
            return BuildCollectionQuery<Stop>(parameterValues, "stops");
        }

        public GraphCollectionQuery<Stop> StopsByBbox(float minLat, float minLon, float maxLat, float maxLon, string agency, List<string> feeds)
        {
            var parameterValues = new object[] { minLat, minLon, maxLat, maxLon, agency, feeds };
            return BuildCollectionQuery<Stop>(parameterValues, "stopsByBbox");
        }

        public GraphItemQuery<StopAtDistanceConnection> StopsByRadius(float lat, float lon, int radius, string agency, List<string> feeds, string before, string after, int? first, int? last)
        {
            var parameterValues = new object[] { lat, lon, radius, agency, feeds, before, after, first, last };
            return BuildItemQuery<StopAtDistanceConnection>(parameterValues, "stopsByRadius");
        }

        public GraphItemQuery<PlaceAtDistanceConnection> Nearest(float lat, float lon, int? maxDistance, int? maxResults, List<FilterPlaceType> filterByPlaceTypes, List<Mode> filterByModes, InputFilters filterByIds, string before, string after, int? first, int? last)
        {
            var parameterValues = new object[] { lat, lon, maxDistance, maxResults, filterByPlaceTypes, filterByModes, filterByIds, before, after, first, last };
            return BuildItemQuery<PlaceAtDistanceConnection>(parameterValues, "nearest");
        }

        public GraphItemQuery<DepartureRow> DepartureRow(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<DepartureRow>(parameterValues, "departureRow");
        }

        public GraphItemQuery<Stop> Stop(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Stop>(parameterValues, "stop");
        }

        public GraphItemQuery<Stop> Station(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Stop>(parameterValues, "station");
        }

        public GraphCollectionQuery<Stop> Stations(List<string> ids, List<string> feeds, string name, int? maxResults)
        {
            var parameterValues = new object[] { ids, feeds, name, maxResults };
            return BuildCollectionQuery<Stop>(parameterValues, "stations");
        }

        public GraphCollectionQuery<Route> Routes(List<string> ids, List<string> feeds, string name, string modes, List<Mode> transportModes)
        {
            var parameterValues = new object[] { ids, feeds, name, modes, transportModes };
            return BuildCollectionQuery<Route>(parameterValues, "routes");
        }

        public GraphItemQuery<Route> Route(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Route>(parameterValues, "route");
        }

        public GraphCollectionQuery<Trip> Trips(List<string> feeds)
        {
            var parameterValues = new object[] { feeds };
            return BuildCollectionQuery<Trip>(parameterValues, "trips");
        }

        public GraphItemQuery<Trip> Trip(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Trip>(parameterValues, "trip");
        }

        public GraphItemQuery<Trip> FuzzyTrip(string route, int? direction, string date, int time)
        {
            var parameterValues = new object[] { route, direction, date, time };
            return BuildItemQuery<Trip>(parameterValues, "fuzzyTrip");
        }

        public GraphCollectionQuery<Stoptime> CancelledTripTimes(List<string> feeds, List<string> routes, List<string> patterns, List<string> trips, string minDate, string maxDate, int? minDepartureTime, int? maxDepartureTime, int? minArrivalTime, int? maxArrivalTime)
        {
            var parameterValues = new object[] { feeds, routes, patterns, trips, minDate, maxDate, minDepartureTime, maxDepartureTime, minArrivalTime, maxArrivalTime };
            return BuildCollectionQuery<Stoptime>(parameterValues, "cancelledTripTimes");
        }

        public GraphCollectionQuery<Pattern> Patterns()
        {
            var parameterValues = new object[] { };
            return BuildCollectionQuery<Pattern>(parameterValues, "patterns");
        }

        public GraphItemQuery<Pattern> Pattern(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Pattern>(parameterValues, "pattern");
        }

        public GraphCollectionQuery<Cluster> Clusters()
        {
            var parameterValues = new object[] { };
            return BuildCollectionQuery<Cluster>(parameterValues, "clusters");
        }

        public GraphItemQuery<Cluster> Cluster(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<Cluster>(parameterValues, "cluster");
        }

        public GraphCollectionQuery<Alert> Alerts(List<string> feeds, List<AlertSeverityLevelType> severityLevel, List<AlertEffectType> effect, List<AlertCauseType> cause, List<string> route, List<string> stop)
        {
            var parameterValues = new object[] { feeds, severityLevel, effect, cause, route, stop };
            return BuildCollectionQuery<Alert>(parameterValues, "alerts");
        }

        public GraphItemQuery<ServiceTimeRange> ServiceTimeRange()
        {
            var parameterValues = new object[] { };
            return BuildItemQuery<ServiceTimeRange>(parameterValues, "serviceTimeRange");
        }

        public GraphCollectionQuery<BikeRentalStation> BikeRentalStations(List<string> ids)
        {
            var parameterValues = new object[] { ids };
            return BuildCollectionQuery<BikeRentalStation>(parameterValues, "bikeRentalStations");
        }

        public GraphItemQuery<BikeRentalStation> BikeRentalStation(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<BikeRentalStation>(parameterValues, "bikeRentalStation");
        }

        public GraphCollectionQuery<BikePark> BikeParks()
        {
            var parameterValues = new object[] { };
            return BuildCollectionQuery<BikePark>(parameterValues, "bikeParks");
        }

        public GraphItemQuery<BikePark> BikePark(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<BikePark>(parameterValues, "bikePark");
        }

        public GraphCollectionQuery<CarPark> CarParks(List<string> ids)
        {
            var parameterValues = new object[] { ids };
            return BuildCollectionQuery<CarPark>(parameterValues, "carParks");
        }

        public GraphItemQuery<CarPark> CarPark(string id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<CarPark>(parameterValues, "carPark");
        }

        public GraphItemQuery<Plan> Plan(string date, string time, InputCoordinates from, InputCoordinates to, string fromPlace, string toPlace, bool? wheelchair, int? numItineraries, float? maxWalkDistance, int? maxPreTransitTime, float? maxSlope, float? carParkCarLegWeight, float? itineraryFiltering, float? walkReluctance, float? walkOnStreetReluctance, float? waitReluctance, float? waitAtBeginningFactor, float? walkSpeed, float? bikeSpeed, int? bikeSwitchTime, int? bikeSwitchCost, OptimizeType optimize, InputTriangle triangle, bool? arriveBy, List<InputCoordinates> intermediatePlaces, InputPreferred preferred, InputUnpreferred unpreferred, int? walkBoardCost, int? bikeBoardCost, InputBanned banned, int? transferPenalty, bool? batch, string modes, List<TransportMode> transportModes, InputModeWeight modeWeight, bool? allowBikeRental, int? boardSlack, int? alightSlack, int? minTransferTime, int? nonpreferredTransferPenalty, int? maxTransfers, string startTransitStopId, string startTransitTripId, long? claimInitialWait, bool? reverseOptimizeOnTheFly, bool? omitCanceled, bool? ignoreRealtimeUpdates, bool? disableRemainingWeightHeuristic, string locale, string ticketTypes, List<string> allowedTicketTypes, int? heuristicStepsPerMainStep, bool? compactLegsByReversedSearch, List<string> allowedBikeRentalNetworks)
        {
            var parameterValues = new object[] { date, time, from, to, fromPlace, toPlace, wheelchair, numItineraries, maxWalkDistance, maxPreTransitTime, maxSlope, carParkCarLegWeight, itineraryFiltering, walkReluctance, walkOnStreetReluctance, waitReluctance, waitAtBeginningFactor, walkSpeed, bikeSpeed, bikeSwitchTime, bikeSwitchCost, optimize, triangle, arriveBy, intermediatePlaces, preferred, unpreferred, walkBoardCost, bikeBoardCost, banned, transferPenalty, batch, modes, transportModes, modeWeight, allowBikeRental, boardSlack, alightSlack, minTransferTime, nonpreferredTransferPenalty, maxTransfers, startTransitStopId, startTransitTripId, claimInitialWait, reverseOptimizeOnTheFly, omitCanceled, ignoreRealtimeUpdates, disableRemainingWeightHeuristic, locale, ticketTypes, allowedTicketTypes, heuristicStepsPerMainStep, compactLegsByReversedSearch, allowedBikeRentalNetworks };
            return BuildItemQuery<Plan>(parameterValues, "plan");
        }
    }
}