using System;
using UnityEngine.UI;
using UnityEngine;

public class SetupRestrictions : MonoBehaviour {

    //car setup restrictions
    //if non adjustable, just put min and max and default to same value.
    public string CarName;
    //initiate stuff here so it will be faster to do shit in inspector.
    public TuneValue[] RestrictionList = new TuneValue[] {
        new TuneValue("Aero",2,4,3),
        new TuneValue("LSD",1,1,1),
        new TuneValue("TorqueSplit",0.5f,0.6f,0.5f),
        new TuneValue("SteerLock",20,30,20),
        new TuneValue("FrontSpringHeight",0.1f,0.1f,0.1f),
        new TuneValue("FrontSpringStiffness",2000,4000,2000),
        new TuneValue("FrontDamperStiffness",200,400,200),
        new TuneValue("RearSpringHeight",0.1f,0.1f,0.1f),
        new TuneValue("RearSpringStiffness",2000,4000,2000),
        new TuneValue("RearDamperStiffness",200,400,200),
        new TuneValue("BrakeStrength",150,250,200),
        new TuneValue("BrakeBalance",0.4f,0.6f,0.5f)
    };
    //brake balance & torq split. 1 = front
    //allowed final drive ratios.if not adjustable put 1 length
    public GearboxRatios gearbox;
    //force gearbox? prob a sim value feature or sth...
    

    public void SetSlider(string search, float value)
    {
        //just loop through the items like a normal search
        foreach (TuneValue part in RestrictionList)
        {
            //ok found item
            if (part.fieldName == search)
            {
                //make sure the part is adjustable. aka slider exists.
                if(part.valueSlider == null)
                {
                    break;
                }
                else
                {
                    part.valueSlider.value = value;
                    break;
                }
            }
        }
    }

    public float GetSlider(string search)
    {
        //just loop through the items like a normal search
        foreach (TuneValue part in RestrictionList)
        {
            //ok found item
            if (part.fieldName == search)
            {
                //make sure the part is adjustable. aka slider exists.
                //if not, return default.
                if (part.valueSlider == null)
                {
                    return part.defaultValue;
                }
                else
                {
                    return part.valueSlider.value;
                }
            }
        }
        //if you end up here you fucked up
        Debug.Log("you fucked up somewhere. GetSlider() in SetupRestrictions cant find what your looking for.");
        return 1;
    }


    public void SetGearboxSliders(float[] value)
    {
        //check that gearbox has sliders, if theres no first slider
        //then may as well assume the gears are not adjustable duh
        if(gearbox.GearBoxRatios[0].valueSlider != null)
        {
            int i = 0;
            foreach(SingleGear despacito in gearbox.GearBoxRatios)
            {
                despacito.valueSlider.value = value[i];
                i++;
            }
        }
    }

    public float[] GetGearboxSliders(float[] gears)
    {
        //check that gearbox has sliders, if theres no first slider
        //then may as well assume the gears are not adjustable duh
        if (gearbox.GearBoxRatios[0].valueSlider != null)
        {
            int i = 0;
            foreach (SingleGear despacito in gearbox.GearBoxRatios)
            {
                gears[i] = despacito.valueSlider.value;
                i++;
            }
            return gears;
        }
        //well if there is no slider then safe to assume i can just return what you fed the function
        return gears;
    }

    public float GetDefaultValue(string search)
    {
        foreach(TuneValue part in RestrictionList)
        {
            if(part.fieldName == search)
            {
                return part.defaultValue;
            }
        }
        //if you end up all the way here you fucked up.
        Debug.Log("UH OH WORM! error happened fetching setup data from makeshift dictionary in CarSetupRestrictions");
        return 1;
    }
}

[Serializable]
public class TuneValue
{
    public string fieldName;
    public float minValue;
    public float maxValue;
    public float defaultValue;
    public Slider valueSlider;

    //constructor to save time over writing default shit in inspector
    public TuneValue(string name, float minVal, float maxVal, float defVal)
    {
        fieldName = name;
        minValue = minVal;
        maxValue = maxVal;
        defaultValue = defVal;
        valueSlider = null;
    }
}

[Serializable]
public class GearboxRatios
{
    //multidim. 0 = first gear
    //second dim. for the ratios itself.
    public SingleGear[] GearBoxRatios;
    //if not adjustable ratios, just put the same numbers to ratios as defaultRatios
    public float[] defaultRatios;
    public float[] finalDrives;//allowed final drives. obv if not adjustable just length 1
    public Slider finalDriveSlider;
    public float defaultFinalDrive;
    public int shifter;//shifter mode 0auto 1manuel 2mclutch
}

[Serializable]
public class SingleGear
{
    public float[] ratios;
    public Slider valueSlider;
}
