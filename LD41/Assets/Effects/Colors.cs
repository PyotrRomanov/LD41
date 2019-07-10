using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Colors {

    public static Color playerColor = Colors.IntHSV(255, 0, 255);
    public static Color cursorColor = Colors.IntHSV(261, 100, 80);
    public static Color cursorSelectableColor = Colors.IntHSV(261, 40, 80);
    public static Color moveSelectColor = new Color(0.9f, 0.9f, 0.9f);

    public static Color DOOM = new Color(0.2f, 0.2f, 0.2f);

    public static Color passableTile = Colors.IntHSV(261, 50, 38);
    public static Color unpassableTile = IntColor(50, 50, 50);
    public static Color homeTile = Colors.IntHSV(261, 60, 38);
    public static Color pawnColor = Colors.IntHSV(261, 80, 80);
    public static Color bishopColor = Colors.IntHSV(261, 50, 80);
    public static Color knightColor = Colors.IntHSV(261, 30, 80);
    public static Color goalTile = Colors.IntColor(0, 0, 0);

    public static Color movementActionColor = Colors.IntHSV(261, 100, 80);
    public static Color directionalShotColor = Colors.IntHSV(261, 100, 80);

    public static Color collapseFadeColor = IntColor(50, 50, 50);

    public static Color buttonNormal = Colors.IntColor(200, 242, 242);
    public static Color buttonHighlight = Colors.IntColor(87, 198, 198);

    public static Color agentDeath = Colors.passableTile;

    public static Color IntColor(int r, int g, int b, int a = 255) {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color IntHSV(int h, int s, int v) {
        return Color.HSVToRGB(h / 360f, s / 100f, v / 100f, true);
    }
}

public class WorldColor {
    private float fHue;
    private int intHue;

    public Color playerColor;
    public Color cursorColor;
    public Color cursorSelectableColor;
    public Color moveSelectColor;

    public Color DOOM;

    public Color passableTile;
    public Color unpassableTile;
    public Color homeTile;
    public Color pawnColor;
    public Color bishopColor;
    public Color knightColor;
    public Color chaserColor;
    public Color goalTile;

    public Color movementActionColor;
    public Color directionalShotColor;

    public Color collapseFadeColor;

    public Color buttonNormal;
    public Color buttonHighlight;

    public Color agentDeath;

    public WorldColor(int hue) {
        
        this.fHue = hue / 360f;
        this.intHue = hue;

        playerColor = Colors.IntHSV(255, 0, 255);
        cursorColor = Colors.IntHSV(hue, 100, 80);
        cursorSelectableColor = Colors.IntHSV(hue, 40, 80);
        moveSelectColor = new Color(0.9f, 0.9f, 0.9f);

        DOOM = new Color(0.2f, 0.2f, 0.2f);

        passableTile = Colors.IntHSV(hue, 50, 38);
        unpassableTile = IntColor(50, 50, 50);
        homeTile = Colors.IntHSV(hue, 60, 38);
        pawnColor = Colors.IntHSV(hue, 80, 80);
        bishopColor = Colors.IntHSV(hue, 50, 80);
        knightColor = Colors.IntHSV(hue, 30, 80);
        chaserColor = Colors.IntHSV(hue, 20, 60);
        goalTile = Colors.IntHSV(hue, 0, 0);


        movementActionColor = Colors.IntHSV(hue, 100, 80);
        directionalShotColor = Colors.IntHSV(hue, 100, 80);

        collapseFadeColor = IntColor(50, 50, 50);

        buttonNormal = Colors.IntColor(200, 242, 242);
        buttonHighlight = Colors.IntColor(87, 198, 198);

        agentDeath = Colors.passableTile;
    }

    public static Color IntColor(int r, int g, int b, int a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color IntHSV(int h, int s, int v)
    {
        return Color.HSVToRGB(h / 360f, s / 100f, v / 100f, false);
    }

    // Returns the result of chanhing the hue of the given color to that of 
    // the WorldColor object.
    public Color ShiftHue(Color c) {
        float H, S, V;
        Color.RGBToHSV(c, out H, out S, out V);
        return Color.HSVToRGB(this.fHue, S, V, false);
    }

    public static int ExtractHue(Color c) {
        float H, S, V;
        Color.RGBToHSV(c, out H, out S, out V);
        return (int) (H / 360f);
    }
}
