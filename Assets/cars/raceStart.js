#pragma strict
var beer : Transform;

function Start () {
	beer.GetComponent(mustang).enabled = false;
	yield WaitForSeconds (3);
	beer.GetComponent(mustang).enabled = true;
}

function Update () {
	
}
