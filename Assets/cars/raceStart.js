#pragma strict
var car : Transform;
var beep : AudioSource;
var launch : AudioSource;
function Start () {
	car.GetComponent(mustang).enabled = false;
	yield WaitForSeconds (1);
	beep.Play();
	yield WaitForSeconds (1);
	beep.Play();
	yield WaitForSeconds (1);
	launch.Play();
	car.GetComponent(mustang).enabled = true;

}


function Update () {
	
}
