using ATR.LOTJ;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;

ColorGraph colors = new();

InputData<int> firstcolor = new( 0 ), secondcolor = new( 0 );
InputData<int> sequencestack = new( 0 ), colorsequencestack = new( 0 );
InputData<string> planetedit = new( string.Empty );
InputData<bool> traffickingdata = new( false );
InputData<double> filterdata = new( 0.0 );
InputData<int> cargodata = new( 0 ), hyperspeeddata = new( 0 ), speeddata = new( 0 );
SequenceFrame routeoptions = new( colorsequencestack,
    new Frame[] {
        new InputFrame<int>("First Color: ", firstcolor),
        new InputFrame<int>("Second Color: ", secondcolor),
        new ComplexFrame(Calculate),
    } );
InputFrame<string> cargoeditoptions = new( "Planet to edit..", planetedit );
ComplexFrame filldataforplanet = new( EditPlanet );
SequenceFrame editcargo = new( sequencestack, new Frame[] { cargoeditoptions, filldataforplanet } );
OptionsFrame cargooptions = new( "Cargo Tool",
    new FrameOption[] {
        new("Calculate", new ComplexFrame(CargoWork)),
        new("Edit Resources", editcargo),
        new("Edit Ship", new SequenceFrame(sequencestack, new Frame[]{
                new InputFrame<int>("Ship cargo units..", cargodata),
                new InputFrame<int>("Ship max speed..", speeddata),
                new InputFrame<int>("Ship max hyperspeed..", hyperspeeddata),
                new ComplexFrame(SaveShip),
            } )),
        new("Edit Options", new SequenceFrame(sequencestack, new Frame[]{
                new InputFrame<bool>("Will you be trafficking instead of paying taxes?", traffickingdata),
                new InputFrame<double>("Filter out options that are less than this many credits-per-minut..", filterdata),
                new ComplexFrame(SaveSearch),
            } )),
        new("Main Menu", new SimpleFrame(SimpleWork.Back))
    } );
OptionsFrame topoptions = new( "Main Menu",
    new FrameOption[] {
        new("Color Gradients", routeoptions),
        new("Cargo Routes", cargooptions),
        new("Quit", new SimpleFrame(SimpleWork.Quit))
    } );

UICanvas canvas = new( topoptions );
canvas.LoadTop();
do
{
    canvas.Step();
} while ( canvas.Alive );

