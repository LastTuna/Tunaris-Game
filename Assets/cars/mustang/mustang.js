var wheelFL : WheelCollider;
var wheelFR : WheelCollider;
var wheelRL : WheelCollider;
var wheelRR : WheelCollider;
var wheelFLTrans : Transform;
var wheelFRTrans : Transform;
var wheelRLTrans : Transform;
var wheelRRTrans : Transform;
var currentSpeed : float;
var wheelRPM : float;
var speedo : Texture2D;
var speedopoint : Texture2D;

var currentGrip : float = 1.3;
//tuneable stats
var brakeStrength : float = 200;
var aero : float = 15.0;
var ratio : float;


var engineRPM : float;
var engineREDLINE : float = 9000;//engine redline
var unitOutput : float;
//gears
var gearR : float = -5.0;
var gearN : float = 0.0;
var gear1 : float = 5.4;
var gear2 : float = 3.4;
var gear3 : float = 2.7;
var gear4 : float = 2.0;
var gear5 : float = 1.8;
var gear6 : float = 1.6;
var gears = [gearR,gearN,gear1,gear2,gear3,gear4,gear5,gear6];
var gear : float;//current gear
var shifting : boolean = false;//shifter delay

function Start () {

engineRPM = 800;
ratio = 4.3;
Physics.gravity = Vector3(0,-aero,0);
GetComponent.<Rigidbody>().centerOfMass = Vector3(0, -0.4, 0.5);
gear = 1;


}

function FixedUpdate () {
engine();

if(Input.GetAxis("Brake")){//brakes
	wheelFL.brakeTorque = brakeStrength;
	wheelFR.brakeTorque = brakeStrength;
	wheelRL.brakeTorque = brakeStrength;
	wheelRR.brakeTorque = brakeStrength;
} else {
	wheelFL.brakeTorque = 0;
	wheelFR.brakeTorque = 0;
	wheelRL.brakeTorque = 0;
	wheelRR.brakeTorque = 0;
}
if(Input.GetAxis("Handbrake")){//HANDBRAKE
	wheelRL.sidewaysFriction.stiffness = 0.5;
	wheelRR.sidewaysFriction.stiffness = 0.5;
	wheelRL.forwardFriction.stiffness = 0.5;
	wheelRR.forwardFriction.stiffness = 0.5;

	wheelRL.brakeTorque = 300;
	wheelRR.brakeTorque = 300;
} else {
	wheelRL.brakeTorque = 0;
	wheelRR.brakeTorque = 0;
	wheelRL.sidewaysFriction.stiffness = currentGrip;
	wheelRR.sidewaysFriction.stiffness = currentGrip;
	wheelRL.forwardFriction.stiffness = currentGrip;
	wheelRR.forwardFriction.stiffness = currentGrip;
}

wheelFR.steerAngle = 20 * Input.GetAxis("Steering");//steering
wheelFL.steerAngle = 20 * Input.GetAxis("Steering");

wheelRPM = (wheelFL.rpm + wheelRL.rpm) / 2; //speed counter
currentSpeed = 2*22/7*wheelFL.radius*wheelRL.rpm*60/1000;
currentSpeed = Mathf.Round(currentSpeed);
}

function Update () {
gearbox();//gearbox update 

wheelFRTrans.Rotate(wheelFL.rpm/60*360*Time.deltaTime,0,0); //graphical updates
wheelFLTrans.Rotate(wheelFL.rpm/60*360*Time.deltaTime,0,0);
wheelRRTrans.Rotate(wheelFL.rpm/60*360*Time.deltaTime,0,0);
wheelRLTrans.Rotate(wheelFL.rpm/60*360*Time.deltaTime,0,0);
wheelFRTrans.localEulerAngles.y = wheelFR.steerAngle - wheelFRTrans.localEulerAngles.z;
wheelFLTrans.localEulerAngles.y = wheelFL.steerAngle - wheelFLTrans.localEulerAngles.z;
WheelPosition(); //graphical update - wheel positions 
}

