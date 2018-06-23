using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Madness : MonoBehaviour
{
    public Transform car;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public bool stunting;
    public float power;
    public float damage;
    public float flipf;//speed of aerial rotation
    public float aerialBoost;//aerial boost gives slight extra air when doing flips
    public float maxAirSpeed;//to limit aerial boost
    public int i;
    public float x;
    public float negx;
    public float z;
    public float negz;
    public bool crash;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        WheelHit wheelHit;
        if (!wheelFL.GetGroundHit(out wheelHit) && !wheelFR.GetGroundHit(out wheelHit) && !wheelRL.GetGroundHit(out wheelHit) && !wheelRR.GetGroundHit(out wheelHit) && crash)
        {
            if (Input.GetKeyDown("space"))
            {
                stunting = true;
                car.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, car.GetComponent<Rigidbody>().angularVelocity.y, 0);
            }
        }
        else
        {
            stunting = false;
        }
    }

    void FixedUpdate()
    {

        StuntsEngine();
    }

    public void RotDamping()
    {
        if (Input.GetKey("up"))
        {//frontflip
            if (x < flipf)
            {
                x = x + flipf / (20 / flipf);

            }//apply transforms
            if(maxAirSpeed > car.GetComponent<Rigidbody>().velocity.x &&
                car.GetComponent<Rigidbody>().velocity.z < maxAirSpeed &&

                car.GetComponent<Rigidbody>().velocity.x > -maxAirSpeed &&///////wip
                car.GetComponent<Rigidbody>().velocity.z > -maxAirSpeed
                )
            {
                car.GetComponent<Rigidbody>().AddForce(new Vector3(
                aerialBoost + car.GetComponent<Rigidbody>().velocity.x,
                car.GetComponent<Rigidbody>().velocity.y,
                aerialBoost + car.GetComponent<Rigidbody>().velocity.z), ForceMode.Acceleration);

            }

            car.GetComponent<Rigidbody>().AddForce(new Vector3(0, aerialBoost, 0), ForceMode.Acceleration);
        }
        else
        {
            if (x > 0)
            {
                x = x - flipf / (40 / flipf);
            }
        }
        //backflip
        if (Input.GetKey("down"))
        {
            if (negx > -flipf)
            {
                negx = negx - flipf / (20 / flipf);

            }//apply transforms
            car.GetComponent<Rigidbody>().AddForce(new Vector3(0, aerialBoost * 1.5f, 0), ForceMode.Acceleration);
        }
        else
        {
            if (negx < 0)
            {
                negx = negx + flipf / (40 / flipf);
            }
        }
        //---------------------------------------------------
        if (Input.GetKey(KeyCode.LeftArrow))
        {//roll left
            if (z < flipf)
            {
                z = z + flipf / (20 / flipf);

            }//apply transforms
            car.GetComponent<Rigidbody>().AddForce(new Vector3(0, aerialBoost, 0), ForceMode.Acceleration);
        }
        else
        {
            if (z > 0)
            {
                z = z - flipf / (40 / flipf);
            }
        }
        //roll right
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (negz > -flipf)
            {
                negz = negz - flipf / (20 / flipf);

            }//apply transforms
            car.GetComponent<Rigidbody>().AddForce(new Vector3(0, aerialBoost * 1.5f, 0), ForceMode.Acceleration);
        }
        else
        {
            if (negz < 0)
            {
                negz = negz + flipf / (40 / flipf);
            }
        }
    }

    public void StuntsEngine()
    {
        //when stunting is active, take inputs to roll / flip etc, then turn false after car hits ground
        if (stunting)
        {
            RotDamping();
            car.GetComponent<Rigidbody>().drag = 0;//kill drag
            car.GetComponent<Rigidbody>().angularDrag = 0;

            car.rotation *= Quaternion.AngleAxis(x + negx, new Vector3(1, 0, 0));//front/backflips
            car.rotation *= Quaternion.AngleAxis(z + negz, new Vector3(0, 0, 1));//rolls front/back

        }

    }

    public void StuntsCounter()
    {
        //count the flips during airborne, and apply with a delay after processed


    }

    public void OnCollisionEnter(Collision collision)
    {
            crash = false;//i demand euthanasia
    }

}