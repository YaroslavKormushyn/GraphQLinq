using System.Linq;
using HSL;
using NUnit.Framework;
using GraphQLinq.Attributes;

namespace GraphQLinq.Tests
{
    [TestFixture(Category = "Query generation tests")]
    public class MethodIncludeQueryGenerationTests
    {
        HslGraphContext hslGraphContext = new HslGraphContext("http://example.com");


        [Test]
        public void IncludingNavigationMethodQueryIncludesIncludedNavigationMethodWithNestedProperties()
        {
            var agencyQuery = hslGraphContext.Agency("248798")
                .Include(agency => agency.Routes.Select(route => route.Trips.Select(trip => trip.stoptimesForDate("20170427"))));

            Assert.That(agencyQuery.Query, Does
                .Contain(typeof(Trip).GetGraphQLNameFromMember(nameof(Trip.StoptimesForDate)))
                .And.Contains(typeof(Stoptime).GetGraphQLNameFromMember(nameof(Stoptime.ScheduledArrival)))
                .And.Contains(typeof(Stoptime).GetGraphQLNameFromMember(nameof(Stoptime.RealtimeArrival))));
        }

        [Test]
        public void IncludingNavigationMethodQueryIncludesIncludedNavigationMethodParameter()
        {
            var agencyQuery = hslGraphContext.Agency("248798")
                .Include(agency => agency.Routes.Select(route => route.Trips.Select(trip => trip.stoptimesForDate("20170427"))));

            Assert.That(agencyQuery.Query, Does.Contain("$serviceDay1: String!").And.Contains("stoptimesForDate(serviceDay: $serviceDay1)"));
        }

        [Test]
        public void IncludingNavigationMethodQueryVariablesIncludesIncludedNavigationMethodArgument()
        {
            var agencyQuery = hslGraphContext.Agency("248798")
                .Include(agency => agency.Routes.Select(route => route.Trips.Select(trip => trip.stoptimesForDate("20170427"))));

            Assert.That(agencyQuery.QueryVariables, Does.ContainKey("serviceDay1").With.ContainValue("20170427"));
        }
    }
}