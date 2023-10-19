using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ATR.LOTJ;

readonly record struct ColorSpace(int Red, int Green, int Blue)
{
    public static int Magnitude(in ColorSpace space)
        => int.Abs(space.Red) + int.Abs(space.Green) + int.Abs(space.Blue);
    public static ColorSpace Abs(in ColorSpace space)
        => new(int.Abs(space.Red), int.Abs(space.Green), int.Abs(space.Blue));
    public static ColorSpace DitherLerp(in ColorSpace left, in ColorSpace right, int at, int max)
    {
        var dif = Abs(right - left);
        var length = Magnitude(in dif);
        var rspan = dif.Red is 0 ? 0
            : at / (max / dif.Red);
        var gspan = dif.Green is 0 ? 0
            : at / (max / dif.Green);
        var bspan = dif.Blue is 0 ? 0
            : at / (max / dif.Blue);
        return new(left.Red + rspan, left.Green + gspan, left.Blue + bspan);
    }
    public static ColorSpace operator -(in ColorSpace left, in ColorSpace right)
        => new(left.Red - right.Red, left.Green - right.Green, left.Blue - right.Blue);
    public static ColorSpace operator +(in ColorSpace left, in ColorSpace right)
        => new(left.Red + right.Red, left.Green + right.Green, left.Blue + right.Blue);
}

enum ColorDitherStyle { RedGreenBlue, BiggerSmaller, Even }
// eh way more maintainable to mutate some state
record struct ColorDither(ColorSpace Left, ColorSpace Right, int Length)
{
    public readonly ColorSpace Range => Right - Left;
    public int At { get; private set; } = 0;
    int _defecit;
    public ColorSpace TakeandMove()
    {
        if (At is 0)
        {
            ++At;
            return Left;
        }
        if (At + 1 == Length)
            return Right;
        var dif = Range;
        _defecit = ColorSpace.Magnitude(in dif) - Length;
        var rspan = dif.Red is 0 ? 0
            : At / (Length / dif.Red);
        var gspan = dif.Green is 0 ? 0
            : At / (Length / dif.Green);
        var bspan = dif.Blue is 0 ? 0
            : At / (Length / dif.Blue);
        ++At;
        return Left + new ColorSpace(rspan, gspan, bspan);
    }

    public ColorSpace SmallandMove()
    {
        // a param for the ORDER of the colors ( 

        if (At is 0)
        {
            ++At;
            return Left;
        }
        if (At + 1 == Length)
            return Right;
        var dif = Range;
        var difr = int.Abs(dif.Green);
        var difg = int.Abs(dif.Red) + difr;
        var difb = int.Abs(dif.Blue) + difg + difr;
        if (At <= difr)
            return Left + new ColorSpace(0, int.Sign(Range.Green) * (At++ - 0), 0);
        else if (At <= difg)
            return Left + new ColorSpace(int.Sign(Range.Red) * (At++ - difr), Range.Green, 0);
        else
            return Left + new ColorSpace(Range.Red, Range.Green, int.Sign(Range.Blue) * (At++ - difg));
        /*var difr = int.Abs(dif.Green);
        var difg = int.Abs(dif.Blue) + difr;
        var difb = int.Abs(dif.Red) + difg + difr;
        if (At <= difr)
            return Left + new ColorSpace(0, int.Sign(Range.Green) * (At++ - 0), 0);
        else if (At <= difg)
            return Left + new ColorSpace(0, Range.Green, int.Sign(Range.Blue) * (At++ - difr));
        else
            return Left + new ColorSpace(int.Sign(Range.Red) * (At++ - difg), Range.Green, Range.Blue);*/
        /*var difr = int.Abs(dif.Blue);
        var difg = int.Abs(dif.Green) + difr;
        var difb = int.Abs(dif.Red) + difg + difr;
        if (At <= difr)
            return Left + new ColorSpace(0, 0, int.Sign(Range.Blue) * (At++ - 0));
        else if (At <= difg)
            return Left + new ColorSpace(0,int.Sign(Range.Green) * (At++ - difr), Range.Blue);
        else
            return Left + new ColorSpace(int.Sign(Range.Red) * (At++ - difg), Range.Green, Range.Blue);*/
        /*
        var difr = int.Abs(dif.Red);
        var difg = int.Abs(dif.Green) + difr;
        var difb = int.Abs(dif.Blue) + difg + difr;
        if (At <= difr)
            return Left + new ColorSpace(int.Sign(Range.Red) * (At++ - 0), 0, 0);
        else if (At <= difg)
            return Left + new ColorSpace(Range.Red, int.Sign(Range.Green) * (At++ - difr), 0);
        else
            return Left + new ColorSpace(Range.Red, Range.Green, int.Sign(Range.Blue) * (At++ - difg));*/
    }
}
readonly record struct ColorSpaceStep(
    int Size);
class ColorGraph
{
    public ColorSpaceStep RedSpace => _redspace;
    public ColorSpaceStep GreenSpace => _greenspace;
    public ColorSpaceStep BlueSpace => _bluespace;
    readonly ColorSpaceStep _redspace = new(36),
                            _greenspace = new(6),
                            _bluespace = new(1);
    public int HexSpan { get; } = 0x28;
    public int HexMin { get; } = 0x5f;

    public int SpaceMin { get; } = 16;
    public int SpaceMax { get; } = 232;
    public int SpaceRange => SpaceMax - SpaceMin;

    public int StepRange { get; } = 6;

    int Xtract(int xcode, in ColorSpaceStep space)
        => (xcode - SpaceMin) / space.Size;
    public ColorSpace Decompile(int xcode)
    {
        int r = Xtract(xcode, in _redspace);
        xcode -= r * _redspace.Size;
        int g = Xtract(xcode, in _greenspace);
        xcode -= g * _greenspace.Size;
        int b = Xtract(xcode, in _bluespace);
        return new ColorSpace(r, g, b);
    }
    public int Compile(in ColorSpace space)
        => (space.Red * _redspace.Size + space.Green * _greenspace.Size + space.Blue * _bluespace.Size)
            + SpaceMin;

    public string ToHex(in ColorSpace xcolor)
    {
        var r = xcolor.Red is 0 ? 0 : HexMin + (xcolor.Red - 1) * HexSpan;
        var g = xcolor.Green is 0 ? 0 : HexMin + (xcolor.Green - 1) * HexSpan;
        var b = xcolor.Blue is 0 ? 0 : HexMin + (xcolor.Blue - 1) * HexSpan;
        return $"{r:X}" +
                $"{g:X}" +
                $"{b:X}";
    }
}
