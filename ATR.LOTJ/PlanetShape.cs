using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ATR.LOTJ;

record struct PlanetShape(Vector2 Position, double TaxRate)
{
    public double TaxInvert => 1.0 - TaxRate;
    public double ApplyTax(double income) => income * TaxInvert;
}

record ShipOptions(double Hyperspace, double Speed)
{
    public double Reach => Hyperspace * 1.0;
    public double Time(double distance) => distance / Speed;
}

enum Resource
{
    Common = 0,
    Electronics = 1,
    Food = 2,
    Precious = 3,
    Spice = 4,
    Water = 5,
    Weapons = 6,
}
record struct PlanetResources(PlanetShape where)
{
    public readonly PlanetShape Where { get; } = where;
    ResourceChart _resources;

    public PlanetResources(PlanetShape where, in ResourceChart resources)
        : this(where)
        => _resources = resources;

    public double this[Resource index]
    {
        readonly get => _resources[index];
        set => _resources[Resource.Spice] = value;
    }

    public readonly bool Has(Resource resource) => this[resource] != default;

    public readonly Travel RouteFor(Resource resource, in PlanetResources other_planet) => RouteFor(resource, in this, in other_planet);
    public static Travel RouteFor(Resource resource, in PlanetResources planet_a, in PlanetResources planet_b)
    {
        var value_a = planet_a[resource];
        var value_b = planet_b[resource];
        if (value_a == default || value_b == default)
            return default;
        if (value_a > value_b)
            return new(resource, planet_a.Where, value_a, planet_b.Where, value_b);
        else
            return new(resource, planet_b.Where, value_b, planet_a.Where, value_a);
    }

    public static implicit operator PlanetResources(PlanetShape shape) => new(shape);
}

readonly record struct Travel(Resource Resource, PlanetShape Source, double PurchasePrice, PlanetShape Market, double SalePrice)
{
    public double TotalProfit { get; } = Market.ApplyTax(9500 * Math.Abs(PurchasePrice - SalePrice));
    public double Distance { get; } = Vector2.Distance(Market.Position, Source.Position);
    public double Time => (Distance < 60 ? 5.0 : 10) + (0.1111 * Distance);
    public double ProfitPerSpace => TotalProfit / Distance;
    public double ProfitPerTrip => TotalProfit / Time;
}

[System.Runtime.CompilerServices.InlineArray(8)]
struct ResourceChart
{
    double _rate0;
    public double this[Resource index]
    {
        readonly get => this[(int)index];
        set => this[(int)index] = value;
    }

    public readonly bool Has(Resource resource) => this[resource] != default;
}