using System;
using System.Runtime.InteropServices;
using Vintagestory.API.Client;

namespace MoreHudBars.Info;

public class AllowedSpace
{
    public double XStart;
    public double XEnd;

    public double YStart;
    public double YEnd;

    public void TryReserve(ref double x, ref double y, ref double width, ref double height, EXYDominant domimantSide = EXYDominant.X, bool invertDirection = false)
    {
        if(domimantSide == EXYDominant.X)
        {
            ShiftBounds(ref y, ref height, ref YStart, ref YEnd, !invertDirection, false, reserve: true);
            ShiftBounds(ref x, ref width, ref XStart, ref XEnd, false, true);
        }
        else
        {
            ShiftBounds(ref y, ref height, ref YStart, ref YEnd, false, true);
            ShiftBounds(ref x, ref width, ref XStart, ref XEnd, !invertDirection, false, reserve: true);
        }
    }

    private static void ShiftBounds(ref double nStart, ref double nLength, ref double nMin, ref double nMax, bool invertDirection, bool shrink, bool reserve = false)
    {
        double spacing = GuiElement.scaled(1);

        if (!invertDirection)
        {
            if (nStart < nMin)
            {
                if (shrink) nLength -= nMin - nStart;
                nStart = nMin;
            }

            if(reserve) nMin = nStart + nLength + spacing;
        }
        else
        {
            if(nStart + nLength > nMax)
            {
                nStart = nMax - nLength;
            }

            if(reserve) nMax = nStart - spacing;
        }
    }
}
