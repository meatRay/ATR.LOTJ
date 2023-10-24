using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ATR.LOTJ;
class UICanvas(Frame tree)
{
    public Frame? At { get; private set; }
    public bool Alive { get; private set; }

    [MemberNotNull(nameof(At))]
    public void LoadTop()
    {
        At = tree;
        Alive = true;
    }

    public void Step()
    {
        switch (At)
        {
            case ComplexFrame complex:
                HandleComplex(complex);
                break;
            case InputFrame<int> inputint:
                HandleInput(inputint);
                break;
            case InputFrame<string> inputstring:
                HandleInput(inputstring);
                break;
            case OptionsFrame option:
                HandleOption(option);
                break;
            case SimpleFrame simple:
                HandleSimple(simple);
                break;
        }
    }

    void HandleInput<TShape>(InputFrame<TShape> input)
        where TShape : IParsable<TShape>
    {
        bool ok = false;
        do
        {
            Console.Clear();
            Console.WriteLine(input.Text);
            Console.Write("\r\n> ");
            if (TShape.TryParse(Console.ReadLine(), default, out var parse))
            {
                input.Data.Value = parse;
                ok = true;
            }
        } while (!ok);
        LoadTop();
    }
    void HandleOption(OptionsFrame options)
    {
        int num = -1;
        do
        {
            Console.Clear();
            Console.WriteLine(options.Text);
            for (int i = 0;
                    i < options.Options.Count;
                    ++i)
                Console.WriteLine($"{i + 1}: {options.Options[i].Text}");
            Console.Write("\r\n> ");
            if (int.TryParse(Console.ReadLine(), out var parse) && parse > 0 && parse <= options.Options.Count)
                num = parse - 1;
        } while (num < 0);
        At = options.Options[num].Goal;
    }

    void HandleComplex(ComplexFrame frame)
    {
        frame.Work(this);
        Console.ReadLine();
        LoadTop();
    }

    void HandleSimple(SimpleFrame frame)
    {
        switch (frame.Work)
        {
            case SimpleWork.Quit: Alive = false; break;
            default: LoadTop(); break;
        }
    }
}
abstract record Frame;
record SayFrame(string Text) : Frame;
class InputData<TShape>(TShape initial)
    where TShape : IParsable<TShape>
{
    public TShape Value { get; set; } = initial;
}
record InputFrame<TShape>(string Text, InputData<TShape> Data) : Frame
     where TShape : IParsable<TShape>;
enum SimpleWork { None, Quit }
record SimpleFrame(SimpleWork Work) : Frame;
delegate void ComplexWork(UICanvas canvas);
record ComplexFrame(ComplexWork Work) : Frame;
readonly record struct FrameOption(string Text, Frame Goal);
record OptionsFrame(string Text, IReadOnlyList<FrameOption> Options)
    : Frame;
record SequenceFrame(SayFrame Parent, IReadOnlyList<SayFrame> Sequence)
    : Frame;