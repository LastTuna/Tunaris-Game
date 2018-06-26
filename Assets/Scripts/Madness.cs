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
    public float power;//current power
    public float damage;//current dmg
    public float maxMag;//cars durability
    public float flipf;//speed of aerial rotation
    public float aerialBoost;//aerial boost gives slight extra air when doing flips
    public float maxAirSpeed;//to limit aerial boost
    public int i;
    public float x;
    public float negx;
    public float z;
    public float negz;
    public bool crash;
    float[] wheelTorqOnLiftOff = new float[4];

    //deform things
    public MeshFilter originalMesh;
    Mesh carMesh;
    Vector3[] ogMesh;
    Vector3[] modMesh;

    public bool carfixed;

    // Use this for initialization
    void Start()
    {
        carMesh = originalMesh.mesh;
        ogMesh = originalMesh.mesh.vertices;
        modMesh = originalMesh.mesh.vertices;

    }

    // Update is called once per frame
    void Update()
    {
        if (carfixed)
        {
            FixHoop();
        }

        WheelHit wheelHit;
        if (!wheelFL.GetGroundHit(out wheelHit) && !wheelFR.GetGroundHit(out wheelHit) && !wheelRL.GetGroundHit(out wheelHit) && !wheelRR.GetGroundHit(out wheelHit) && !crash)
        {
            if (Input.GetKeyDown("space") && !stunting)
            {
                stunting = true;
                car.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, car.GetComponent<Rigidbody>().angularVelocity.y, 0);
                wheelTorqOnLiftOff[0] = wheelFL.rpm;
                wheelTorqOnLiftOff[1] = wheelFR.rpm;
                wheelTorqOnLiftOff[2] = wheelRL.rpm;
                wheelTorqOnLiftOff[3] = wheelRR.rpm;
            }
        }
        else
        {
            x = 0;
            negx = 0;
            z = 0;
            negz = 0;
            stunting = false;
            car.GetComponent<Rigidbody>().drag = 0.1f;//re enable drag
            car.GetComponent<Rigidbody>().angularDrag = 0.05f;
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

            }//apply aerial boost
            if(maxAirSpeed > car.GetComponent<Rigidbody>().velocity.x &&
                car.GetComponent<Rigidbody>().velocity.z < maxAirSpeed &&

                car.GetComponent<Rigidbody>().velocity.x > -maxAirSpeed &&
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

            wheelFL.brakeTorque = 0;
            wheelFR.brakeTorque = 0;
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
        }
    }

    public void StuntsCounter()
    {
        //count the flips during airborne, and apply with a delay after processed


    }
    public void CarDamage()
    {
        //damage values, when you crash i suppose its rigidbody.velocity,
        //durability values
        //fix hoops - take the hoop and reset damage

        //visual damage updates


    }

    public void CarDeform(Vector3 impactPoint, float radius)
    {
        i = 0;
            foreach (Vector3 e in modMesh)
            {
                if((modMesh[i].x + car.position.x > impactPoint.x
                && modMesh[i].y + car.position.y > impactPoint.y
                && modMesh[i].z + car.position.z > impactPoint.z)
                )
                {//ATTRIBUTE WORLD POSITION OF CAR, TO THE VERTEX TRYING TO DEFORM
                //IF VERTEX IS IN RANGE OF RADIUS, MOVE IT INWARDS
                modMesh[i] = new Vector3(modMesh[i].x, modMesh[i].y, modMesh[i].z - 0.2f);
                //apply deform
                }

                i++;
            }

        carMesh.vertices = modMesh;
        originalMesh.mesh.RecalculateNormals();
        print("boxcollision, " + impactPoint.ToString());

    }

    public void FixHoop()
    {
        carMesh.vertices = ogMesh;
        modMesh = ogMesh;
        originalMesh.mesh = carMesh;
        originalMesh.mesh.RecalculateNormals();
        print("ok");

    }

    public void OnCollisionEnter(Collision collision)
    {
        crash = true;
        if (collision.collider.name != "ramp")
        {
            CarDamage();
            damage += 2 / maxMag;
        }
        //
        foreach (var contact in collision.contacts)
        {
            CarDeform(contact.point, 1f);
        }
        
    }
    public void OnCollisionExit(Collision collision)
    {
        crash = false;
    }
}