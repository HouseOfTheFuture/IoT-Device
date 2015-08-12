const int sensorPin = 2;     
const int ledPin =  3;      

int sensorState = 0;         

void setup() {
  pinMode(ledPin, OUTPUT);
  pinMode(sensorPin, INPUT);
}

void loop() {
  sensorState = digitalRead(sensorPin);
  if (sensorState == HIGH) {
    digitalWrite(ledPin, HIGH);
  }
  else {
    digitalWrite(ledPin, LOW);
  }
}
