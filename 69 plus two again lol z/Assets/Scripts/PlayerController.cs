using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameObject bar1, leftArm, rightArm, torso, head, leftFemur, rightFemur, leftLowerLeg, rightLowerLeg, leftFoot, rightFoot; // get GameObjects from editor
    HingeJoint leftBarJoint, rightBarJoint, leftShoulder, rightShoulder, neck, leftHip, rightHip, leftKnee, rightKnee, leftAnkle, rightAnkle; // init player joints 
    JointSpring leftShoulderSpring, rightShoulderSpring, neckSpring, leftHipSpring, rightHipSpring, leftKneeSpring, rightKneeSpring, leftAnkleSpring, rightAnkleSpring; // init spring for each joint

    public Transform leftHand, rightHand; // used to check distance for regrabs

    public float strength, damp; // determine strength of noomi. strength is set dynamically, damp stays the same

    public AudioSource metalHit; // to play on regrab. doesn't really make sense to make a separate sound controller just for this

    bool onBar; // tells whether noomi is currently swinging, used for regrabs and setting body positions

    bool distanceThreshold; // used so that noomi doesn't grab the bar again as soon as you let go

    GameObject lastBarGrabbed; // last bar noomi was on, used for regrabs

    public bool letGoButton, tuckButton, archButton; // used to get button input

    public static PlayerController instance; // also for button input

    bool justLetGo; // used to enable collisions between arms and bar at the right time

    bool collisionThreshold; // used to enable collisions between arms and bar once noomi is far enough away

    public Text regrabCounter; // to show regrab count

    // declare joints and springs, set some physics related variables
    void initJoints()
    {
        // declare joints
        leftBarJoint = leftArm.GetComponent<HingeJoint>();
        rightBarJoint = rightArm.GetComponent<HingeJoint>();
        leftShoulder = torso.GetComponents<HingeJoint>()[0];
        rightShoulder = torso.GetComponents<HingeJoint>()[1];
        neck = head.GetComponent<HingeJoint>();
        leftHip = leftFemur.GetComponent<HingeJoint>();
        rightHip = rightFemur.GetComponent<HingeJoint>();
        leftKnee = leftLowerLeg.GetComponent<HingeJoint>();
        rightKnee = rightLowerLeg.GetComponent<HingeJoint>();
        leftAnkle = leftFoot.GetComponent<HingeJoint>();
        rightAnkle = rightFoot.GetComponent<HingeJoint>();

        // declare springs
        leftShoulderSpring = leftShoulder.spring;
        rightShoulderSpring = rightShoulder.spring;
        neckSpring = neck.spring;
        leftHipSpring = leftHip.spring;
        rightHipSpring = rightHip.spring;
        leftKneeSpring = leftKnee.spring;
        rightKneeSpring = rightKnee.spring;
        leftAnkleSpring = leftAnkle.spring;
        rightAnkleSpring = rightAnkle.spring;

        // set damper because i don't plan on changing it dynamically
        leftShoulderSpring.damper = damp;
        rightShoulderSpring.damper = damp;
        neckSpring.damper = damp;
        leftHipSpring.damper = damp;
        rightHipSpring.damper = damp;
        leftKneeSpring.damper = damp;
        rightKneeSpring.damper = damp;
        leftAnkleSpring.damper = damp;
        rightAnkleSpring.damper = damp;

        // make physics engine ignore collisions between arms and legs, sometimes they get stuck on each other when they shouldn't
        Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), leftFemur.GetComponent<Collider>());
        Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), leftLowerLeg.GetComponent<Collider>());
        Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), rightFemur.GetComponent<Collider>());
        Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), rightLowerLeg.GetComponent<Collider>());

        // and make it ignore collisions between arms and first bar
        Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), bar1.GetComponent<Collider>());
        Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), bar1.GetComponent<Collider>());
    }

    // Start is called before the first frame update
    void Start()
    {
        // for buttons
        if (instance)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        initJoints();

        // set variables to do regrabs / set body position correctly
        onBar = true;
        collisionThreshold = false;
        distanceThreshold = false;
        lastBarGrabbed = bar1;

        // set player prefs for regrab counter
        // need to set text at the start even if it has already been set, otherwise will say 0 and then jump to the right number on the next regrab
        if (!PlayerPrefs.HasKey("regrabs")) // if it has not been set yet, set to zero (don't need to set text as that has already been set in the editor)
        {
            PlayerPrefs.SetInt("regrabs", 0);
        } else if (PlayerPrefs.GetInt("regrabs") == 1) // if there is only 1 regrab, use correct grammar lol
        {
            regrabCounter.text = "1 regrab"; 
        } else // otherwise just set to the number plus the word
        {
            regrabCounter.text = PlayerPrefs.GetInt("regrabs").ToString() + " regrabs";
        }
    }

    // FixedUpdate is called once every physics frame
    void FixedUpdate()
    {
        checkCollisions();
        checkRegrabs();
        checkInputs();
    }

    // makes sure that collisions between arms and bar are enabled...
    // once the player is far enough from the bar that it won't affect their trajectory
    void checkCollisions()
    {
        if (justLetGo) // if the player has let go but collisions haven't been enabled yet
        {
            if (collisionThreshold) // if noomi is far enough from the bar
            {
                // enable collisions
                Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), lastBarGrabbed.GetComponent<Collider>(), false);
                Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), lastBarGrabbed.GetComponent<Collider>(), false);

                justLetGo = false; // method will not be called again until noomi lets go again and collisions need to be enabled
            }
        }
    }

    // checks for player input, sets body position based on that
    void checkInputs()
    {
        // logic to determine which body position / state noomi should be in
        if ((Input.GetKey(KeyCode.UpArrow) || letGoButton))
        {
            // let go
            Destroy(leftBarJoint);
            Destroy(rightBarJoint);
            onBar = false;
            justLetGo = true;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || archButton)
        {
            // arch
            if (onBar)
            {
                // normal strength
                setBodyPosition(-15, 1, 30, 2, -15, 1, -40, 2, -30, 2);
            }
            else
            {
                // extra strength so he can jump while on the ground
                setBodyPosition(-15, 1, 30, 2, -15, 3, -40, 2, -30, 2);
            }
        }
        else if (Input.GetKey(KeyCode.Space) || tuckButton)
        {
            // tuck
            if (onBar)
            {
                // normal strength
                setBodyPosition(150, 1, -80, 2, 150, 1, -120, 2, 30, 2);
            }
            else
            {
                // extra strength so he can actually pull it in the air
                setBodyPosition(150, 2, -80, 2, 150, 2, -120, 2, 30, 2);
            }

        }
        else
        {
            // default
            if (onBar || !distanceThreshold) // if close to the bar, hold the pike (helpful for some moves)
            {
                // pull up pike thing
                setBodyPosition(150, 1, 0, 2, 120, 1, 0, 2, 30, 2);
            }
            else
            {
                // landing
                setBodyPosition(90, 1, 0, 2, 60, 1, -90, 2, 30, 2);
            }
        }
    }

    // check if any bars are close enough to regrab (and that the player left the last bar), if so call regrab function
    void checkRegrabs()
    {
        float regrabThreshold = 0.375f; // distance at which the regrab function is called

        foreach (GameObject bar in GameObject.FindGameObjectsWithTag("bar"))
        {
            float leftHandDistance = Vector3.Distance(leftHand.position, bar.transform.position);
            float rightHandDistance = Vector3.Distance(leftHand.position, bar.transform.position);
            if (bar == lastBarGrabbed)
            {
                // must use distance threshold so noomi doesn't grab again when you try to let go
                if (leftHandDistance > 1 && rightHandDistance > 1)
                {
                    distanceThreshold = true;
                }
                if (leftHandDistance > 0.5 && rightHandDistance > 0.5)
                {
                    collisionThreshold = true;
                }
                if (distanceThreshold && leftHandDistance < regrabThreshold && rightHandDistance < regrabThreshold)
                {
                    regrab(bar);
                }
            }
            else
            {
                // don't need distance threshold
                if (leftHandDistance < regrabThreshold && rightHandDistance < regrabThreshold)
                {
                    regrab(bar);
                }
            }
        }
    }

    // creates the appearance of regrabbing by creating a joint near the bar and moving it into the middle 
    void regrab(GameObject bar)
    {
        // ignore physics collisions between arms and bar
        Physics.IgnoreCollision(bar.GetComponent<Collider>(), leftArm.GetComponent<Collider>(), true);
        Physics.IgnoreCollision(bar.GetComponent<Collider>(), rightArm.GetComponent<Collider>(), true);
        // create joints between arms and bar
        leftBarJoint = leftArm.AddComponent<HingeJoint>();
        leftBarJoint.anchor = new Vector3(0, 1, 0);
        leftBarJoint.axis = new Vector3(0, 0, 1);
        leftBarJoint.autoConfigureConnectedAnchor = false;
        rightBarJoint = rightArm.AddComponent<HingeJoint>();
        rightBarJoint.anchor = new Vector3(0, 1, 0);
        rightBarJoint.axis = new Vector3(0, 0, 1);
        rightBarJoint.autoConfigureConnectedAnchor = false;
        // move joints to proper position
        leftBarJoint.connectedAnchor = new Vector3(bar.transform.position.x, bar.transform.position.y, 0.3f);
        rightBarJoint.connectedAnchor = new Vector3(bar.transform.position.x, bar.transform.position.y, -0.3f);
        // reset variables
        lastBarGrabbed = bar;
        distanceThreshold = false;
        collisionThreshold = false;
        onBar = true;
        // play sound
        metalHit.Play();
        // update regrab counter on screen
        PlayerPrefs.SetInt("regrabs", PlayerPrefs.GetInt("regrabs") + 1); // increment playerprefs
        if (PlayerPrefs.GetInt("regrabs") == 1) // if it's 1, make the grammar correct again
        {
            regrabCounter.text = "1 regrab";
        } else { // otherwise set text to the number plus the word, again
            regrabCounter.text = PlayerPrefs.GetInt("regrabs").ToString() + " regrabs";
        }
    }

    // set the angle of the player joints to match a desired body position
    void setBodyPosition(float shoulderAngle, float shoulderStrength, float neckAngle, float neckStrength, float hipAngle, float hipStrength, float kneeAngle, float kneeStrength, float ankleAngle, float ankleStrength)
    {
        // for each joint, set target position, strength, and then reset the spring with the new attributes
        leftShoulderSpring.targetPosition = shoulderAngle;
        leftShoulderSpring.spring = strength * shoulderStrength;
        leftShoulder.spring = leftShoulderSpring;
        rightShoulderSpring.targetPosition = shoulderAngle;
        rightShoulderSpring.spring = strength * shoulderStrength;
        rightShoulder.spring = rightShoulderSpring;

        neckSpring.targetPosition = neckAngle;
        neckSpring.spring = strength * neckStrength;
        neck.spring = neckSpring;

        leftHipSpring.targetPosition = hipAngle;
        leftHipSpring.spring = strength * hipStrength;
        leftHip.spring = leftHipSpring;
        rightHipSpring.targetPosition = hipAngle;
        rightHipSpring.spring = strength * hipStrength;
        rightHip.spring = rightHipSpring;

        leftKneeSpring.targetPosition = kneeAngle;
        leftKneeSpring.spring = strength * kneeStrength;
        leftKnee.spring = leftKneeSpring;
        rightKneeSpring.targetPosition = kneeAngle;
        rightKneeSpring.spring = strength * kneeStrength;
        rightKnee.spring = rightKneeSpring;

        leftAnkleSpring.targetPosition = ankleAngle;
        leftAnkleSpring.spring = strength * ankleStrength;
        leftAnkle.spring = leftAnkleSpring;
        rightAnkleSpring.targetPosition = ankleAngle;
        rightAnkleSpring.spring = strength * ankleStrength;
        rightAnkle.spring = rightAnkleSpring;
    }
}

