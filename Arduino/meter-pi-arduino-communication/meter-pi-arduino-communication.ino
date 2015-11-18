
unsigned long lastSend = millis();
int pushButton = 2;
unsigned long revolutions = 0;
bool started = 0;

void setup() {
  Serial.begin(9600);
}

void loop() {
  if(started) {
    int revolved = digitalRead(pushButton);
    if(revolved == 1) {
      revolutions++;
      String ts = padding(millis(), 10);
      String revStr = padding(revolutions, 10);
      Serial.print("*KCHH*" + ts + ":" + revStr + "OVER");
      delay(500);
    } 
    
    unsigned long boundary = lastSend + 1000;
    if (revolutions == 0 && boundary < millis()) {
      lastSend = millis();
      String revStr = padding(revolutions, 10);
      Serial.print("*KCHH*" + padding(lastSend, 10) + ":" + revStr + "OVER");
    }
  } else if (digitalRead(3)) {
    started = 1;
  }
}

String padding(unsigned long number, byte width ) {
  unsigned long currentMax = 10;
  String paddedNumber = "";
  for (byte i=1; i<width; i++){
    if (number < currentMax) {
      paddedNumber += "0";
    }
    currentMax *= 10;
  } 
  paddedNumber += number;
  return paddedNumber;
}