void SaveShip( UICanvas canvas )
{
    ShipOptions ship = new( cargodata.Value, hyperspeeddata.Value, speeddata.Value );
    Cargo.SaveShipToFile( ship );
}
void SaveSearch( UICanvas canvas )
{
    SearchOptions ship = new( filterdata.Value, traffickingdata.Value );
    Cargo.SaveSearchToFile( ship );
}
void EditPlanet( UICanvas canvas )
{
    var worlds = Cargo.LoadPlanetsFromFile();
    var bestmatch = worlds.Where( w => w.Name.StartsWith( planetedit.Value, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
    if ( bestmatch is null )
        return;
    Console.WriteLine( $"=== {bestmatch.Name}\t===" );
    var resources = bestmatch.ToShorthand();
    for ( Resource r = Resource.Common; r <= Resource.Weapons; ++r )
    {
        if ( resources.Has( r ) )
            Console.WriteLine( $"\t{r}:\t\t{resources[r]:0.00}" );
    }
    Console.WriteLine( "\r\nInput new values or 'quit' to abort." );
    ResourceChart nextresources = default;
    for ( Resource r = Resource.Common; r <= Resource.Weapons; ++r )
    {
        if ( resources.Has( r ) )
        {
            double num = double.NaN;
            do
            {
                Console.Write( $"\t{r}\t\t> " );
                var got = Console.ReadLine();
                if ( "quit".StartsWith( got, StringComparison.OrdinalIgnoreCase ) )
                    return;
                double.TryParse( got, out num );
            } while ( num is double.NaN );
            nextresources[r] = num;
        }
    }
    bestmatch.Resources = (new PlanetResources( nextresources ) { Where = bestmatch.Shape }).ToArray();
    Cargo.SavePlanetsToFile( worlds );

    Console.WriteLine( "Changes saved." );
}

void Calculate( UICanvas canvas )
{
    ColorSpace startspace = colors.Decompile( firstcolor.Value );
    ColorSpace endspace = colors.Decompile( secondcolor.Value );

    var tokencount = ColorSpace.Magnitude( endspace - startspace ) + 1;
    Span<ColorSpace> spaces = stackalloc ColorSpace[tokencount];

    static void RunOn( ColorDither dither, ColorGraph graph )
    {
        Console.Write( '\t' );
        for ( int i = 0; i < dither.Length; ++i )
        {
            Console.Write( $"\x1b[38;5;{graph.Compile( dither.SmallandMove() )}m" );
            Console.Write( (char)('a' + i) );
        }
        Console.WriteLine( "\x1b[0m" );
    }
    static void CodesOn( ColorDither dither, ColorGraph graph )
    {
        Console.Write( '\t' );
        for ( int i = 0; i < dither.Length; ++i )
        {
            Console.Write( $"&{graph.Compile( dither.SmallandMove() ):000}" );
            Console.Write( (char)('a' + i) );
        }
        Console.WriteLine();
    }

    Console.WriteLine( "Path of MAX magnitude" );
    Console.WriteLine( "RED -> GREEN -> BLUE" );
    RunOn( new( startspace, endspace, tokencount, ColorOrient.RedGreen ), colors );
    CodesOn( new( startspace, endspace, tokencount, ColorOrient.RedGreen ), colors );

    Console.WriteLine( "GREEN -> BLUE -> RED" );
    RunOn( new( startspace, endspace, tokencount, ColorOrient.GreenBlue ), colors );
    CodesOn( new( startspace, endspace, tokencount, ColorOrient.GreenBlue ), colors );

    Console.WriteLine( "BLUE -> RED -> GREEN" );
    RunOn( new( startspace, endspace, tokencount, ColorOrient.BlueRed ), colors );
    CodesOn( new( startspace, endspace, tokencount, ColorOrient.BlueRed ), colors );

    Console.WriteLine( "GREEN -> RED -> BLUE" );
    RunOn( new( startspace, endspace, tokencount, ColorOrient.GreenRed ), colors );
    CodesOn( new( startspace, endspace, tokencount, ColorOrient.GreenRed ), colors );

    Console.WriteLine( "RED -> BLUE -> GREEN" );
    RunOn( new( startspace, endspace, tokencount, ColorOrient.RedBlue ), colors );
    CodesOn( new( startspace, endspace, tokencount, ColorOrient.RedBlue ), colors );

    Console.WriteLine( "BLUE -> GREEN -> RED" );
    RunOn( new( startspace, endspace, tokencount, ColorOrient.BlueGreen ), colors );
    CodesOn( new( startspace, endspace, tokencount, ColorOrient.BlueGreen ), colors );
    Console.WriteLine();

    int xcode = default;
    var travel = endspace - startspace;
    ColorDither dither = new( startspace, endspace, 1 + int.Max( int.Abs( travel.Red ), int.Max( int.Abs( travel.Green ), int.Abs( travel.Blue ) ) ) );
    Console.WriteLine( "Path of MIN magnitude" );
    Console.Write( '\t' );
    for ( int i = 0; i < dither.Length; ++i )
    {
        ColorSpace space = dither.TakeandMove();

        var trycode = colors.Compile( in space );
        Console.Write( $"\x1b[38;5;{trycode}m" );
        Console.Write( (char)('a' + i) );
        xcode = trycode;
    }
    Console.WriteLine( "\x1b[0m" );
    dither = new( startspace, endspace, 1 + int.Max( int.Abs( travel.Red ), int.Max( int.Abs( travel.Green ), int.Abs( travel.Blue ) ) ) );
    Console.Write( '\t' );
    for ( int i = 0; i < dither.Length; ++i )
    {
        ColorSpace space = dither.TakeandMove();

        var trycode = colors.Compile( in space );
        if ( trycode != xcode )
            Console.Write( $"&{trycode:000}" );
        Console.Write( (char)('a' + i) );
        xcode = trycode;
    }
}

//Application.Run<LotJWindow>();
//Application.Shutdown();
//return;
void CargoWork( UICanvas canvas )
{
    var worlds = Cargo.LoadPlanetsFromFile();
    ShipOptions ship = Cargo.LoadShipFromFile();
    SearchOptions config = Cargo.LoadSearchFromFile();


    //Console.ReadLine();
    Span<TravelShape> bestroutes = stackalloc TravelShape[8];
    TravelAgent bestagent = default;
    for ( int i = 0; i < worlds.Count; ++i )
    {
        PlanetResources world = worlds[i].ToShorthand();
        Console.WriteLine( $"=== {worlds[i].Name}\t===" );

        for ( Resource r = Resource.Common; r <= Resource.Weapons; ++r )
        {
            for ( int k = 0; k < worlds.Count; ++k )
            {
                if ( i == k )
                    continue;
                TravelShape shape = world.RouteFor( r, worlds[k].ToShorthand(), force_forward: true );
                TravelAgent route = new( shape, config );
                if ( route != default )
                {
                    var profit = route.ProfitPerTime;
                    if ( profit * ship.Cargo < config.MinProfit )
                        continue;
                    for ( int l = 0; l < bestroutes.Length; ++l )
                    {
                        bestagent = new( bestroutes[l], config );
                        if ( bestagent.ProfitPerTime < profit )
                        {
                            if ( l + 1 < bestroutes.Length )
                                bestroutes[l..^1].CopyTo( bestroutes[(l + 1)..] );
                            bestroutes[l] = shape;
                            break;
                        }
                    }
                }
            }
        }
        for ( int k = 0; k < bestroutes.Length; ++k )
        {
            ref var route = ref bestroutes[k];
            bestagent = new( route, config );
            if ( route == default )
                break;
            // bah heap-ifying now.
            string? s = null;
            foreach ( var plan in worlds )
                if ( plan.Shape == route.Market )
                {
                    s = plan.Name;
                    break;
                }
            Console.WriteLine( $"{(bestagent.ProfitPerTime * ship.Cargo):N0} : {route.Resource} -> {s} : {bestagent.Time:0.0}m {bestagent.ProfitPerTime:0.0}u" );
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