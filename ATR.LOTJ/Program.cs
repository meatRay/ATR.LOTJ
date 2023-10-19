using ATR.LOTJ;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using Terminal.Gui;

ColorGraph colors = new();
Console.Write("First Color\t: ");
int firstx = int.Parse(Console.ReadLine()!);
Console.Write("Second Color\t: ");
int secondx = int.Parse(Console.ReadLine()!);
Console.Write("Item Name\t: ");
string text = Console.ReadLine()!;

ColorSpace startspace = colors.Decompile(firstx);//new(1, 1, 1);//colors.Decompile(firstx);
ColorSpace endspace = colors.Decompile(secondx);// new(3, 2, 2);//colors.Decompile(secondx);

var firstxx = colors.Compile(in startspace);
var secondxx = colors.Compile(in endspace);

int xcode = 0;
//ColorDither dither = new(startspace, endspace, text.Length);
var tokencount = ColorSpace.Magnitude(endspace - startspace) + 1;
Span<ColorSpace> spaces = stackalloc ColorSpace[tokencount];

static void RunOn(ColorDither dither, ColorGraph graph)
{
    Console.Write('\t');
    for (int i = 0; i < dither.Length; ++i)
    {
        Console.Write($"\x1b[38;5;{graph.Compile(dither.SmallandMove())}m");
        Console.Write((char)('a' + i));
    }
    Console.WriteLine("\x1b[0m");
}
static void CodesOn(ColorDither dither, ColorGraph graph)
{
    Console.Write('\t');
    for (int i = 0; i < dither.Length; ++i)
    {
        Console.Write($"&{graph.Compile(dither.SmallandMove()):000}");
        Console.Write((char)('a' + i));
    }
    Console.WriteLine();
}

Console.WriteLine("Path of MAX magnitude");
Console.WriteLine("RED -> GREEN -> BLUE");
RunOn(new(startspace, endspace, tokencount, ColorOrient.RedGreen), colors);
CodesOn(new(startspace, endspace, tokencount, ColorOrient.RedGreen), colors);

Console.WriteLine("GREEN -> BLUE -> RED");
RunOn(new(startspace, endspace, tokencount, ColorOrient.GreenBlue), colors);
CodesOn(new(startspace, endspace, tokencount, ColorOrient.GreenBlue), colors);

Console.WriteLine("BLUE -> RED -> GREEN");
RunOn(new(startspace, endspace, tokencount, ColorOrient.BlueRed), colors);
CodesOn(new(startspace, endspace, tokencount, ColorOrient.BlueRed), colors);

Console.WriteLine("GREEN -> RED -> BLUE");
RunOn(new(startspace, endspace, tokencount, ColorOrient.GreenRed), colors);
CodesOn(new(startspace, endspace, tokencount, ColorOrient.GreenRed), colors);

Console.WriteLine("RED -> BLUE -> GREEN");
RunOn(new(startspace, endspace, tokencount, ColorOrient.RedBlue), colors);
CodesOn(new(startspace, endspace, tokencount, ColorOrient.RedBlue), colors);

Console.WriteLine("BLUE -> GREEN -> RED");
RunOn(new(startspace, endspace, tokencount, ColorOrient.BlueGreen), colors);
CodesOn(new(startspace, endspace, tokencount, ColorOrient.BlueGreen), colors);
Console.WriteLine();


var travel = endspace - startspace;
ColorDither dither = new(startspace, endspace, 1 + int.Max(int.Abs(travel.Red), int.Max(int.Abs(travel.Green), int.Abs(travel.Blue))));
Console.WriteLine("Path of MIN magnitude");
Console.Write('\t');
for (int i = 0; i < dither.Length; ++i)
{
    ColorSpace space = dither.TakeandMove();

    var trycode = colors.Compile(in space);
    Console.Write($"\x1b[38;5;{trycode}m");
    Console.Write((char)('a' + i));
    xcode = trycode;
}
Console.WriteLine("\x1b[0m");
dither = new(startspace, endspace, 1 + int.Max(int.Abs(travel.Red), int.Max(int.Abs(travel.Green), int.Abs(travel.Blue))));
Console.Write('\t');
for (int i = 0; i < dither.Length; ++i)
{
    ColorSpace space = dither.TakeandMove();

    var trycode = colors.Compile(in space);
    if (trycode != xcode)
        Console.Write($"&{trycode:000}");
    Console.Write((char)('a' + i));
    xcode = trycode;
}

//Application.Run<LotJWindow>();
//Application.Shutdown();
return;

ShipOptions ship = new(9500, 40, 40);
SearchOptions config = new(9000);