function engine(){//engine
if(gear == 1){//neutral revs

if(engineRPM >= engineREDLINE){
engineRPM = engineRPM - 300;
}else{
if(Input.GetAxis("Throttle") > 0){
yield WaitForSeconds (0.1);
engineRPM = 100 + engineRPM;
}else{
if(engineRPM > 800){
yield WaitForSeconds (0.05);
engineRPM = engineRPM - 100;
}
}


}
}else{ //drive revs
engineRPM = wheelRPM * gears[gear] * ratio + 800;
}

if(gear > 0){
unitOutput = (engineRPM / 1000) * (engineRPM / 1000) + 120; //ENGINE OUTPUT TO WHEELS
}else{
unitOutput = -(engineRPM / 1000) * (engineRPM / 1000) - 120; //reverse output
}
if(engineRPM > engineREDLINE || gear == 1  || Input.GetAxis("Throttle") < 0){//throttle & rev limit
wheelFR.motorTorque = 0;
wheelFL.motorTorque = 0;
}else{
wheelFR.motorTorque = unitOutput * Input.GetAxis("Throttle");
wheelFL.motorTorque = unitOutput * Input.GetAxis("Throttle");
}
}
function gearbox(){
if(Input.GetButtonDown("ShiftUp") == true && gear < 7 && shifting == false){
shifting = true;
gear = gear + 1;
yield WaitForSeconds (0.3);
shifting = false;
}
if(Input.GetButtonDown("ShiftDown") == true && gear > 0 && shifting == false){
shifting = true;
gear = gear - 1;
yield WaitForSeconds (0.1);
shifting = false;
}
}

function WheelPosition(){ //graphical update - wheel positions 
var hit : RaycastHit;
var wheelPos : Vector3;
//FL
if (Physics.Raycast(wheelFL.transform.position, -wheelFL.transform.up,hit,wheelFL.radius+wheelFL.suspensionDistance) ){
wheelPos = hit.point+wheelFL.transform.up * wheelFL.radius;
}
else {
wheelPos = wheelFL.transform.position -wheelFL.transform.up* wheelFL.suspensionDistance;
}
wheelFLTrans.position = wheelPos;
//FR
if (Physics.Raycast(wheelFR.transform.position, -wheelFR.transform.up,hit,wheelFR.radius+wheelFR.suspensionDistance) ){
wheelPos = hit.point+wheelFR.transform.up * wheelFR.radius;
}
else {
wheelPos = wheelFR.transform.position -wheelFR.transform.up* wheelFR.suspensionDistance;
}
wheelFRTrans.position = wheelPos;
//RL
if (Physics.Raycast(wheelRL.transform.position, -wheelRL.transform.up,hit,wheelRL.radius+wheelRL.suspensionDistance) ){
wheelPos = hit.point+wheelRL.transform.up * wheelRL.radius;
}
else {
wheelPos = wheelRL.transform.position -wheelRL.transform.up* wheelRL.suspensionDistance;
}
wheelRLTrans.position = wheelPos;
//RR
if (Physics.Raycast(wheelRR.transform.position, -wheelRR.transform.up,hit,wheelRR.radius+wheelRR.suspensionDistance) ){
wheelPos = hit.point+wheelRR.transform.up * wheelRR.radius;
}
else {
wheelPos = wheelRR.transform.position -wheelRR.transform.up* wheelRR.suspensionDistance;
}
wheelRRTrans.position = wheelPos;
}

function OnGUI (){//dial
GUI.DrawTexture(Rect(Screen.width - 250,Screen.height-160,240,160),speedo);
var speedFactor : float = engineRPM / engineREDLINE;
var rotationAngle : float;
if (engineRPM >= 0){
  rotationAngle = Mathf.Lerp(-70,160,speedFactor);
  }
GUIUtility.RotateAroundPivot(rotationAngle,Vector2(Screen.width-162,Screen.height-77));
GUI.DrawTexture(Rect(Screen.width - 215,Screen.height-112,100,67),speedopoint);
 
}