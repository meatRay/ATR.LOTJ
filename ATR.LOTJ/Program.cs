using ATR.LOTJ;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;

ColorGraph colors = new();

InputData<int> firstcolor = new(0), secondcolor = new(0);

OptionsFrame routeoptions = new("Color Tool",
    new FrameOption[] {
        new("First Color", new InputFrame<int>("First Color: ", firstcolor)),
        new("Second Color", new InputFrame<int>("Second Color: ", secondcolor)),
        new("Calculate", new ComplexFrame(Calculate)),
        new("Main Menu", new SimpleFrame(SimpleWork.Back))
    });
OptionsFrame cargooptions = new("Cargo Tool",
    new FrameOption[] {
        new("Calculate", new ComplexFrame(CargoWork)),
        new("Main Menu", new SimpleFrame(SimpleWork.Back)),
    });
OptionsFrame topoptions = new("Main Menu",
    new FrameOption[] {
        new("Color Gradients", routeoptions),
        new("Cargo Routes", cargooptions),
        new("Quit", new SimpleFrame(SimpleWork.Quit))
    });

UICanvas canvas = new(topoptions);
canvas.LoadTop();
do
{
    canvas.Step();
} while (canvas.Alive);

void Calculate(UICanvas canvas)
{
    ColorSpace startspace = colors.Decompile(firstcolor.Value);
    ColorSpace endspace = colors.Decompile(secondcolor.Value);

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

    int xcode = default;
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
}

//Application.Run<LotJWindow>();
//Application.Shutdown();
//return;
void CargoWork(UICanvas canvas)
{
    var worlds = Cargo.LoadFromFile();
    ShipOptions ship = new(9500, 40, 40);
    SearchOptions config = new(0.75, false);


    //Console.ReadLine();
    Span<TravelShape> bestroutes = stackalloc TravelShape[8];
    TravelAgent bestagent = default;
    for (int i = 0; i < worlds.Count; ++i)
    {
        PlanetResources world = worlds[i].ToShorthand();
        Console.WriteLine($"=== {worlds[i].Name}\t===");

        for (Resource r = Resource.Common; r <= Resource.Weapons; ++r)
        {
            for (int k = 0; k < worlds.Count; ++k)
            {
                if (i == k)
                    continue;
                TravelShape shape = world.RouteFor(r, worlds[k].ToShorthand(), force_forward: true);
                TravelAgent route = new(shape, config);
                if (route != default)
                {
                    var profit = route.ProfitPerTime;
                    if (profit < config.MinProfit)
                        continue;
                    for (int l = 0; l < bestroutes.Length; ++l)
                    {
                        bestagent = new(bestroutes[l], config);
                        if (bestagent.ProfitPerTime < profit)
                        {
                            if (l + 1 < bestroutes.Length)
                                bestroutes[l..^1].CopyTo(bestroutes[(l + 1)..]);
                            bestroutes[l] = shape;
                            break;
                        }
                    }
                }
            }
        }
        for (int k = 0; k < bestroutes.Length; ++k)
        {
            ref var route = ref bestroutes[k];
            bestagent = new(route, config);
            if (route == default)
                break;
            // bah heap-ifying now.
            string? s = null;
            foreach (var plan in worlds)
                if (plan.Shape == route.Market)
                {
                    s = plan.Name;
                    break;
                }
            Console.WriteLine($"{(bestagent.ProfitPerTime * ship.Cargo):N0} : {route.Resource} -> {s} : {bestagent.Time:0.0}m {bestagent.ProfitPerTime:0.0}u");
            route = default;
        }
        Console.WriteLine();
    }
}

class Engine
{

    void Move()
    {

    }
}