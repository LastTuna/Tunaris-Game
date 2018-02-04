#pragma strict
var beep : AudioSource;
var launch: AudioSource;

function Start() {
	yield WaitForSeconds (1);
	beep.Play();
	yield WaitForSeconds (1);
	beep.Play();
	yield WaitForSeconds (1);
	launch.Play();

}


function Update () {
	
}
