using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ATR.LOTJ;
class UICanvas( Frame tree )
{
    readonly Stack<Frame> _at = new();
    public Frame? At => _at.Count > 0 ? _at.Peek() : null;
    public bool Alive { get; private set; }

    [MemberNotNull( nameof( At ) )]
    public void LoadTop()
    {
        _at.Clear();
        _at.Push( tree );
        Alive = true;
    }

    public void Step()
    {
        if ( Handle( At ) )
            _at.Pop();
    }
    public bool Handle( Frame? frame )
        => frame switch {
            SequenceFrame sequence
                => HandleSequence( sequence ),
            ComplexFrame complex
                => HandleComplex( complex ),
            InputFrame<bool> input
                => HandleInput( input ),
            InputFrame<double> input
                => HandleInput( input ),
            InputFrame<int> input
                => HandleInput( input ),
            InputFrame<string> input
                => HandleInput( input ),
            OptionsFrame option
                => HandleOption( option ),
            SimpleFrame simple
                => HandleSimple( simple ),
            _ => false,
        };

    bool HandleInput( InputFrame<bool> input )
    {
        bool ok = false;
        do
        {
            Console.Clear();
            Console.WriteLine( input.Text );
            Console.Write( "\r\n> " );
            var ln = Console.ReadLine();
            if( ln.Equals( "yes", StringComparison.OrdinalIgnoreCase ) )
            {
                input.Data.Value = true;
                return true;
            }
            else if ( ln.Equals( "no", StringComparison.OrdinalIgnoreCase ) )
            {
                input.Data.Value = false;
                return true;
            }
            else if ( bool.TryParse(ln , out var parse ) )
            {
                input.Data.Value = parse;
                return true;
            }
        } while ( !ok );
        return false;
        //LoadTop();
    }
    bool HandleInput<TShape>( InputFrame<TShape> input )
        where TShape : IParsable<TShape>
    {
        bool ok = false;
        do
        {
            Console.Clear();
            Console.WriteLine( input.Text );
            Console.Write( "\r\n> " );
            if ( TShape.TryParse( Console.ReadLine(), default, out var parse ) )
            {
                input.Data.Value = parse;
                return true;
            }
        } while ( !ok );
        return false;
        //LoadTop();
    }

    bool HandleSequence( SequenceFrame sequence )
    {
        if ( sequence.Stack.Value < sequence.Sequence.Count )
            _at.Push( sequence.Sequence[sequence.Stack.Value++] );
        else
        {
            sequence.Stack.Value = 0;
            return true;
        }
        return false;
    }

    bool HandleOption( OptionsFrame options )
    {
        int num = -1;
        do
        {
            Console.Clear();
            Console.WriteLine( options.Text );
            for ( int i = 0;
                    i < options.Options.Count;
                    ++i )
                Console.WriteLine( $"{i + 1}: {options.Options[i].Text}" );
            Console.Write( "\r\n> " );
            if ( int.TryParse( Console.ReadLine(), out var parse ) && parse > 0 && parse <= options.Options.Count )
                num = parse - 1;
        } while ( num < 0 );
        _at.Push( options.Options[num].Goal );
        return false;
    }

    bool HandleComplex( ComplexFrame frame )
    {
        Console.Clear();
        frame.Work( this );
        Console.ReadLine();
        return true;
    }

    bool HandleSimple( SimpleFrame frame )
    {
        switch ( frame.Work )
        {
            case SimpleWork.Quit: Alive = false; break;
            case SimpleWork.Back: _at.Pop(); break;
            default:
                if ( _at.Count > 1 )
                    _at.Pop();
                break;//LoadTop(); break;
        }
        return true;
    }
}
abstract record Frame;
record SayFrame( string Text ) : Frame;
class InputData<TShape>( TShape initial )
    where TShape : IParsable<TShape>
{
    public TShape Value { get; set; } = initial;
}
record InputFrame<TShape>( string Text, InputData<TShape> Data ) : Frame
     where TShape : IParsable<TShape>;
enum SimpleWork { None, Quit, Back }
record SimpleFrame( SimpleWork Work ) : Frame;
delegate void ComplexWork( UICanvas canvas );
record ComplexFrame( ComplexWork Work ) : Frame;
readonly record struct FrameOption( string Text, Frame Goal, char Key = default );
record OptionsFrame( string Text, IReadOnlyList<FrameOption> Options )
    : Frame;
record SequenceFrame( InputData<int> Stack, IReadOnlyList<Frame> Sequence )
    : Frame;