internal record Temperature(decimal Celcius)
{
    public decimal Fahrenheit => Celcius * 9 / 5 + 32;
    public decimal Kelvin => Celcius + 273.15m;
}