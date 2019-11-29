// defines pins numbers
const int stepPin = 8; 
const int dirPin = 7; 

#include <SimpleHX711.h>

#include "Arduino.h"


#define DOUT A1
#define SCLK A0

double curSpeed = 0;
int accelTime = 50;

double calibrationZero = 0;

SimpleHX711 scale(A0, A1); 
static unsigned long lastRead;

long timerUnits = 0;
double forceGrams;
double lastForceGrams;
uint32_t forceTimestamp = 0;

bool slowMotion = false;

long stepsFromZero = 0;

bool r = false;

int dir = 1;

long startMeasureTime = 0;
long startMeasureStep = 0;

double scaleFactor;

uint32_t LastScaleUpdate;

double distanceK;

double maxForce = 0;

void setup() {
  Serial.begin(250000);
  // Sets the two pins as Outputs
  pinMode(stepPin,OUTPUT); 
  pinMode(dirPin,OUTPUT);
  float points = 0;
  while (!scale.read()) {
    yield();
  };
  scaleFactor = (285000000-340000)/500.0;
  calibrationZero = scale.getAdjusted()/scaleFactor;
  
  Serial.print("cal: ");
  Serial.println(calibrationZero);
  
  forceGrams = 0;
  lastForceGrams = 0;

  LastScaleUpdate = millis();

  distanceK = 38.5/100000;

}
void loop() {
  if (LastScaleUpdate + 100 < millis()){
    LastScaleUpdate = millis();
    updateForce();
    if (slowMotion){
      Serial.print("time=");
      Serial.print(forceTimestamp-startMeasureTime);
      Serial.print(";force=");
      Serial.println(long(forceGrams*100));
      if (lastForceGrams > forceGrams && lastForceGrams > maxForce){
          maxForce = lastForceGrams;
          Serial.print("distance=");
          Serial.print(long((stepsFromZero-startMeasureStep)*distanceK*100));
          Serial.print(";maxForce=");
          Serial.println(long(maxForce*100));
       }
    }
  }
  scale.read();
  if (Serial.available()>0){
    stp();
    r = true;
    String s =Serial.readString();
    Serial.print(s);
    if (s.equals("-\n")){
      upDir();
      Serial.println("up");
    } else if (s.equals("+\n")){
      downDir();
      Serial.println("down");
    } else if (s.equals("--\n")){
      upDir();
      stepsFromZero = 999999999;
      Serial.println("up");
    } else if (s.equals("home\n")){
      stepsFromZero=0;
      r = false;
    } else {
      Serial.println("r = false");
      r = false;
    }

  }
  if (stepsFromZero < 0){
    r = false;
    stp();
    stepsFromZero = 0;
    maxForce = 0;
    Serial.println("Ready");
  }
  if (r){
    move2();
  }
}


void upDir(){
  digitalWrite(dirPin, LOW);
  dir = -1;
}
void downDir(){
  digitalWrite(dirPin, HIGH);
  dir = 1;
}

void move2(){
   if (forceGrams < 10 || dir==-1){
      moveFast();
      if (slowMotion){
        slowMotion = false;
        upDir();
      }
   } else if (forceGrams < 800){
      moveSlow();
      if (!slowMotion){
        startMeasureStep = stepsFromZero;
        startMeasureTime = millis();
        slowMotion = true;
      }
   } else {
    Serial.println(forceGrams);
    stp();
   }
}

void stp(){
  Serial.println("stop");
  curSpeed = 0;
  
}
void moveFast(){
    digitalWrite(stepPin,HIGH); 
    delayMicroseconds(int(accelTime + 15 - curSpeed)); 
    digitalWrite(stepPin,LOW); 
    delayMicroseconds(int(accelTime + 15 - curSpeed));
    if (curSpeed < accelTime){
      curSpeed+=0.005;
    }
    stepsFromZero += dir;
}

void moveSlow(){
    digitalWrite(stepPin,HIGH); 
    delayMicroseconds(int(accelTime + 60 - curSpeed)); 
    digitalWrite(stepPin,LOW); 
    delayMicroseconds(int(accelTime + 60 - curSpeed));
    if (curSpeed < accelTime){
      curSpeed+=0.005;
    }
    stepsFromZero += dir;
}


void updateForce(){
  lastForceGrams = forceGrams;
  if (scale.getStatus() == SimpleHX711::valid){
    forceGrams = scale.getRaw()/scaleFactor-calibrationZero;
    forceTimestamp = scale.getTimestamp();
  }
}
