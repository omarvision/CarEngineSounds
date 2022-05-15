using TMPro;
using UnityEngine;

public class Car : MonoBehaviour
{
    #region -- helpers --
    [System.Serializable]
    public class clsSounds
    {
        public string fileEngine = "car engine 02";
        public string fileStart = "car start 02";
        public string fileShift1 = "car shift 1";
        public string fileShift2 = "car shift 2";
        public string fileAmbient = "car ambient";
        public string fileCrash1 = "car crash 1";
        public string fileCrash2 = "car crash 2";
        public string fileCrash3 = "car crash 3";

        [HideInInspector]
        public AudioSource audEngine = null;
        [HideInInspector]
        public AudioSource audStart = null;
        [HideInInspector]
        public AudioSource audShift1 = null;
        [HideInInspector]
        public AudioSource audShift2 = null;
        [HideInInspector]
        public AudioSource audAmbient = null;
        [HideInInspector]
        public AudioSource audCrash1 = null;
        [HideInInspector]
        public AudioSource audCrash2 = null;
        [HideInInspector]
        public AudioSource audCrash3 = null;

        public float MaxSpeed = 36;
        [HideInInspector]
        public int Gear;
        public float[] GearRatio = new float[] { 24, 36 };
        public float BasePitch = 1.0f;
        public float StartToIdleLeadTime = 0.60f;

        private AudioSource CreateAudioSource(string filename, GameObject parent)
        {
            AudioSource aud = parent.AddComponent<AudioSource>();

            aud.clip = Resources.Load<AudioClip>(filename);
            if (aud.clip == null)
                Debug.LogError(string.Format("Error: audiosource clip did not load [filename]={0} [gameobject]={1}", filename, parent.name));

            return aud;
        }
        public void LoadSounds(GameObject parent)
        {
            audEngine = CreateAudioSource(fileEngine, parent);
            audEngine.loop = true;

            audStart = CreateAudioSource(fileStart, parent);
            audStart.loop = false;

            audShift1 = CreateAudioSource(fileShift1, parent);
            audShift1.loop = false;

            audShift2 = CreateAudioSource(fileShift2, parent);
            audShift2.loop = false;

            audAmbient = CreateAudioSource(fileAmbient, parent);
            audAmbient.loop = true;

            audCrash1 = CreateAudioSource(fileCrash1, parent);
            audCrash1.loop = false;

            audCrash2 = CreateAudioSource(fileCrash2, parent);
            audCrash2.loop = false;

            audCrash3 = CreateAudioSource(fileCrash3, parent);
            audCrash3.loop = false;
        }
    }
    private enum WheelsDown
    {
        OnRoad = 0,
        OnNonRoad = 1,
        Flipped = 2
    }
    private class clsFlip
    {
        public WheelsDown wheelsdown = WheelsDown.OnRoad;
        public bool bInvokeReset = false;
        public float CarFlippedTime = 1.0f;
        public Vector3 left_road_position;
        public Bounds bnd;
    }
    #endregion

    public float MoveSpeed = 50;
    public float Turnspeed = 60;
    public clsSounds snd = new clsSounds();
    private Rigidbody rb = null;    
    private TextMeshPro carlabel = null;
    private bool bCanMove = false;
    private clsFlip flip = new clsFlip();

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        carlabel = this.transform.Find("carlabel").GetComponent<TextMeshPro>();

        snd.LoadSounds(this.gameObject);

        PlayStartSound();

