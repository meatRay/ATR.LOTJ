using ATR.LOTJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ATR.LOTJ;
class NamedPlanet
{
    public string Name { get; set; }
    public PlanetShape Shape { get; set; }
    public IList<double> Resources { get; set; }
    [JsonConstructor]
    public NamedPlanet(string name, PlanetShape shape, IList<double> resources)
    {
        Name = name;
        Shape = shape;
        Resources = resources;
    }
    public NamedPlanet(string name, PlanetResources resources)
    {
        Name = name;
        Shape = resources.Where;
        Resources = resources.ToArray();
    }
    public PlanetResources ToShorthand()
    {
        ResourceChart chart = new();
        for( int i = 0; i < Resources.Count && i < 8 ; ++i )
            chart[i] = Resources[i];
        return new(in chart) { Where = Shape };
    }
}
class Cargo
{
    public const string Arkania = nameof(Arkania),
        Ryloth = nameof(Ryloth),
        Wroona = nameof(Wroona),
        Ithor = nameof(Ithor),
        Corellia = nameof(Corellia),
        Coruscant = nameof(Coruscant),
        Alderaan = nameof(Alderaan),
        NalHutta = "Nal Hutta",
        MonCalamari = "Mon Calamari",
        Tatooine = nameof(Tatooine),
        Kashyyyk = nameof(Kashyyyk);
    public static IList<string> PlanetNames => planetkeys;
    static readonly List<string> planetkeys = [
        Arkania, Ryloth, Wroona,
        Ithor, Corellia, Coruscant,
        Alderaan, NalHutta, MonCalamari,
        Tatooine, Kashyyyk];
    static IList<NamedPlanet> CreatePlanetDefaults()
        => [
            new NamedPlanet(Arkania,new() {
                Where = new(new(47, 34), 0.20),
                [Resource.Common] = 29.24,
                [Resource.Electronics] = 29.89,
                [Resource.Food] = 22.17,
                [Resource.Precious] = 56.00
            }),
            new NamedPlanet(Ryloth, new() {
                Where = new(new(53, 21), 0.20, true),
                [Resource.Common] = 33.60,
                [Resource.Electronics] = 38.50,
                [Resource.Food] = 25.70,
                [Resource.Precious] = 56.62,
                [Resource.Spice] = 90.72,
                [Resource.Water] = 19.79,
                [Resource.Weapons] = 36.00,
            }),
            new NamedPlanet(Wroona , new() {
                Where =new(new(29, 35), 0.20, true),
                [Resource.Common] = 30.4,
                [Resource.Electronics] = 38.80,
                [Resource.Precious] = 68.80,
                [Resource.Spice] = 98.29,
                [Resource.Water] = 7.32,
            }),
            new NamedPlanet(Ithor , new() {
                Where =new(new(20, 17), 0.20),
                [Resource.Common] = 28.8,
                [Resource.Electronics] = 39.11,
                [Resource.Food] = 12.08,
            }),
            new NamedPlanet(Corellia , new() {
                Where =new(new(02, 19), 0.20),
                [Resource.Common] = 16.09,
                [Resource.Electronics] = 38.90,
                [Resource.Food] = 23.80,
                [Resource.Precious] = 73.60,
                [Resource.Spice] = 105.20,
                [Resource.Weapons] = 54.50,
            }),
            new NamedPlanet(Coruscant , new() {
                Where =new(new(00, 00), 0.20),
                [Resource.Common] = 28.90,
                [Resource.Electronics] = 26.61,
                [Resource.Food] = 23.80,
                [Resource.Spice] = 109.98,
                [Resource.Water] = 23.10,
            }),
            new NamedPlanet(Alderaan , new() {
                Where =new(new(18, -4), 0.20),
                [Resource.Common] = 29.9,
                [Resource.Electronics] = 38.8,
                [Resource.Food] = 12.16,
                [Resource.Precious] = 72.6,
            }),
            new NamedPlanet(NalHutta , new() {
                Where =new(new(81, 45), 0.20, true),
                [Resource.Common] = 30.50,
                [Resource.Electronics] = 39.20,
                [Resource.Food] = 28.30,
                [Resource.Precious] = 70.44,
                [Resource.Spice] = 77.46,
                [Resource.Weapons] = 51.40,
            }),
            new NamedPlanet(MonCalamari , new() {
                Where =new(new(59, 50), 0.20),
                [Resource.Common] = 27.63,
                [Resource.Electronics] = 26.00,
                [Resource.Water] = 6.33,
            }),
            new NamedPlanet(Tatooine , new() {
                Where =new(new(59, 68), 0.15, true),
                [Resource.Common] = 19.56,
                [Resource.Electronics] = 37.55,
                [Resource.Food] = 22.49,
                [Resource.Spice] = 90.50,
                [Resource.Water] = 18.64,
                [Resource.Weapons] = 53.85,
            }),
            new NamedPlanet(Kashyyyk , new() {
                Where =new(new(35, 49), 0.15, true),
                [Resource.Common] = 29.53,
                [Resource.Electronics] = 38.67,
                [Resource.Food] = 14.51,
            }) ];

    static JsonSerializerOptions JsonOptions { get; } = new() {
        //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        IncludeFields = true,
        WriteIndented = true,
        //PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
    };
    public static IList<NamedPlanet> LoadFromFile(string filename = "Planets.json")
    {
        if (File.Exists(filename))
        {
            using var fl = File.OpenRead(filename);
            var got = JsonSerializer.Deserialize<IList<NamedPlanet>>(fl, JsonOptions);
            if (got is not null)
                return got;
            return CreatePlanetDefaults();
        }
        else
        {
            var dict = CreatePlanetDefaults();
            using var fl = File.OpenWrite(filename);
            JsonSerializer.Serialize(fl, dict, JsonOptions);
            return dict;
        }
    }
}
