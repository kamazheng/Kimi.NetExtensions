using System.Drawing;

public static class ColorTool
{
    static ColorTool() { LicenceHelper.CheckLicense(); }

    public static Color[] GetColors(int count, Color initialColor)
    {
        Color[] colors = new Color[count];

        // Convert the initial color to HSL
        float initialHue, initialSaturation, initialLightness;
        ColorToHSL(initialColor, out initialHue, out initialSaturation, out initialLightness);

        // Calculate the step size for the hue component
        float hueStep = 360f / count;

        // Generate the colors
        for (int i = 0; i < count; i++)
        {
            // Calculate the hue for the current color
            float hue = (initialHue + i * hueStep) % 360;

            // Convert the HSL color back to RGB
            colors[i] = HSLToColor(hue, initialSaturation, initialLightness);
        }

        return colors;
    }

    private static void ColorToHSL(Color color, out float hue, out float saturation, out float lightness)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));

        float delta = max - min;

        // Calculate the hue
        if (delta == 0)
        {
            hue = 0;
        }
        else if (max == r)
        {
            hue = 60 * ((g - b) / delta % 6);
        }
        else if (max == g)
        {
            hue = 60 * ((b - r) / delta + 2);
        }
        else
        {
            hue = 60 * ((r - g) / delta + 4);
        }

        // Calculate the lightness
        lightness = (max + min) / 2;

        // Calculate the saturation
        if (delta == 0)
        {
            saturation = 0;
        }
        else
        {
            saturation = delta / (1 - Math.Abs(2 * lightness - 1));
        }
    }

    private static Color HSLToColor(float hue, float saturation, float lightness)
    {
        float c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        float x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
        float m = lightness - c / 2;

        float r, g, b;

        if (hue >= 0 && hue < 60)
        {
            r = c;
            g = x;
            b = 0;
        }
        else if (hue >= 60 && hue < 120)
        {
            r = x;
            g = c;
            b = 0;
        }
        else if (hue >= 120 && hue < 180)
        {
            r = 0;
            g = c;
            b = x;
        }
        else if (hue >= 180 && hue < 240)
        {
            r = 0;
            g = x;
            b = c;
        }
        else if (hue >= 240 && hue < 300)
        {
            r = x;
            g = 0;
            b = c;
        }
        else
        {
            r = c;
            g = 0;
            b = x;
        }

        byte red = (byte)((r + m) * 255);
        byte green = (byte)((g + m) * 255);
        byte blue = (byte)((b + m) * 255);

        return Color.FromArgb(red, green, blue);
    }

    public static string RgbColorToHex(this Color color)
    {
        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
    }

    //includes an alpha (transparency) component
    public static string ArgbColorToHex(this Color color)
    {
        return "#" + color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
    }
}