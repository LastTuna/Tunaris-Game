#pragma strict
var car : Transform;
var distance : float = 6.4;
var height : float = 1.4;
var rotationDamping : float = 3.0;
var heightDamping : float = 2.0;
var zoomRacio : float = 0.5;
var DefaultFOV : float = 60;
private var rotationVector : Vector3;
function Start () {
}
 
function LateUpdate () {
var wantedAngel = rotationVector.y;
var wantedHeight = car.position.y + height;
var myAngel = transform.eulerAngles.y;
var myHeight = transform.position.y;
myAngel = Mathf.LerpAngle(myAngel,wantedAngel,rotationDamping*Time.deltaTime);
myHeight = Mathf.Lerp(myHeight,wantedHeight,heightDamping*Time.deltaTime);
var currentRotation = Quaternion.Euler(0,myAngel,0);
transform.position = car.position;
transform.position -= currentRotation*Vector3.forward*distance;
transform.position.y = myHeight;
transform.LookAt(car);
}
function FixedUpdate (){
var localVilocity = car.InverseTransformDirection(car.GetComponent.<Rigidbody>().velocity);
if (localVilocity.z<-0.5){
rotationVector.y = car.eulerAngles.y + 180;
}
else {
rotationVector.y = car.eulerAngles.y;
}
var acc = car.GetComponent.<Rigidbody>().velocity.magnitude;
GetComponent.<Camera>().fieldOfView = DefaultFOV + acc*zoomRacio;
}