PlanetShape arkania = new(new(47, 34), 0.05);
PlanetShape ryloth = new(new(53, 21), 0.20);
PlanetShape wroona = new(new(29, 35), 0.20);
PlanetShape ithor = new(new(20, 17), 0.20);
PlanetShape corellia = new(new(02, 19), 0.20);
PlanetShape coruscant = new(new(00, 00), 0.20);
PlanetShape alderaan = new(new(18, -4), 0.20);
PlanetShape nalhutta = new(new(81, 45), 0.20);
PlanetShape moncalamari = new(new(59, 50), 0.20);
PlanetShape tatooine = new(new(59, 68), 0.15);
PlanetShape kashyyyk = new(new(35, 49), 0.15);

Dictionary<PlanetShape, string> names = new() {
    [arkania] = "Arkania",
    [ryloth] = "Ryloth",
    [wroona] = "Wroona",
    [ithor] = "Ithor",
    [corellia] = "Corellia",
    [coruscant] = "Coruscant",
    [alderaan] = "Alderaan",
    [nalhutta] = "Nal Hutta",
    [moncalamari] = "Mon Calamari",
    [tatooine] = "Tatooine",
    [kashyyyk] = "Kashyyyk",
};
ReadOnlySpan<PlanetResources> world = stackalloc PlanetResources[] {
    new(arkania) {
        [Resource.Common] = 29.24,
        [Resource.Electronics] = 29.89,
        [Resource.Food] = 22.17,
        [Resource.Precious] = 56.00,
    },
    new(ryloth) {
        [Resource.Common] = 33.60,
        [Resource.Electronics] = 38.50,
        [Resource.Food] = 25.70,
        [Resource.Precious] = 56.62,
        [Resource.Spice] = 90.72,
        [Resource.Water] = 19.79,
        [Resource.Weapons] = 36.00,
    },
    new(wroona) {
        [Resource.Common] = 30.4,
        [Resource.Electronics] = 38.80,
        [Resource.Precious] = 68.80,
        [Resource.Spice] = 102.82,
        [Resource.Water] = 6.57,
    },
    new(ithor) {
        [Resource.Common] = 28.8,
        [Resource.Electronics] = 39.11,
        [Resource.Food] = 12.08,
    },
    new(corellia) {
        [Resource.Common] = 17.38,
        [Resource.Electronics] = 38.90,
        [Resource.Food] = 23.80,
        [Resource.Precious] = 73.60,
        [Resource.Spice] = 112.23,
        [Resource.Weapons] = 54.50,
    },
    new(alderaan) {
        [Resource.Common] = 29.9,
        [Resource.Electronics] = 38.8,
        [Resource.Food] = 12.16,
        [Resource.Precious] = 72.6,
    },
    new(coruscant) {
        [Resource.Common] = 28.90,
        [Resource.Electronics] = 26.61,
        [Resource.Food] = 23.80,
        [Resource.Spice] = 109.98,
        [Resource.Water] = 23.10,
    },
    new(kashyyyk) {
        [Resource.Common] = 30.30,
        [Resource.Electronics] = 37.96,
        [Resource.Food] = 12.98,
    },
    new(moncalamari) {
        [Resource.Common] = 27.63,
        [Resource.Electronics] = 26.00,
        [Resource.Water] = 6.33,
    },
    new(nalhutta) {
        [Resource.Common] = 30.50,
        [Resource.Electronics] = 39.20,
        [Resource.Food] = 28.30,
        [Resource.Precious] = 70.44,
        [Resource.Spice] = 77.46,
        [Resource.Weapons] = 51.40,
    },
    new(tatooine) {
        [Resource.Common] = 19.92,
        [Resource.Electronics] = 46.00,
        [Resource.Food] = 25.30,
        [Resource.Spice] = 90.50,
        [Resource.Water] = 18.73,
        [Resource.Weapons] = 53.60,
    }
};


//Console.ReadLine();
Span<Travel> bestroutes = stackalloc Travel[8];
for (int i = 0; i < world.Length; ++i)
{
    Console.WriteLine($"=== {names[world[i].Where]}\t===");

    for (Resource r = Resource.Common; r <= Resource.Weapons; ++r)
    {
        for (int k = 0; k < world.Length; ++k)
        {
            if (i == k)
                continue;
            Travel route = world[i].RouteFor(r, in world[k], force_forward: true);
            if (route != default)
            {
                var profit = route.ProfitPerTime;
                if (profit < config.MinProfit)
                    continue;
                for (int l = 0; l < bestroutes.Length; ++l)
                    if (bestroutes[l].ProfitPerTime < profit)
                    {
                        if (l + 1 < bestroutes.Length)
                            bestroutes[l..^1].CopyTo(bestroutes[(l + 1)..]);
                        bestroutes[l] = route;
                        break;
                    }
            }
        }
    }
    for (int k = 0; k < bestroutes.Length; ++k)
    {
        ref var route = ref bestroutes[k];
        if (route == default)
            break;
        Console.WriteLine($"{route.ProfitPerTime:N0} : {route.Resource} -> {names[route.Market]}");
        route = default;
    }
    Console.WriteLine();
}

class Engine
{

    void Move()
    {

    }
}