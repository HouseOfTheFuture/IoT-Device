namespace ElectricityMeter.Calculators
{
    internal interface IAnalogReader
    {
        int AnalogRead(uint pin);
    }
}