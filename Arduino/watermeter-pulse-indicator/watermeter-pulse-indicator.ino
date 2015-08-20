const int sensorPin = 2;     
const int ledPin =  3;      

int sensorState = 0; 
int previousState = 0;

void setup() {
  pinMode(ledPin, OUTPUT);
  pinMode(sensorPin, INPUT);
  Serial.begin(9600);  
  Serial.println(millis());
  previousState = digitalRead(sensorPin);
}

void loop() {
  sensorState = digitalRead(sensorPin);
 
  if (sensorState != previousState) {
    if(previousState == HIGH)
    {
      digitalWrite(ledPin, HIGH);    
      Serial.println(millis());
      delay(1000);
       Serial.println(millis());
          digitalWrite(ledPin, LOW);  
    }  
    previousState = sensorState;

  }
}
