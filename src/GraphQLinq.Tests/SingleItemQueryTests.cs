using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HSL;
using NUnit.Framework;

namespace GraphQLinq.Tests
{
    [TestFixture]
    [Category("Single item query")]
    [Category("Integration tests")]
    class SingleItemQueryTests
    {
        const string AgencyId = "LINKKI:54836";
        const string TripId = "OULU:0000880601314021";

        readonly HslGraphContext hslGraphContext = new HslGraphContext("https://api.digitransit.fi/routing/v1/routers/finland/index/graphql");

        [Test]
        public async Task SelectingSingleTripIdIsNotNull()
        {
            var tripId = await hslGraphContext.Trip(TripId).Select(t => t.GtfsId).ToItem();

            Assert.That(tripId, Is.Not.Null);
        }

        [Test]
        public async Task SelectingNestedPropertiesOfSingleTripNestedPropertiesAreNotNull()
        {
            var item = await hslGraphContext.Trip(TripId)
                .Select(trip => new TripDetails(trip.GtfsId, trip.Route.GtfsId, trip.Pattern.Geometry, trip.Route.Agency.Name, trip.Route.Agency.Phone))
                .ToItem();

            Assert.Multiple(() =>
            {
                Assert.That(item.TripId, Is.Not.Null);
                Assert.That(item.RouteId, Is.Not.Null);
                Assert.That(item.Geometry, Is.Not.Null);
                Assert.That(item.Name, Is.Not.Null);
                //Assert.That(item.Phone, Is.Not.Null);
            });
        }

        [Test]
        public async Task SelectingNestedPropertiesOfSingleTripAndCallingConstructorNestedPropertiesAreNotNull()
        {
            var item = await hslGraphContext.Trip(TripId)
                .Select(trip => new TripDetails(trip.GtfsId, trip.Route.GtfsId, trip.Pattern.Geometry, trip.Route.Agency.Name, trip.Route.Agency.Phone))
                .ToItem();

            Assert.Multiple(() =>
            {
                Assert.That(item.TripId, Is.Not.Null);
                Assert.That(item.RouteId, Is.Not.Null);
                Assert.That(item.Geometry, Is.Not.Null);
                Assert.That(item.Name, Is.Not.Null);
                //Assert.That(item.Phone, Is.Not.Null);
            });
        }

        [Test]
        public async Task SelectingThreeLevelNestedPropertyOfSingleTripNestedPropertyIsNotNull()
        {
            var routes = await hslGraphContext.Trip(TripId).Select(trip => trip.Route.Agency.Routes).ToItem();

            CollectionAssert.IsNotEmpty(routes);
        }

        [Test]
        public async Task SelectingSingleTripNestedPropertyIsNull()
        {
            var trip = await hslGraphContext.Trip(TripId).ToItem();

            Assert.That(trip.Pattern, Is.Null);
        }

        [Test]
        public async Task SelectingAndIncludingNestedPropertySingleTripNestedPropertyIsNotNull()
        {
            var pattern = await hslGraphContext.Trip(TripId).Include(trip => trip.Route).ToItem();

            Assert.That(pattern.Route, Is.Not.Null);
        }

        [Test]
        public void SelectingListOfListNestedPropertyShouldCheckListTypeRecursively()
        {
            Agency agency = null;

            Assert.DoesNotThrowAsync(async () => agency = await hslGraphContext.Agency(AgencyId).Include(a => a.Routes.Select(route => route.Trips.Select(trip => trip.Geometry))).ToItem());

            if (agency == null)
            {
                Assert.Inconclusive($"Agency with id {AgencyId} not found");
            }
            else
            {
                Assert.Multiple(() =>
                {
                    CollectionAssert.IsNotEmpty(agency.Routes[0].Trips[0].Geometry);
                    CollectionAssert.IsNotEmpty(agency.Routes[1].Trips[0].Geometry);
                });
            }
        }
    }

    class TripDetails
    {
        public string TripId { get; }
        public string RouteId { get; }
        public List<Coordinates> Geometry { get; }
        public string Name { get; }
        public string Phone { get; }

        internal TripDetails(string tripId, string routeId, List<Coordinates> geometry, string name, string phone)
        {
            TripId = tripId;
            RouteId = routeId;
            Geometry = geometry;
            Name = name;
            Phone = phone;
        }
    }
}