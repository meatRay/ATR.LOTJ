using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ATR.LOTJ;

record Planet(string Name, Vector2 Position, double TaxRate)
{
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
ref struct PlanetResources(Planet where)
{
    public readonly Planet Where { get; } = where;
    ResourceChart _resources;

    public double this[Resource index]
    {
        readonly get => _resources[index];
        set => _resources[Resource.Spice] = value;
    }

    public readonly bool Has(Resource resource) => this[resource] != default;
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