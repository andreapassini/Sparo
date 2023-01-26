//-------------------------------
//--- Prototype FPC
//--- Version 1.0
//--- © The Famous Mouse™
//-------------------------------

using UnityEngine;
using PrototypeFPC;

namespace PrototypeFPC
{
    public class Jump : MonoBehaviour
    {
        //Dependencies
        [Header("Dependencies")]
        [SerializeField] Dependencies dependencies;

        //Input
        [Header("Input Properties")]
        [SerializeField] KeyCode jumpKey = KeyCode.Space;

        //Jump properties
        [Header("Jumping Properties")]
        [SerializeField] float amount = 14f;
        [SerializeField] float coolDownRate = 15f;

        //Landing properties
        [Header("Landing Properties")]
        [SerializeField] float distanceBeforeForce = 25f;
        [SerializeField] float rateBeforeForce = -15f;
        [SerializeField] float hardLandForce = 0.25f;

        //Audio properties
        [Header("Audio Properties")]
        [SerializeField] AudioClip jumpSound;
        [SerializeField] AudioClip landSound;

        //Helpers
        float nextTimeToJump = 0f;
        bool landed = true;
        
        Vector3 newFallVelocity;
        Rigidbody rb;
        AudioSource audioSource;
        RaycastHit falltHit;


        //-----------------------


        //Functions
        ///////////////

        void Start()
        {
            Setup(); //- Line 74
        }

        void Update()
        {
            Land(); //- Line 117
        }

        void FixedUpdate()
        {
            SimulateJump(); //- Line 82
            Fall(); //- Line 102
        }


        //-----------------------


        void Setup()
        {
             //Setup dependencies
            rb = dependencies.rb;
            audioSource = dependencies.audioSourceBottom;
        }

        //Initiate jump
        void SimulateJump()
        {
            if(Input.GetKey(jumpKey) && dependencies.isGrounded && !dependencies.isWallRunning && !dependencies.isVaulting && !dependencies.isInspecting && Time.time >= nextTimeToJump)
            {
                //Jump cooldown rate
                nextTimeToJump = Time.time + 1f / coolDownRate;

                //Apply force if grounded
                if(dependencies.isGrounded)
                {
                    //Apply upward force
                    rb.AddForce(Vector3.up * amount - Vector3.up * rb.velocity.y, ForceMode.VelocityChange);

                    //Audio
                    audioSource.PlayOneShot(jumpSound);
                }
            }
        }


        void Fall()
        {
            if(!dependencies.isGrounded)
            {
                //Check fall rate
                if(rb.velocity.y < rateBeforeForce && Physics.Raycast(rb.transform.position, Vector3.down, out falltHit, distanceBeforeForce))
                {
                    //Apply additional force towards ground if falling faster the fall rate
                    newFallVelocity = rb.velocity;
                    rb.velocity = new Vector3(newFallVelocity.x, newFallVelocity.y += (-1 * hardLandForce), newFallVelocity.z);
                }
            }
        }


        void Land()
        {
            //Toggle landing upon ground detection
            if(!dependencies.isGrounded && landed)
            {
                landed = false;
            }

            if(dependencies.isGrounded && !landed)
            {
                landed = true;

                if(!dependencies.isVaulting)
                {
                    //Audio
                    audioSource.PlayOneShot(landSound);
                }
            }
         }
    }
}
