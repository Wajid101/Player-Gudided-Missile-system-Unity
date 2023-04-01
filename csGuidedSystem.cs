using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class csGuidedSystem : csFunctionGroup
{
    public static csGuidedSystem instance;
    public Messile_Sobject M_data;
    public float M_D_time = 0.1f;
    public Text mcount_text;
    Camera cam;
    public int messilekit = 5, m_count = 0, misilecount;

    public string[] TargetTag; //SetTarget

    GameObject SingleTarget; //Single Target

    List<GameObject> MultiTarget = new List<GameObject>(); //Multi Target
    public int MaxMultiTargetNumber; //Able to Target Lock Number

    public enum TargettingState { Single, Multi };
    public TargettingState TargettingType = TargettingState.Single;
    public float LockedObjectAngle; //Max Locked Object Angle Value;
    public int AbleToLockDistance = 500; //MaxDistance

    public Transform[] FirePort;
    public enum PortOrder { Random, Order }
    public PortOrder PortType = PortOrder.Order;
    int PortNumber = 0;

    public Transform Missile;
    public bool MissileGuided; //Check Missile Guided

    public float MaxFireDelayTime;
    float NowFireDelayTime;

    public enum MissileFireState { OrderAttack, RandomAttack };
    public MissileFireState MultiMissileFireType = MissileFireState.OrderAttack;
    int MultiMissileNumber = -1;

    public enum SortOrderState { Ascending, Descending };
    public SortOrderState SortOrderType = SortOrderState.Descending;

    public Texture LockedTexture;
    public static bool radio;
    public AudioSource messile;
    public int Ammo_cast = 30;
    public bool isjetfighter = false;


    float AngleValue(Transform Target, Transform _Transform) //return Angle Value between Target, _Transform
    {
        Vector3 DirectionValue = (Target.transform.position - _Transform.position).normalized; //Set Direction Value between target and _Trasnform.
        float Angle = Vector3.Angle(DirectionValue, _Transform.forward); //Set Angle Value between DirectionValue and forward transform.
        return Angle; // return Angle
    }

    void Start()
    {
        if (isjetfighter == true)
        {


            M_data.M_F_Speed = M_data.M_F_Speed * 1.3f;
            M_data.Megsize = M_data.Megsize - 2;
            M_data.M_U_Speed = M_data.M_U_Speed * 1.3f;
        }
        else
        {
            AbleToLockDistance = M_data.Range;
        }
        m_count = 0;
        misilecount = M_data.Megsize;
        mcount_text.text = misilecount.ToString();
        cam = Camera.main;
    }

    void Update()
    {
        NowFireDelayTime += Time.deltaTime;
        Guided();
        CheckLockedObjectState();
        

        if (cam == null)
        {
            cam = Camera.main;
        }
        if (instance == null)
        {
            instance = this;
        }



    }

    void Guided()
    {
        int NowMultiTargetNumber = 0;
        List<GameObject> TargetObjectsList = new List<GameObject>(); //Multi Target
        //if just Clear MultiTarget List, Not in MultiTarget but csGuidedCheck's isLocked is true.
        //so if MultiTarget.Count is more than 0, change true -> false. before MultiTarget.Clear() execute.
        if (MultiTarget.Count > 0)
        {
            for (int i = 0; i < MultiTarget.Count; i++)
            {
                if (MultiTarget[i] != null)
                {
                    csGuidedCheck LockedCheck = (csGuidedCheck)MultiTarget[i].gameObject.GetComponent<csGuidedCheck>(); // if didnt work  ("csGuidedCheck") replace with <csGuidedCheck>

                    LockedCheck.isLocked = false;
                }

            }
            MultiTarget.Clear();
        }

        SingleTarget = null;

        for (int i = 0; i < TargetTag.Length; i++) // Check Target Length.
        {
            GameObject[] TargetObjects = GameObject.FindGameObjectsWithTag(TargetTag[i]); //Find TargetTag Object.

            for (int j = 0; j < TargetObjects.Length; j++)
            {
                if (TargetObjects[j])
                {
                    float Angle = AngleValue(TargetObjects[j].transform, transform); // set Angle
                    float Distance = (TargetObjects[j].transform.position - transform.position).magnitude; // set Distance

                    if (Angle <= LockedObjectAngle && Distance < AbleToLockDistance) // Check Angle and Distnace.
                        TargetObjectsList.Add(TargetObjects[j]); // if suitable to if function, Add TargetObjects to TargetObjectLists.
                }
            }

            //TargetObjectsList Sort.
            for (int j = 0; j < TargetObjectsList.Count; j++)
            {
                for (int x = 1; x < TargetObjectsList.Count; x++)
                {
                    GameObject TargetA = TargetObjectsList[x - 1];
                    float TargetADistance = (TargetObjectsList[x - 1].transform.position - transform.position).magnitude;

                    GameObject TargetB = TargetObjectsList[x];
                    float TargetBDistance = (TargetObjectsList[x].transform.position - transform.position).magnitude;

                    if (SortOrderType == SortOrderState.Descending) //if AttackOrderType is Descending, Sort TargetObjectsList Descending.
                    {
                        if (TargetADistance > TargetBDistance)
                        {
                            TargetObjectsList[x - 1] = TargetB;
                            TargetObjectsList[x] = TargetA;
                        }
                    }
                    else if (SortOrderType == SortOrderState.Ascending) //if AttackOrderType is Ascending, Sort TargetObjectsList Ascending.
                    {
                        if (TargetADistance < TargetBDistance)
                        {
                            TargetObjectsList[x - 1] = TargetB;
                            TargetObjectsList[x] = TargetA;
                        }
                    }
                }
            }

            for (int j = 0; j < TargetObjectsList.Count; j++)
            {
                if (TargetObjectsList[j])
                {
                    float Angle = AngleValue(TargetObjectsList[j].transform, transform); // set Angle
                    float Distance = (TargetObjectsList[j].transform.position - transform.position).magnitude; // set Distance


                    if (Angle <= LockedObjectAngle && Distance < AbleToLockDistance) // Check Angle and Distnace.
                    {
                        if (TargettingType == TargettingState.Multi) //Check TargettingType.
                        {
                            if (NowMultiTargetNumber < MaxMultiTargetNumber)
                            {
                                MultiTarget.Add(TargetObjectsList[j]);
                                NowMultiTargetNumber++;
                            }
                        }
                        else
                            SingleTarget = TargetObjectsList[0];
                    }
                }
            }

        }
    }

    void CheckLockedObjectState()
    {
        if (TargettingType == TargettingState.Multi)
        {
            for (int i = 0; i < MultiTarget.Count; i++)
                CheckTargetAngle(MultiTarget[i], transform);
        }
        else if (TargettingType == TargettingState.Single)
        {
            if (SingleTarget)
                CheckTargetAngle(SingleTarget, transform);
        }
    }

    void CheckTargetAngle(GameObject _Target, Transform _Transform)
    {
        float Angle = AngleValue(_Target.transform, _Transform); // Set Angle Value
        float Distance = (_Target.transform.position - _Transform.position).magnitude; // Set Distance.

        if (Distance > AbleToLockDistance || Angle >= LockedObjectAngle)
        {
            // Check Angle and Distance.
            if (TargettingType == TargettingState.Multi)
            {
                // Check TargettingType.

                MultiTarget.Remove(_Target.gameObject);

            }
            else
            {
                if (_Target == SingleTarget)
                    SingleTarget = null;
            }

        }
        else
        {

            radio = true;
            //			Debug.Log(radio);

        }


    }

    public IEnumerator Firing()
    {


        //		Debug.Log (m_count);
        if (m_count < M_data.Megsize)
        {

            if (NowFireDelayTime > MaxFireDelayTime)
            {

                m_count++;
                misilecount--;
                mcount_text.text = misilecount.ToString();
                NowFireDelayTime = 0;
                if (TargettingType == TargettingState.Multi)
                {
                    for (int i = 0; i < messilekit; i++)
                    {
                        yield return new WaitForSeconds(M_D_time);
                        Fire();
                    }
                }
                else
                    Fire();
            }

        }

    }

    void Fire()
    {
        messile.Play();
        Transform Port = null;
        Dbonus_Acast.instance.Ammo_Cast += Ammo_cast;
        if (FirePort.Length > 0)
            Port = FirePortCheck(); // set Port.

        if (Port)
        {
            Transform _Missile = Instantiate(Missile, Port.transform.position, Port.transform.rotation) as Transform; //Make Missile

            if ((csGuidedMissile)_Missile.gameObject.GetComponent<csGuidedMissile>())
            {
                //Receive Code 'csGuidedMissile' if 'csGuidedMissile' Code Attached in Maked Missile.
                csGuidedMissile MissileCode = (csGuidedMissile)_Missile.gameObject.GetComponent<csGuidedMissile>();
                MissileCode.Guided = MissileGuided; // Give Guided bool Value that is ture or False.

                if (TargettingType == TargettingState.Multi)
                { // Check Targetting Type.
                    if (MultiMissileFireType == MissileFireState.RandomAttack)
                    {
                        int Count = 0;

                        for (int i = 0; i < MultiTarget.Count; i++)
                            Count++;

                        int Rnd = Random.Range(Count, -1);
                        if (MultiTarget.Count > 0)
                            MissileCode.Target = MultiTarget[Rnd].transform;
                    }
                    else if (MultiMissileFireType == MissileFireState.OrderAttack)
                    {
                        if (MultiMissileNumber >= MultiTarget.Count - 1)
                            MultiMissileNumber = 0;
                        else
                            MultiMissileNumber++;

                        if (MultiTarget.Count > 0)
                            MissileCode.Target = MultiTarget[MultiMissileNumber].transform;
                    }
                }
                else
                {
                    if (SingleTarget)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            MissileCode.Target = SingleTarget.transform;
                        }
                    }
                }
            }
        }
    }

    Transform FirePortCheck()
    {
        Transform Port = null;

        if (FirePort.Length > 0)
        {
            if (PortType == PortOrder.Random) // Check Fire Type.
            {
                int PortLength = FirePort.Length;
                int RandomValue = Random.Range(0, PortLength);
                Port = FirePort[RandomValue];
            }
            else if (PortType == PortOrder.Order)
            {
                Port = FirePort[PortNumber];
                if (PortNumber <= FirePort.Length - 2)
                    PortNumber++;
                else
                    PortNumber = 0;
            }
        }
        return Port;
    }

    void OnGUI()
    {
        if (misilecount > 0)
        {
            DisplayTargetTexture();
        }
    }

    void DisplayTargetTexture()
    {
        for (int i = 0; i < TargetTag.Length; i++)
        {

            GameObject[] TargetObjects = GameObject.FindGameObjectsWithTag(TargetTag[i]); //Find TargetTag Object.

            if (TargetObjects.Length > 0)
            {
                for (int j = 0; j < TargetObjects.Length; j++)
                {
                    csGuidedCheck LockedCheck;
                    

                    if ((csGuidedCheck)TargetObjects[j].gameObject.GetComponent<csGuidedCheck>())
                        LockedCheck = (csGuidedCheck)TargetObjects[j].gameObject.GetComponent<csGuidedCheck>(); // if didnt work  ("csGuidedCheck") replace with <csGuidedCheck> // Receive 'csGuidedCheck' Code That is Attached TargetObjects.
                    else
                        return;

                    if (TargettingType == TargettingState.Multi)
                    { // Check TarggettingType.
                        for (int n = 0; n < MultiTarget.Count; n++)
                        {
                            if (TargetObjects[j] == MultiTarget[n])
                            { // Check TargetObject that TargetObject is Same MultiTarget.
                               // Debug.Log(TargetObjects[j]);
                                LockedCheck.isLocked = true;

                                break;
                            }
                            else
                            {
                                radio = false;
                                LockedCheck.isLocked = false;

                            }
                        }
                    }
                    else
                    {
                        if (TargetObjects[j] == SingleTarget)
                        {// Check TargetObject that TargetObject is Same Target
                            LockedCheck.isLocked = true;

                        }
                        else
                        {
                            radio = false;
                            LockedCheck.isLocked = false;
                        }
                    }
                    

                    if (LockedCheck.isLocked == true && UIGameController.instance.ison == false)
                    {

                        DisplayObjects(TargetObjects[j], LockedTexture, true, 1.75f); //Draw Locked Target Texture.
                    }

                }

            }
        }
    }

    void DisplayObjects(GameObject Target, Texture _Texture, bool Locked, float TextureScale)  //Draw Texture 
    {
        float Angle = AngleValue(Target.transform, transform); // set Angle
        float Distance = (Target.transform.position - transform.position).magnitude; // set Distacne
                                                                                     // float distance = Vector3.Distance(transform.position, Target.transform.position);

        float _AimAngle;

        // if Locked Checked, _AimAngle is LockedObject Angle, if not, _AimAngle = NoneLocekdObjectAngle.
        if (Locked)
        {
            _AimAngle = LockedObjectAngle;
        }


        float TargetWidth = _Texture.width / TextureScale;
        float TargetHeight = _Texture.height / TextureScale;

        if (Distance > AbleToLockDistance) // Check Distance and Angle
            return;

        Vector3 screenPos = cam.WorldToScreenPoint(Target.transform.position);
        //		Vector3 screenPos =cam.WorldToScreenPoint(Target.transform.position);
        GUI.DrawTexture(new Rect(screenPos.x - TargetWidth / 2, Screen.height - screenPos.y - TargetHeight / 2, TargetWidth, TargetHeight), _Texture);
        GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y, 200, 30), Target.name + " " + Mathf.Floor(Distance) + "m.");
    }

    public void fireing()
    {
        StartCoroutine(Firing());
    }


    public void Refilling(int amount)
    {
        misilecount += amount;
        if (misilecount >= M_data.Megsize)
        {
            misilecount = M_data.Megsize;
            mcount_text.text = misilecount.ToString();
        }
        else
        {
            mcount_text.text = misilecount.ToString();
        }
    }

    public void Data_reload()
    {
        m_count = 0;
        misilecount = M_data.Megsize;
        mcount_text.text = misilecount.ToString();
    }
}