        flip.bnd = this.GetComponent<MeshRenderer>().bounds;
    }
    private void FixedUpdate()
    {
        if (bCanMove == true)
        {
            float move = Input.GetAxis("Vertical");     // w, s  up, down arrows      (-1..0..1)
            float turn = Input.GetAxis("Horizontal");   // a, d  left, right arrows

            if (move < 0)
            {
                rb.AddRelativeForce(Vector3.back * MoveSpeed * Time.deltaTime, ForceMode.VelocityChange);
            }
            else if (move > 0)
            {
                rb.AddRelativeForce(Vector3.forward * MoveSpeed * Time.deltaTime, ForceMode.VelocityChange);
            }

            Vector3 rotateTo;
            if (turn < 0)
            {
                rotateTo = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y - 20, this.transform.eulerAngles.z);
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(rotateTo), Turnspeed * Time.deltaTime);
            }
            else if (turn > 0)
            {
                rotateTo = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + 20, this.transform.eulerAngles.z);
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(rotateTo), Turnspeed * Time.deltaTime);
            }
        }

        DoCarSounds();

        switch (isWheelsDown())
        {
            case WheelsDown.OnRoad:
                break;
            case WheelsDown.OnNonRoad:
                if (flip.bInvokeReset == false)
                {
                    flip.bInvokeReset = true;
                    Invoke("ResetCar", flip.CarFlippedTime);
                }
                break;
            case WheelsDown.Flipped:
                if (flip.bInvokeReset == false)
                {
                    flip.bInvokeReset = true;
                    Invoke("ResetCar", flip.CarFlippedTime);
                }
                break;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BarrierLeft") == true 
            || collision.gameObject.CompareTag("BarrierRight") == true
            || collision.gameObject.CompareTag("Car") == true)
        {
            float hitz = collision.relativeVelocity.z;
            float hitx = collision.relativeVelocity.x;
            if (hitz > hitx * 2)
            {
                PlayCarHitSound();
            }
            else
            {
                snd.audCrash3.Play();
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("GravelLeft") == true
            || collision.gameObject.CompareTag("GravelRight") == true
            || collision.gameObject.CompareTag("RoadLeft") == true
            || collision.gameObject.CompareTag("RoadRight") == true)
        {
            flip.left_road_position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        }
    }
    private void PlayStartSound()
    {
        snd.audStart.Play();
        Invoke("PlayEngineSound", snd.audStart.clip.length * snd.StartToIdleLeadTime);
    }
    private void PlayEngineSound()
    {
        snd.audEngine.Play();

        snd.audAmbient.Play();
        snd.audAmbient.volume = 0;

        bCanMove = true;
    }
    private void PlayCarHitSound()
    {
        snd.audCrash1.Play();
        snd.audEngine.Stop();
        bCanMove = false;
        Invoke("PlayStartSound", snd.audCrash1.clip.length);
    }
    private void PlayCarRollSound()
    {
        snd.audCrash2.Play();
        snd.audEngine.Stop();
        bCanMove = false;
        Invoke("PlayStartSound", snd.audCrash2.clip.length);
    }
    private void DoCarSounds()
    {
        //speed?
        float carspeed = rb.velocity.magnitude;

        //-------------------------------------------------
        //what is the gear?
        for (int i = 0; i < snd.GearRatio.Length; i++)
        {
            if (snd.GearRatio[i] > carspeed)
            {
                if (snd.Gear < i) //shifting up
                    snd.audShift1.Play();
                else if (snd.Gear > i) //shifting down
                    snd.audShift2.Play();

                //change the gear
                snd.Gear = i;
                break;
            }
        }

        //speed range is based gear?
        float gearmin;
        float gearmax;
        if (snd.Gear == 0)
        {
            gearmin = 0;
            gearmax = snd.GearRatio[snd.Gear];
        }
        else
        {
            gearmin = snd.GearRatio[snd.Gear - 1];
            gearmax = snd.GearRatio[snd.Gear];
        }

        //engine sound pitch?
        float pitch = (carspeed - gearmin) / (gearmax - gearmin);   //linear value
        pitch = snd.BasePitch + Mathf.Pow(pitch, 0.5f);  //this is curved (rise fast, then slower)
        snd.audEngine.pitch = pitch;

        //-------------------------------------------------
        //ambient sound volume based on speed
        float volume = carspeed / snd.MaxSpeed;
        snd.audAmbient.volume = Mathf.Pow(volume, 0.5f);

        //-------------------------------------------------
        //am I rolling crashing?
        float rollx = rb.angularVelocity.x;
        float rollz = rb.angularVelocity.z;        
        if ((rollx > 1 || rollz > 1) && flip.wheelsdown == WheelsDown.Flipped)
        {
            if (snd.audCrash2.isPlaying == false)
                PlayCarRollSound();
        }

        carlabel.text = string.Format("{0} {1}", carspeed.ToString("0"), snd.Gear + 1);
    }
    private WheelsDown isWheelsDown()
    {
        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, flip.bnd.size.y * 0.55f) == true)
        {
            if (hit.collider.CompareTag("GravelLeft") == true
                || hit.collider.CompareTag("GravelRight") == true
                || hit.collider.CompareTag("RoadLeft") == true
                || hit.collider.CompareTag("RoadRight") == true)
            {
                flip.wheelsdown = WheelsDown.OnRoad;
            }
            else
            {
                flip.wheelsdown = WheelsDown.OnNonRoad;
            }
        }
        else
        {
            flip.wheelsdown = WheelsDown.Flipped;
        }

        return flip.wheelsdown;
    }
    private void ResetCar()
    {
        flip.bInvokeReset = false;
        Vector3 eulerRotation;

        //flip the car upright, over that last known drivable road position
        switch (flip.wheelsdown)
        {
            case WheelsDown.OnNonRoad:                
            case WheelsDown.Flipped:
                if (rb.angularVelocity.magnitude > 0.1f) //is car still rolling?
                    break;
                this.transform.position = flip.left_road_position;  //car over road
                eulerRotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0); //flip wheels down
                break;
        }
    }
}
