using HtmlAgilityPack;
using Microsoft.VisualBasic;

internal class SeaTemperatureParser
{
    public static bool TryParse(string html, out Temperature? seaTemperature)
    {
        seaTemperature = null;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var temperatureNode = htmlDoc.DocumentNode
            .SelectSingleNode("//p[span[@class='boldCast' and contains(text(), 'Sea temperature:')]]");

        if (temperatureNode is null) { return false; }

        var spanNode = temperatureNode.SelectSingleNode("span[@class='boldCast']");
        spanNode?.Remove();

        var temperatureText = temperatureNode.InnerText.Trim();

        if (decimal.TryParse(temperatureText.Split('°')[0], out var temperature))
        {
            seaTemperature = new Temperature(temperature);
            return true;
        }

        return false;
    }
}