using ATR.LOTJ;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

PlanetShape arkania     = new(new(47, 34), 0.15);
PlanetShape ryloth      = new(new(47, 34), 0.15);
PlanetShape wroona      = new(new(29, 35), 0.20);
PlanetShape ithor       = new(new(20, 17), 0.20);
PlanetShape corellia    = new(new(02, 19), 0.20);
PlanetShape coruscant   = new(new(00, 00), 0.20);
PlanetShape alderaan    = new(new(18, -4), 0.20);
PlanetShape nalhutta    = new(new(81, 45), 0.20);
PlanetShape moncalamari = new(new(59, 50), 0.20);
PlanetShape tatooine    = new(new(59, 68), 0.15);
PlanetShape kashyyyk    = new(new(35, 49), 0.15);

Span<PlanetResources> world = stackalloc PlanetResources[] {
    new(arkania){
        [Resource.Common] = 31.2,
        [Resource.Electronics] = 26.00,
        [Resource.Food] = 23.90,
        [Resource.Precious] = 56.00,
    },
    new(ryloth) {
        [Resource.Common] = 33.60,
        [Resource.Electronics] = 38.50,
        [Resource.Food] = 25.70,
        [Resource.Precious] = 56.40,
        [Resource.Spice] = 93.71,
        [Resource.Water] = 19.90,
        [Resource.Weapons] = 36.00,
    },
    new(wroona){
        [Resource.Common] = 30.4,
        [Resource.Electronics] = 38.80,
        [Resource.Precious] = 68.53,
        [Resource.Spice] = 98.4,
        [Resource.Water] = 7.31,
    },
    new(ithor),
    new(corellia),
    new(alderaan),
    new(coruscant),
};
Dictionary<PlanetShape, ResourceChart> source = new() {
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

var planets =

foreach (var kvp in source)
{
    PlanetResources sourceresource = new(kvp.Key, kvp.Value);

}

void Visit(in PlanetResources from)
{
    for (Resource r = Resource.Common; r <= Resource.Weapons; ++r)
    {
        foreach (var kvp in source)
        {
            PlanetResources destresource = new(kvp.Key, kvp.Value);
            var route = from.RouteFor(r, in destresource);
        }
    }
}