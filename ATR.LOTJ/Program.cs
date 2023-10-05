using System.Diagnostics.CodeAnalysis;
using System.Numerics;

Dictionary<Planet, Dictionary<Resource, double>> source = new() {
    [new("Arkania", new(47, 34), 0.15)] = new() {
        [Resource.Common] = 31.2,
        [Resource.Electronics] = 26.00,
        [Resource.Food] = 23.90,
        [Resource.Precious] = 56.00,
    },
    [new("Ryloth", new(53, 21), 0.20)] = new() {
        [Resource.Common] = 33.6,
        [Resource.Electronics] = 38.5,
        [Resource.Food] = 25.7,
        [Resource.Precious] = 56.69,
        [Resource.Spice] = 91.40,
        [Resource.Water] = 19.28,
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
        [Resource.Common] = 19.07,
        [Resource.Electronics] = 38.90,
        [Resource.Food] = 23.8,
        [Resource.Precious] = 73.6,
        [Resource.Spice] = 105.2,
        [Resource.Weapons] = 54.5,
    },
    [new("Alderaan", new(18, -4), 0.20)] = new() {
        [Resource.Common] = 29.9,
        [Resource.Electronics] = 38.8,
        [Resource.Food] = 11.00,
        [Resource.Precious] = 72.6,
    },
    [new("Coruscant", new(0, 0), 0.20)] = new() {
        [Resource.Common] = 28.90,
        [Resource.Electronics] = 26.59,
        [Resource.Food] = 23.80,
        [Resource.Spice] = 199.0,//110.0,
        [Resource.Water] = 23.1,
    },
    [new("Kashyyyk", new(35, 49), 0.15)] = new() {
        [Resource.Common] = 30.30,
        [Resource.Electronics] = 38.90,
        [Resource.Food] = 11.00,
    },
    [new("Mon Calamari", new(59, 50), 0.20)] = new() {
        [Resource.Common] = 27.36,
        [Resource.Electronics] = 26.00,
        [Resource.Water] = 6.12,
    },
    [new("Nal Hutta", new(81, 45), 0.05)] = new() {
        [Resource.Common] = 30.5,
        [Resource.Electronics] = 39.2,
        [Resource.Food] = 28.3,
        [Resource.Precious] = 71.1,
        [Resource.Spice] = 76.04,
        [Resource.Weapons] = 51.4,
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
        foreach (var resource_a in planet_a.Value)
        {
            if (planet_b.Value.TryGetValue(resource_a.Key, out var resource_b))
            {
                if (resource_a.Value > resource_b)
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


enum Resource { Common, Electronics, Food, Precious, Spice, Water, Weapons }
record Planet(string Name, Vector2 Position, double TaxRate);
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