using ATR.LOTJ;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

Dictionary<Planet, ResourcesGraph> source = new() {
    [new("Arkania", new(47, 34), 0.15)] = new() {
        [Resource.Common] = 31.2,
        [Resource.Electronics] = 26.00,
        [Resource.Food] = 23.90,
        [Resource.Precious] = 56.00,
    },
    [new("Ryloth", new(53, 21), 0.20)] = new() {
        [Resource.Common] = 33.60,
        [Resource.Electronics] = 38.50,
        [Resource.Food] = 25.70,
        [Resource.Precious] = 56.40,
        [Resource.Spice] = 93.71,
        [Resource.Water] = 19.90,
        [Resource.Weapons] = 36.00,
    },
    [new("Wroona", new(29, 35), 0.20)] = new() {
        [Resource.Common] = 30.4,
        [Resource.Electronics] = 38.80,
        [Resource.Precious] = 68.53,
        [Resource.Spice] = 98.4,
        [Resource.Water] = 7.31,
    },
    [new("Ithor", new(20, 17), 0.20)] = new() {
        [Resource.Common] = 28.8,
        [Resource.Electronics] = 39.7,
        [Resource.Food] = 11.00,
    },
    [new("Corellia", new(2, 19), 0.20)] = new() {
        [Resource.Common] = 19.97,
        [Resource.Electronics] = 38.55,
        [Resource.Food] = 23.56,
        [Resource.Precious] = 73.60,
        [Resource.Spice] = 118.92,
        [Resource.Weapons] = 54.50,
    },
    [new("Alderaan", new(18, -4), 0.20)] = new() {
        [Resource.Common] = 29.9,
        [Resource.Electronics] = 38.8,
        [Resource.Food] = 11.00,
        [Resource.Precious] = 72.6,
    },
    [new("Coruscant", new(0, 0), 0.20)] = new() {
        [Resource.Common] = 28.66,
        [Resource.Electronics] = 27.04,
        [Resource.Food] = 23.80,
        [Resource.Spice] = 110.83,
        [Resource.Water] = 23.10,
    },
    [new("Kashyyyk", new(35, 49), 0.15)] = new() {
        [Resource.Common] = 28.33,
        [Resource.Electronics] = 38.85,
        [Resource.Food] = 13.54,
    },
    [new("Mon Calamari", new(59, 50), 0.20)] = new() {
        [Resource.Common] = 27.36,
        [Resource.Electronics] = 26.00,
        [Resource.Water] = 6.12,
    },
    [new("Nal Hutta", new(81, 45), 0.20)] = new() {
        [Resource.Common] = 30.50,
        [Resource.Electronics] = 39.20,
        [Resource.Food] = 28.30,
        [Resource.Precious] = 71.10,
        [Resource.Spice] = 76.00,
        [Resource.Weapons] = 51.40,
    },
    [new("Tatooine", new(59, 68), 0.15)] = new() {
        [Resource.Common] = 19.97,
        [Resource.Electronics] = 38.80,
        [Resource.Food] = 36.62,
        [Resource.Spice] = 90.44,
        [Resource.Water] = 18.80,
        [Resource.Weapons] = 53.60,
    },
};
List<Route> routes = new();
Dictionary<Planet, Dictionary<Planet, Route>> bests = new();
foreach (var planet_a in source)
{
    Dictionary<Planet, Route> forthis = new();
    bests.Add(planet_a.Key, forthis);
    foreach (var planet_b in source)
    {
        if (planet_a.Key == planet_b.Key)
            continue;
        Route? found = null;
        for (var resource_a = Resource.Min; resource_a < Resource.Max; resource_a = (Resource)((int)resource_a << 1))
        //foreach (var resource_a in planet_a.Value)
        {
            if (planet_a.Value[resource_a] is double.NaN)
                continue;

            if (planet_b.Value[resource_a] != double.NaN)
            {
                if (resource_a[Resource_a] > resource_b)
                    continue;
                var trying = new Route(
                        resource_a.Key,
                        new GoingRate(resource_a.Value, planet_a.Key),
                        new GoingRate(resource_b, planet_b.Key));
                //if (routes.Any(r => r.Similar(trying)))
                //continue;
                routes.Add(trying);
                if (found is null || trying.ProfitPerTrip > found.ProfitPerTrip)
                    found = trying;
            }
        }
        if (found is not null)
            forthis.Add(planet_b.Key, found);
    }
}

foreach (var k in source.Keys)
{
    var best = bests[k].Values.OrderByDescending(m => m.ProfitPerTrip);
    Console.WriteLine($"[ {k.Name} ]");
    foreach (var r in best.Take(5))
    {
        Console.WriteLine($"{r.Distance:0}\t-> {r.ProfitPerTrip:0.00} : {r.Sell.Source.Name}\t{r.Resource}");
    }
}


record GoingRate(double Cost, Planet Source);
record Route(Resource Resource, GoingRate Buy, GoingRate Sell)
{
    public double TotalProfit { get; } = (1.0 - Sell.Source.TaxRate) * 9500 * Math.Abs(Buy.Cost - Sell.Cost);
    public double Distance { get; } = Vector2.Distance(Buy.Source.Position, Sell.Source.Position);
    public double Time => (Distance < 60 ? 5.0 : 10) + (0.1111 * Distance);
    public double ProfitPerSpace => TotalProfit / Distance;
    public double ProfitPerTrip => TotalProfit / Time;

    public bool Similar(Route other)
        => Resource == other.Resource && Sell.Source == other.Sell.Source && Buy.Source == other.Buy.Source;

    public override string ToString() => $"{Resource}, {Buy.Source.Name}->{Sell.Source.Name}, {ProfitPerTrip}";
}