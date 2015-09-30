using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricityMeter.Calculators
{
    interface IElectricityCalculator
    {
        void voltage(uint _inPinV, double _VCAL, double _PHASECAL);
        void current(uint _inPinI, double _ICAL);

        void voltageTX(double _VCAL, double _PHASECAL);
        void currentTX(uint _channel, double _ICAL);

        int GetSupplyVoltage();
        void calcVI(uint crossings, uint timeout);
        double calcIrms(uint numberOfSamples);
        void serialprint();
        long readVcc();
    }
}
