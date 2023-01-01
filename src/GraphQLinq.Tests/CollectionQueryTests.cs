using System.Linq;
using System.Threading.Tasks;
using HSL;
using NUnit.Framework;

namespace GraphQLinq.Tests
{
    [TestFixture]
    [Category("Collection query")]
    [Category("Integration tests")]
    class CollectionQueryTests
    {
        //readonly HslGraphContext hslGraphContext = new HslGraphContext("https://api.digitransit.fi/routing/v1/routers/finland/index/graphql");
        readonly QueryContext hslGraphContext = new QueryContext("https://api.digitransit.fi/routing/v1/routers/finland/index/graphql");

        [Test]
        public async Task SelectingNamesReturnsListOfNames()
        {
            var query = hslGraphContext.Stations(null, null, string.Empty, null).Select(l => l.Name);

            var names = await query.ToEnumerable();

            Assert.Multiple(() =>
            {
                CollectionAssert.IsNotEmpty(names);
                CollectionAssert.AllItemsAreNotNull(names);
            });
        }

        [Test]
        public async Task SelectingNamesDoesNotReturnStops()
        {
            var query = hslGraphContext.Stations(null, null, string.Empty, null);

            var stations = await query.ToEnumerable();

            Assert.That(stations, Is.All.Matches<Stop>(l => l.Stops == null));
        }
        
        [Test]
        public async Task SelectingNamesAndIncludingStopsReturnsStops()
        {
            var query = hslGraphContext.Stations(null, null, string.Empty, null).Include(s => s.Stops);

            var stations = await query.ToEnumerable();

            Assert.That(stations, Is.All.Matches<Stop>(s => s.Stops != null));
        }

        [Test]
        public async Task SelectingNamesAndStopsReturnsStops()
        {
            var query = hslGraphContext.Stations(null, null, string.Empty, null).Select(location => new { location.Name, location.Stops });

            var stations = await query.ToEnumerable();

            var stationsWithNullStops = stations.Where(s => s.Stops == null).ToList();
            CollectionAssert.IsEmpty(stationsWithNullStops);
        }

        [Test]
        public async Task SelectingNamesWithAliasAndStopsReturnsStopsAndNames()
        {
            var query = hslGraphContext.Stations(null, null, string.Empty, null).Select(location => new { StationName = location.Name, location.Stops });

            var stations = await query.ToEnumerable();

            var stationsWithNullStops = stations.Where(s => s.Stops == null).ToList();
            var stationsWithNullCity = stations.Where(s => s.StationName == null).ToList();

            Assert.Multiple(() =>
            {
                CollectionAssert.IsEmpty(stationsWithNullStops);
                CollectionAssert.IsEmpty(stationsWithNullCity);
            });
        }
    }
}