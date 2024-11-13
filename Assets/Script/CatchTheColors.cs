using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatchTheColors : MonoBehaviour
{
    public static CatchTheColors instance;
    public List<Color> predefinedColors; // Define your list of predefined colors
    public int numberOfColorsToFind = 3; // Number of most used colors to find

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public List<MyColors> FindMostUsedColors(Texture2D texture)
    {
        List<MyColors> MostCommonColors = new List<MyColors>();
        Color[] pixels = texture.GetPixels();
        Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

        foreach (Color pixel in pixels)
        {
            Color closestColor = FindClosestPredefinedColor(predefinedColors, pixel);

            if (colorCount.ContainsKey(closestColor))
            {
                colorCount[closestColor]++;
            }
            else
            {
                colorCount[closestColor] = 1;
            }
        }

        // Get the 'numberOfColors' most used colors
        var mostUsedColors = colorCount.OrderByDescending(kvp => kvp.Value)
            .Take(numberOfColorsToFind)
            .Select(kvp => kvp.Key);

        foreach (var color in mostUsedColors)
        {
            //if (color == Color.black) //Black
                //MostCommonColors.Add(MyColors.Black);
            //else if (color == Color.white) //White
                //MostCommonColors.Add(MyColors.White);
            if (color == Color.red) //Red
                MostCommonColors.Add(MyColors.Red);
            else if (color == Color.green) //Green
                MostCommonColors.Add(MyColors.Green);
            else if (color == Color.blue) //Blue
                MostCommonColors.Add(MyColors.Blue);
            else if (color == Color.cyan) //Cyan
                MostCommonColors.Add(MyColors.Cyan);
            else if (color == new Color(1,1,0,1)) //Yellow
                MostCommonColors.Add(MyColors.Yellow);
            else if (color == Color.magenta) //Purple
                MostCommonColors.Add(MyColors.Purple);
        }

        return MostCommonColors;
    }
    public List<Color> MyColorsControl(List<MyColors> _MyColors)
    {
        List<Color> colors = new List<Color>();
        foreach (var MyColor in _MyColors)
        {
            switch (MyColor)
            {
                case MyColors.Black:
                    colors.Add(Color.black);
                    break;
                case MyColors.White:
                    colors.Add(Color.white);
                    break;
                case MyColors.Red:
                    colors.Add(Color.red);
                    break;
                case MyColors.Green:
                    colors.Add(Color.green);
                    break;
                case MyColors.Blue:
                    colors.Add(Color.blue);
                    break;
                case MyColors.Cyan:
                    colors.Add(Color.cyan);
                    break;
                case MyColors.Yellow:
                    colors.Add(Color.yellow);
                    break;
                case MyColors.Purple:
                    colors.Add(Color.magenta);
                    break;               
                default:
                    break;
            }
        }
        return colors;
    }
    Color FindClosestPredefinedColor(List<Color> colors, Color target)
    {
        Color closestColor = Color.clear;
        float closestDistance = Mathf.Infinity;

        foreach (Color color in colors)
        {
            float distance = Vector4.Distance(target, color);

            if (distance < closestDistance)
            {
                closestColor = color;
                closestDistance = distance;
            }
        }

        return closestColor;
    }

    public Sprite TextureToSprite(Texture2D texture)
    {
        Rect rect;
        if (texture == null)
        {
            rect = new Rect(0, 0, 256, 256);
        }
        else
        {
            rect = new Rect(0, 0, texture.width, texture.height);
        }
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        // Sprite'ı oluştur
        Sprite sprite = Sprite.Create(texture, rect, pivot);

        return sprite;
    }


}

public enum MyColors
{
    Black,
    White,
    Red,
    Green,
    Blue,
    Cyan,
    Yellow,
    Purple,
    Length,
}