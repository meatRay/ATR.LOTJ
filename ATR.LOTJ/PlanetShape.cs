using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ATR.LOTJ;

record struct PlanetShape(Vector2 Position, double TaxRate, bool CanSmuggle = false)
{
    public double TaxInvert => 1.0 - TaxRate;
    public double ApplyTax(double income) => income * TaxInvert;
}

record SearchOptions(double MinProfit, bool Trafficking = false);
record ShipOptions(double Cargo, double Hyperspace, double Speed)
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
struct PlanetResources
{
    [JsonInclude]
    public required PlanetShape Where { get; set; }
    [JsonInclude]
    [JsonConverter(typeof(ResourceChartConverter))]
    ResourceChart _resources;

    public PlanetResources(in ResourceChart resources)
        => _resources = resources;

    public double this[Resource index]
    {
        readonly get => _resources[index];
        set => _resources[index] = value;
    }

    public readonly bool Has(Resource resource) => this[resource] != default;

    public readonly TravelShape RouteFor(Resource resource, in PlanetResources other_planet, bool force_forward = true)
        => RouteFor(resource, in this, in other_planet, force_forward);
    public static TravelShape RouteFor(Resource resource, in PlanetResources planet_a, in PlanetResources planet_b, bool force_forward = true)
    {
        var value_a = planet_a[resource];
        var value_b = planet_b[resource];
        if (value_a == default || value_b == default)
            return default;
        if (value_b > value_a)
            return new(resource, planet_a.Where, value_a, planet_b.Where, value_b);
        else if (!force_forward)
            return new(resource, planet_b.Where, value_b, planet_a.Where, value_a);
        return default;
    }

    public readonly double[] ToArray()
    {
        var array = new double[8];
        for( int i = 0; i < array.Length; ++i)
            array[i] = _resources[i];
        return array;
    }
}

readonly record struct TravelShape(Resource Resource, PlanetShape Source, double PurchasePrice, PlanetShape Market, double SalePrice)
{
    public double Profit => Math.Abs(PurchasePrice - SalePrice);
}
readonly struct TravelAgent(TravelShape shape, SearchOptions search) : IEquatable<TravelAgent>
{
    public double TotalProfit { get; } = search.Trafficking && shape.Market.CanSmuggle
            ? shape.Profit
            : shape.Market.ApplyTax(shape.Profit);
    public double Distance { get; } = Vector2.Distance(shape.Market.Position, shape.Source.Position);
    public double Time => ((Distance < 50 ? 1 : 2) * 6.93) + (0.10889 * Distance);
    //public double Time => ((Distance < 120 ? 1 : 2) * 4.05) + (0.10889 * Distance); //+ (0.9708 * Distance);
    public double ProfitPerSpace => TotalProfit / Distance;
    public double ProfitPerTime => TotalProfit / Time;

    public bool Equals(TravelAgent other) => other.TotalProfit == TotalProfit && other.Distance == Distance;
    public override bool Equals(object? obj) => obj is TravelAgent travel && Equals(travel);
    public override int GetHashCode() => HashCode.Combine(TotalProfit, Distance);

    public static bool operator ==(in TravelAgent left, in TravelAgent right) => left.Equals(right);
    public static bool operator !=(in TravelAgent left, in TravelAgent right) => !(left == right);
}

record Planet(in PlanetShape Shape, in PlanetResources Resources);

[System.Runtime.CompilerServices.InlineArray(8)]
[JsonConverter(typeof(ResourceChartConverter))]
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

class ResourceChartConverter : JsonConverter<ResourceChart>
{
    readonly static JsonConverter<List<double>> _default = (JsonConverter<List<double>>)JsonSerializerOptions.Default.GetConverter(typeof(List<double>));

    public override ResourceChart Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<double>? read = _default.Read(ref reader, typeof(List<double>), options);
        if (read is null)
            return default;
        ResourceChart chart = new();
        for (int i = 0; i < 8 && i < read.Count; ++i)
            chart[i] = read[i];
        return chart;
    }
    public override void Write(Utf8JsonWriter writer, ResourceChart value, JsonSerializerOptions options)
    {

        List<double> chart = new(8);
        for (int i = 0; i < 8; ++i)
            chart.Add(value[i]);
        _default.Write(writer, chart, options);
    }
}