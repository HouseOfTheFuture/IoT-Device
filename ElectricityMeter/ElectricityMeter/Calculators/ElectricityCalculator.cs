using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricityMeter.Calculators
{
    class ElectricityCalculator : IElectricityCalculator, IAnalogReader
    {
        
        public ElectricityCalculator(bool isArm)
        {
            this.isArm = isArm;
        }

        #region Constants
        // define theoretical vref calibration constant for use in readvcc()
        // 1100mV*1024 ADC steps http://openenergymonitor.org/emon/node/1186
        // override in your code with value for your specific AVR chip
        // determined by procedure described under "Calibrating the internal reference voltage" at
        // http://openenergymonitor.org/emon/buildingblocks/calibration
        private const long READVCC_CALIBRATION_CONST = 1126400L;


        private int ADCBits(bool isArm)
        {
            if (isArm)
                return 12;
            return 10;
        }

        private int ADCCounts(bool isArm)
        {
            return (1 << ADCBits(isArm));
        }

        #endregion Constants

        #region Private Fields

        private bool isArm;

        private uint inPinV;
        private uint inPinI;
        //Calibration coefficients
        //These need to be set in order to obtain accurate results
        private double VCAL;
        private double ICAL;
        private double PHASECAL;

        //--------------------------------------------------------------------------------------
        // Variable declaration for emon_calc procedure
        //--------------------------------------------------------------------------------------
        private int sampleV;                             //sample_ holds the raw analog read value
        private int sampleI;

        private double lastFilteredV, filteredV;          //Filtered_ is the raw analog value minus the DC offset
        private double filteredI;
        private double offsetV;                          //Low-pass filter output
        private double offsetI;                          //Low-pass filter output               

        private double phaseShiftedV;                             //Holds the calibrated phase shifted voltage.

        private double sqV, sumV, sqI, sumI, instP, sumP;              //sq = squared, sum = Sum, inst = instantaneous

        private int startV;                                       //Instantaneous voltage at start of sample window.

        private bool lastVCross, checkVCross;                  //Used to measure number of times threshold is crossed.
        #endregion

        public double RealPower { get; private set; }
        public double ApparentPower { get; private set; }
        public double PowerFactor { get; private set; }
        public double Vrms { get; private set; }
        public double Irms { get; private set; }

        public void voltage(uint _inPinV, double _VCAL, double _PHASECAL)
        {
            inPinV = _inPinV;
            VCAL = _VCAL;
            PHASECAL = _PHASECAL;
            offsetV = ADCCounts(isArm) >> 1;
        }

        public void current(uint _inPinI, double _ICAL)
        {
            inPinI = _inPinI;
            ICAL = _ICAL;
            offsetI = ADCCounts(isArm) >> 1;
        }

        public void voltageTX(double _VCAL, double _PHASECAL)
        {
            inPinV = 2;
            VCAL = _VCAL;
            PHASECAL = _PHASECAL;
            offsetV = ADCCounts(isArm) >> 1;
        }

        public void currentTX(uint _channel, double _ICAL)
        {
            if (_channel == 1) inPinI = 3;
            if (_channel == 2) inPinI = 0;
            if (_channel == 3) inPinI = 1;
            ICAL = _ICAL;
            offsetI = ADCCounts(isArm) >> 1;
        }

        public void calcVI(uint crossings, uint timeout)
        {
            long SupplyVoltage = GetSupplyVoltage();

            uint crossCount = 0;                             //Used to measure number of times threshold is crossed.
            uint numberOfSamples = 0;                        //This is now incremented   

            //-------------------------------------------------------------------------------------------------------------------------
            // 1) Waits for the waveform to be close to 'zero' (mid-scale adc) part in sin curve.
            //-------------------------------------------------------------------------------------------------------------------------
            bool st = false;                                  //an indicator to exit the while loop

            ulong start = DateTime.Now.TotalMilliseconds();    //millis()-start makes sure it doesnt get stuck in the loop if there is an error.

            while (st == false)                                   //the while loop...
            {
                startV = AnalogRead(inPinV);                    //using the voltage waveform
                if ((startV < (ADCCounts(isArm) * 0.55)) && (startV > (ADCCounts(isArm) * 0.45))) st = true;  //check its within range
                if ((DateTime.Now.TotalMilliseconds() - start) > timeout) st = true;
            }

            //-------------------------------------------------------------------------------------------------------------------------
            // 2) Main measurement loop
            //------------------------------------------------------------------------------------------------------------------------- 
            start = DateTime.Now.TotalMilliseconds();

            while ((crossCount < crossings) && ((DateTime.Now.TotalMilliseconds() - start) < timeout))
            {
                numberOfSamples++;                       //Count number of times looped.
                lastFilteredV = filteredV;               //Used for delay/phase compensation

                //-----------------------------------------------------------------------------
                // A) Read in raw voltage and current samples
                //-----------------------------------------------------------------------------
                sampleV = AnalogRead(inPinV);                 //Read in raw voltage signal
                sampleI = AnalogRead(inPinI);                 //Read in raw current signal

                //-----------------------------------------------------------------------------
                // B) Apply digital low pass filters to extract the 2.5 V or 1.65 V dc offset,
                //     then subtract this - signal is now centred on 0 counts.
                //-----------------------------------------------------------------------------
                offsetV = offsetV + ((sampleV - offsetV) / 1024);
                filteredV = sampleV - offsetV;
                offsetI = offsetI + ((sampleI - offsetI) / 1024);
                filteredI = sampleI - offsetI;

                //-----------------------------------------------------------------------------
                // C) Root-mean-square method voltage
                //-----------------------------------------------------------------------------  
                sqV = filteredV * filteredV;                 //1) square voltage values
                sumV += sqV;                                //2) sum

                //-----------------------------------------------------------------------------
                // D) Root-mean-square method current
                //-----------------------------------------------------------------------------   
                sqI = filteredI * filteredI;                //1) square current values
                sumI += sqI;                                //2) sum 

                //-----------------------------------------------------------------------------
                // E) Phase calibration
                //-----------------------------------------------------------------------------
                phaseShiftedV = lastFilteredV + PHASECAL * (filteredV - lastFilteredV);

                //-----------------------------------------------------------------------------
                // F) Instantaneous power calc
                //-----------------------------------------------------------------------------   
                instP = phaseShiftedV * filteredI;          //Instantaneous Power
                sumP += instP;                               //Sum  

                //-----------------------------------------------------------------------------
                // G) Find the number of times the voltage has crossed the initial voltage
                //    - every 2 crosses we will have sampled 1 wavelength 
                //    - so this method allows us to sample an integer number of half wavelengths which increases accuracy
                //-----------------------------------------------------------------------------       
                lastVCross = checkVCross;
                if (sampleV > startV) checkVCross = true;
                else checkVCross = false;
                if (numberOfSamples == 1) lastVCross = checkVCross;

                if (lastVCross != checkVCross) crossCount++;
            }

            //-------------------------------------------------------------------------------------------------------------------------
            // 3) Post loop calculations
            //------------------------------------------------------------------------------------------------------------------------- 
            //Calculation of the root of the mean of the voltage and current squared (rms)
            //Calibration coefficients applied. 

            double V_RATIO = VCAL * ((SupplyVoltage / 1000.0) / (ADCCounts(isArm)));
            Vrms = V_RATIO * Math.Sqrt(sumV / numberOfSamples);

            double I_RATIO = ICAL * ((SupplyVoltage / 1000.0) / (ADCCounts(isArm)));
            Irms = I_RATIO * Math.Sqrt(sumI / numberOfSamples);

            //Calculation power values
            RealPower = V_RATIO * I_RATIO * sumP / numberOfSamples;
            ApparentPower = Vrms * Irms;
            PowerFactor = RealPower / ApparentPower;

            //Reset accumulators
            sumV = 0;
            sumI = 0;
            sumP = 0;
            //--------------------------------------------------------------------------------------       
        }

        public double calcIrms(uint numberOfSamples)
        {

            long supplyVoltage = GetSupplyVoltage();

            for (uint n = 0; n < numberOfSamples; n++)
            {
                sampleI = AnalogRead(inPinI);

                // Digital low pass filter extracts the 2.5 V or 1.65 V dc offset, 
                //  then subtract this - signal is now centered on 0 counts.
                offsetI = (offsetI + (sampleI - offsetI) / 1024);
                filteredI = sampleI - offsetI;

                // Root-mean-square method current
                // 1) square current values
                sqI = filteredI * filteredI;
                // 2) sum 
                sumI += sqI;
            }

            double I_RATIO = ICAL * ((supplyVoltage / 1000.0) / (ADCCounts(isArm)));
            Irms = I_RATIO * Math.Sqrt(sumI / numberOfSamples);

            //Reset accumulators
            sumI = 0;
            //--------------------------------------------------------------------------------------       

            return Irms;
        }

        public void serialprint()
        {
            //not implemented yet
        }

        public long readVcc()
        {
            /* long result;
  
              //not used on emonTx V3 - as Vcc is always 3.3V - eliminates bandgap error and need for calibration http://harizanov.com/2013/09/thoughts-on-avr-adc-accuracy/

              #if defined(__AVR_ATmega168__) || defined(__AVR_ATmega328__) || defined (__AVR_ATmega328P__)
              ADMUX = _BV(REFS0) | _BV(MUX3) | _BV(MUX2) | _BV(MUX1);  
              #elif defined(__AVR_ATmega32U4__) || defined(__AVR_ATmega1280__) || defined(__AVR_ATmega2560__) || defined(__AVR_AT90USB1286__)
              ADMUX = _BV(REFS0) | _BV(MUX4) | _BV(MUX3) | _BV(MUX2) | _BV(MUX1);
              ADCSRB &= ~_BV(MUX5);   // Without this the function always returns -1 on the ATmega2560 http://openenergymonitor.org/emon/node/2253#comment-11432
              #elif defined (__AVR_ATtiny24__) || defined(__AVR_ATtiny44__) || defined(__AVR_ATtiny84__)
              ADMUX = _BV(MUX5) | _BV(MUX0);
              #elif defined (__AVR_ATtiny25__) || defined(__AVR_ATtiny45__) || defined(__AVR_ATtiny85__)
              ADMUX = _BV(MUX3) | _BV(MUX2);
	
              #endif


              #if defined(__AVR__) 
              delay(2);                                        // Wait for Vref to settle
              ADCSRA |= _BV(ADSC);                             // Convert
              while (bit_is_set(ADCSRA,ADSC));
              result = ADCL;
              result |= ADCH<<8;
              result = READVCC_CALIBRATION_CONST / result;  //1100mV*1024 ADC steps http://openenergymonitor.org/emon/node/1186
              return result;
             #elif defined(__arm__)
              return (3300);                                  //Arduino Due
             #else 
              return (3300);                                  //Guess that other un-supported architectures will be running a 3.3V!
             #endif
 */

            //search connected device and search for Vcc, for now just return 3.3V because no idea how to search
            return 3300;
        }

        public int AnalogRead(uint pin)
        {
            //uitlezen van een pinnetje, volgt nog
            throw new NotImplementedException();
        }

        public long GetSupplyVoltage()
        {
            //standard voltage is 3300, maar beter gewoon lezen?
            //int SupplyVoltage=3300;
            return readVcc();
        }
    }
}
