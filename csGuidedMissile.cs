using UnityEngine;


public class csGuidedMissile : csFunctionGroup
{
    public float LiveTime = 10; // Missile Survive Time.

    [HideInInspector]
    public Transform Target; // Guid Target.
                             //    public Transform Booster; // Missile's tail effect.

    public Transform ExplosionEffect, Watereffect; // missile's hit effect.
    public BulletKind _BulletKind = BulletKind.Explosive;

    [HideInInspector]
    public bool Guided; // Check Guided
    Quaternion MissileRotation;

    public bool Predict; // Check Predict Target Position.
    Vector3 OldTargetPosition;
    Vector3 NewTargetPosition;
    public Rigidbody MissileRB;

    public bool RigidBodyMove; // Check RigidBody Move.
    public float Speed = 0; // Initial Speed.
    public float SpeedMax; // Max Speed.
    public float SpeedIndex; // Increacing Index that speed every time 

    public enum MissileShakeState { Shake, NoneShake };
    public MissileShakeState MissileShakeType = MissileShakeState.NoneShake;
    public float MissileShakeScale;
    Vector3 ShakeValue;

    //Explosion force values
    public string[] TargetTag;
    public float RigidBodyHitForce = 100;
    public float RigidBodyHitRadious = 100;
    public float TargetMaxLookAngle = 20, collredious;
    public Messile_Sobject M_data;
    Vector3 DirectionValue, AdditionalPosition, Velocity;
    float _Angle, Angle, DistanceValue, waittime;
    Transform Explosion;
    public CapsuleCollider coll;
    public int sped = 1;
    private bool Isdestroy = false;
    public Rigidbody Rig_body;

    public GameObject particles;
    public bool iscam_follow = false;
    public Camera missile_cam;

    float MissileFS, MissileUFS;



    public DimissionState DimissionType = DimissionState._3D;

    void Start()
    {
        MissileUFS = csGuidedSystem.instance.M_data.M_U_Speed;
        MissileFS = csGuidedSystem.instance.M_data.M_F_Speed;
        if (camra.intance.innercam == true && iscam_follow == true)
        {
            missile_cam.gameObject.SetActive(true);
        }

        waittime = 0;
        Destroy(gameObject, LiveTime);
        if (Target)
        {
            Rig_body.useGravity = true;
            particles.SetActive(false);
            OldTargetPosition = Target.position;
        }
        else
        {
            Rig_body.useGravity = false;
        }

    }

    float AngleValue(Vector3 Target, Transform _Transform)
    {
        DirectionValue = (Target - _Transform.position).normalized;
        _Angle = Vector3.Angle(DirectionValue, _Transform.forward);
        return _Angle;
    }

    void FixedUpdate()
    {

        if (MissileShakeType == MissileShakeState.Shake) // Check Random.
            ShakeValue = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
        else if (MissileShakeType == MissileShakeState.NoneShake)
            ShakeValue = new Vector3(0, 0, 0);

        if (RigidBodyMove)
        {
            if (DimissionType == DimissionState._3D)
            {
                Velocity = Vector3.zero; //set Velocity value that is Vector3 Information.
                Velocity = (transform.rotation * Vector3.forward + ShakeValue * MissileShakeScale) * MissileUFS; //Set Velocity.
                MissileRB.velocity = Vector3.Lerp(MissileRB.velocity, Velocity, Time.fixedDeltaTime); // Change this rigidbody's Velocity to Velocity Value via Time.fixedDeltaTime.
            }

        }
        else if (!RigidBodyMove && !Target)
        {
            transform.Translate(Vector3.forward * MissileUFS * sped * Time.fixedDeltaTime);
        }

    }

    void Update()
    {
        
        if (Target && Guided)
        {
            waittime = waittime + Time.deltaTime;
            if (waittime >= 1f)
            {
                if (!Predict)
                {
                    NewTargetPosition = Target.position; //Set Target Position.

                    Angle = AngleValue(NewTargetPosition, this.transform); // set Angle between NewTargetPosition this transform
                    if (Angle > TargetMaxLookAngle) // Check Angle.
                    {
                        Target = null;
                    }
                       
                }
                else if (Predict)
                {
                    AdditionalPosition = Target.position - OldTargetPosition; //Set Vector3 value between Target and OldTargetPosition.

                    DistanceValue = (transform.position - Target.position).magnitude / (MissileUFS * Time.smoothDeltaTime); //Set Length between transform.position, Target.position.

                    NewTargetPosition = Target.position + AdditionalPosition * DistanceValue; //set predicted target position. 
                }

                MissileRotation = Quaternion.LookRotation(NewTargetPosition - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, MissileRotation, Time.smoothDeltaTime * 10);
                transform.Translate(Vector3.forward * MissileUFS * sped / 1.5f * Time.fixedDeltaTime);
            }
            else if (waittime >= .5f && waittime < 0.8f)
            {
                if(particles.activeInHierarchy==false)
                {
                    particles.SetActive(true);
                    Rig_body.useGravity = false;
                }
                
                transform.Translate(Vector3.forward * MissileUFS * sped / 1.5f * Time.fixedDeltaTime);
            }


        }

        if (MissileUFS < SpeedMax)
        {
            Speed += MissileFS * Time.fixedDeltaTime;
            if (MissileUFS > SpeedMax)
                MissileUFS = SpeedMax;
        }
    }

    void OnTriggerEnter(Collider Col) //Work on 3D collider
    {
        for (int i = 0; i < TargetTag.Length; i++)
        {
            if (Col.gameObject.tag == TargetTag[i] && Isdestroy == false)
            {
                Isdestroy = true;
                coll.radius = collredious;
                sped = 0;
                if (_BulletKind == BulletKind.Explosive)
                {
                    ApplyForce(TargetTag, RigidBodyHitForce, RigidBodyHitRadious, transform.position);
                }
                Explosion = Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
                Destroy(Explosion.gameObject, 3);  // uncomment after video shoot
                Destroy(this.gameObject, 0.5f);
                return;
            }
            else if (Col.gameObject.tag == "W" && Isdestroy == false)
            {
                Isdestroy = true;
                coll.radius = collredious;
                sped = 0;
                Explosion = Instantiate(Watereffect, transform.position, Quaternion.identity);
                Destroy(Explosion.gameObject, 3); // uncomment after video shoot
                Destroy(this.gameObject, 0.5f);
                return;
            }

        }
    }


}