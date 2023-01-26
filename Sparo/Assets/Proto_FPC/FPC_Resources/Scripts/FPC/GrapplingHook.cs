//-------------------------------
//--- Prototype FPC
//--- Version 1.0
//--- © The Famous Mouse™
//-------------------------------

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PrototypeFPC;

namespace PrototypeFPC
{
    public class GrapplingHook : MonoBehaviour
    {
        //Dependencies
        [Header("Dependencies")]
        [SerializeField] Dependencies dependencies;
        
        //Hook properties
        [Header("Hook Properties")]
        [SerializeField] GameObject hookModel;
        [SerializeField] float holdDelayToSwing = 0.2f;
        [SerializeField] float playerRetractStrength = 1000f;
        [SerializeField] float retractStrength = 500f; 
        [SerializeField] float latchOnImpulse = 200f;
        
        //Rope properties
        [Header("Rope Properties")]
        [SerializeField] Material ropeMaterial;
        [SerializeField] float startThickness = 0.02f;
        [SerializeField] float endThickness = 0.06f;

        //Balloon properties
        [Header("Balloon Properties")]
        [SerializeField] GameObject balloonModel;
        [SerializeField] float riseForce = 1500f;
        [SerializeField] float size = 2f;
        [SerializeField] float inflateSpeed = 2f;
        [SerializeField] float deflateSpeed = 2.5f;

        //Rope visual spring properties
        [Header("Rope Visual Spring Properties")]
        [SerializeField] int segments = 50;
        [SerializeField] float damper = 12;
        [SerializeField] float springStrength = 800;
        [SerializeField] float speed = 12;
        [SerializeField] float waveCount = 5;
        [SerializeField] float waveHeight = 4;
        [SerializeField] AnimationCurve affectCurve;

        //Audio properties
        [Header("Audio Properties")]
        [SerializeField] AudioClip grapplingSound;
        [SerializeField] AudioClip releaseSound;
        [SerializeField] AudioClip retractSound;
        [SerializeField] AudioClip balloonInflateSound;
        [SerializeField] AudioClip balloonDeflateSound;

        //Lists to store data
        [HideInInspector]
        [SerializeField] List<GameObject> hooks, 
        hookModels, hookLatches, 
        ropeColliders, balloons;
        [HideInInspector]
        [SerializeField] List<LineRenderer> ropes;

        //Helpers
        float mouseDownTimer = 0;

        bool  executeHookSwing = false;
        bool hookRelease = false;
        bool hooked = false;
        bool holdingBalloon = false;
        bool isOptimizing = false;

        Rigidbody player;
        Transform spawnPoint;
        Spring spring;

        Ray ray;
        RaycastHit hit;

        AudioSource audioSource;

        
        //-----------------------


        //Functions
        ///////////////

        void Start()
        {
            Setup(); //- Line 122
            CreateSpring(); //- Line 132
        }

        void Update()
        {
            InputCheck(); //- Line 141
            CreateHooks(); //- Line 172
            CreateBalloons(); //- Line 421
            RetractHooks(); //- Line 590
            CutRopes(); //- Line 625
        }

        void FixedUpdate()
        {
            InflateDeflateBalloons(); //- Line 537
        }

        void LateUpdate()
        {
            DrawRopes(); //- Line 819
        }
        

        //-----------------------


        void Setup()
        {
            //Setup dependencies
            player = dependencies.rb;
            spawnPoint = dependencies.spawnPoint;
            audioSource = dependencies.audioSourceTop;
        }


        //Create spring
        void CreateSpring()
        {
            //Create and set rope visual spring value
            spring = new Spring();
            spring.SetTarget(0);
        }


        //Input
        void InputCheck()
        {
                //Reset checker
                if(Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftControl) && !dependencies.isInspecting)
                {
                    mouseDownTimer = 0;
                    hookRelease = false;
                    executeHookSwing = false;
                }

                //Check input for hook to swing
                if(Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftControl) && !dependencies.isInspecting)
                {
                    mouseDownTimer += Time.deltaTime;

                    if(hooked && mouseDownTimer >= holdDelayToSwing && !executeHookSwing)
                    {
                        executeHookSwing = true;
                    }
                }

                //Check input for hook to latch
                if(Input.GetMouseButtonUp(1) && !Input.GetKey(KeyCode.LeftControl) && mouseDownTimer >= holdDelayToSwing && executeHookSwing && !dependencies.isInspecting)
                {
                    executeHookSwing = false;
                    hookRelease = true;
                }
        }


        //Create Hooks
        void CreateHooks()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftControl) && !dependencies.isInspecting)
            {
                //Check and set target rigidbody if none
                if(Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Ignore Raycast")), QueryTriggerInteraction.Ignore)) 
                {
                    if(hit.transform.gameObject.GetComponent<Rigidbody>() == null)
                    {
                        //Add and set to kinematic/Static if not manually created/specified
                        hit.transform.gameObject.AddComponent<Rigidbody>().isKinematic = true;
                    }
                }

                //Create first hook
                if(!hooked)
                {
                    if(Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Ignore Raycast")), QueryTriggerInteraction.Ignore)) 
                    {
                        if(!hit.collider.isTrigger && hit.collider.gameObject.GetComponent<Rigidbody>() != player)
                        {
                            hooked = true;
                            
                            //Create new hook object
                            hooks.Add(new GameObject("Hook"));
                            hooks[hooks.Count - 1].transform.position = hit.point;

                            //Hook end point model
                            hookModels.Add(Instantiate(hookModel, hooks[hooks.Count - 1].transform.position, Quaternion.identity));
                            hookModels[hookModels.Count - 1].transform.parent = hooks[hooks.Count - 1].transform;

                            //Hook start point model
                            hookModels.Add(Instantiate(hookModel, spawnPoint.position, Quaternion.identity));
                            hookModels[hookModels.Count - 1].transform.parent = spawnPoint.transform;
                            
                            //Set hook rope values
                            ropes.Add(hooks[hooks.Count - 1].AddComponent<LineRenderer>());
                            ropes[ropes.Count - 1].material = ropeMaterial;
                            ropes[ropes.Count - 1].startWidth = startThickness;
                            ropes[ropes.Count - 1].endWidth = endThickness;
                            ropes[ropes.Count - 1].numCornerVertices = 2;
                            ropes[ropes.Count - 1].numCapVertices = 10;
                            ropes[ropes.Count - 1].textureMode = LineTextureMode.Tile;
                            ropes[ropes.Count - 1].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                            ropes[ropes.Count - 1].receiveShadows = false;
                            
                            //Add and set joint parameters
                            spring.Reset();
                            spring.SetVelocity(speed);
                            ropes[hooks.Count - 1].positionCount = segments + 1;

                            //Check if collider hit is balloon
                            if(balloons.Count != 0)
                            {
                                for(var i = balloons.Count - 1; i >= 0; i--)
                                {
                                    //If hook already contain rigidbody
                                    if(hit.transform.gameObject.GetComponent<Collider>() == balloons[i].GetComponent<Collider>() && hooks[hooks.Count - 1].GetComponent<Rigidbody>())
                                    {
                                        balloons[i].AddComponent<FixedJoint>().connectedBody = hooks[hooks.Count - 1].GetComponent<Rigidbody>();
                                    }

                                    //If hook do not contain rigidbody
                                    else if(hit.transform.gameObject.GetComponent<Collider>() == balloons[i].GetComponent<Collider>() && !hooks[hooks.Count - 1].GetComponent<Rigidbody>())
                                    {
                                        balloons[i].AddComponent<FixedJoint>().connectedBody = hooks[hooks.Count - 1].AddComponent<Rigidbody>();
                                    }

                                    //If raycast did not hit balloon collider
                                    else
                                    {
                                        hooks[hooks.Count - 1].AddComponent<FixedJoint>().connectedBody = hit.transform.gameObject.GetComponent<Rigidbody>();
                                    }
                                }
                            }

                            //If no balloons then proceed
                            else
                            {
                                hooks[hooks.Count - 1].AddComponent<FixedJoint>().connectedBody = hit.transform.gameObject.GetComponent<Rigidbody>();
                            }

                            player.gameObject.AddComponent<SpringJoint>().connectedBody = hooks[hooks.Count - 1].GetComponent<Rigidbody>();
                            player.GetComponent<SpringJoint>().autoConfigureConnectedAnchor = false;
                            player.GetComponent<SpringJoint>().connectedAnchor = Vector3.zero;

                            //The distance to keep from hook point for player swing
                            float distanceFromHook = Vector3.Distance(player.gameObject.transform.position, hooks[hooks.Count - 1].transform.position);
                            player.GetComponent<SpringJoint>().maxDistance = distanceFromHook * 0.85f;
                            player.GetComponent<SpringJoint>().minDistance = distanceFromHook * 0.2f;
                            player.GetComponent<SpringJoint>().spring = 0;

                            //Add collider for rope cutting
                            ropeColliders.Add(new GameObject("RopeCollider"));
                            ropeColliders[ropeColliders.Count - 1].transform.parent = hooks[hooks.Count - 1].transform;
                            ropeColliders[ropeColliders.Count - 1].AddComponent<BoxCollider>().size = new Vector3(0.1f, 0, 0.1f);
                            ropeColliders[ropeColliders.Count - 1].GetComponent<BoxCollider>().isTrigger = true;
                            ropeColliders[ropeColliders.Count - 1].GetComponent<BoxCollider>().enabled = false;

                            //Knock back when hooked
                            hooks[hooks.Count - 1].GetComponent<Rigidbody>().AddForce(ray.direction * latchOnImpulse * 0.2f, ForceMode.Impulse);

                            //Set previous rope quality to 2 if not already
                            if(ropes.Count > 1)
                            {
                                    if(ropes[ropes.Count - 2].positionCount > 2)
                                    {
                                        ropes[ropes.Count - 2].positionCount = 2;
                                    }
                            }

                            //Audio
                            audioSource.PlayOneShot(grapplingSound);
                        }
                    }
                }
            
                //Create hook latch
                else if(hooked)
                {
                    //Create latch if not currently holding A balloon
                    if(!holdingBalloon)
                    {
                        if(Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Ignore Raycast")), QueryTriggerInteraction.Ignore)) 
                        {
                            if(!hit.collider.isTrigger && hit.collider.gameObject.GetComponent<Rigidbody>() != player)
                            {
                                //Create new hook latch object
                                hookLatches.Add(new GameObject("HookLatch"));
                                hookLatches[hookLatches.Count - 1].transform.position = hit.point;

                                //Remove hook start point model
                                Destroy(hookModels[hookModels.Count - 1].gameObject);
                                hookModels.RemoveAt(hookModels.Count - 1);

                                //Add hook latch point model
                                hookModels.Add(Instantiate(hookModel, hookLatches[hooks.Count - 1].transform.position, Quaternion.identity));
                                hookModels[hookModels.Count - 1].transform.parent = hookLatches[hooks.Count - 1].transform;
                                    
                                //Add and set joint parameters
                                spring.Reset();
                                spring.SetVelocity(speed);

                                //Check if collider hit is balloon
                                if(balloons.Count != 0)
                                {
                                    for(var i = balloons.Count - 1; i >= 0; i--)
                                    {
                                        //If latch already contain rigidbody
                                        if(hit.transform.gameObject.GetComponent<Collider>() == balloons[i].GetComponent<Collider>() && hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>())
                                        {
                                            balloons[i].AddComponent<FixedJoint>().connectedBody = hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>();
                                        }

                                        //If latch does not contain rigidbody
                                        else if(hit.transform.gameObject.GetComponent<Collider>() == balloons[i].GetComponent<Collider>() && !hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>())
                                        {
                                            balloons[i].AddComponent<FixedJoint>().connectedBody = hookLatches[hookLatches.Count - 1].AddComponent<Rigidbody>();
                                        }

                                        //If raycast did not hit balloon collider
                                        else
                                        {
                                            hookLatches[hookLatches.Count - 1].AddComponent<FixedJoint>().connectedBody = hit.transform.gameObject.GetComponent<Rigidbody>();
                                        }
                                    }
                                }

                                //If no balloons then proceed
                                else
                                {
                                    hookLatches[hookLatches.Count - 1].AddComponent<FixedJoint>().connectedBody = hit.transform.gameObject.GetComponent<Rigidbody>();
                                }

                                Destroy(player.GetComponent<SpringJoint>());
                                hooks[hooks.Count - 1].AddComponent<SpringJoint>().connectedBody = hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>();
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().autoConfigureConnectedAnchor = false;
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().anchor = Vector3.zero;
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().connectedAnchor = Vector3.zero;
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().spring = 0;
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().damper = 0.2f;
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().maxDistance = 0;
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().minDistance = 0;

                                //Knock back when hooked
                                hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>().AddForce(ray.direction * latchOnImpulse * 0.2f, ForceMode.Impulse);

                                //Set rope width
                                ropes[ropes.Count - 1].startWidth = endThickness;
                                ropes[ropes.Count - 1].endWidth = endThickness;

                                //Enable rope collider
                                ropeColliders[ropeColliders.Count - 1].GetComponent<BoxCollider>().enabled = true;

                                isOptimizing = true;
                                    
                                hooked = false;

                                //Audio
                                audioSource.PlayOneShot(grapplingSound);
                            }
                        }
                    }

                    //Recalculate new hook point if already holding balloon
                    else if(holdingBalloon)
                    {
                        if(Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Ignore Raycast")), QueryTriggerInteraction.Ignore)) 
                        {
                            if(!hit.collider.isTrigger && hit.collider.gameObject.GetComponent<Rigidbody>() != player)
                            {
                                //Reposition hook
                                hooks[hooks.Count - 1].transform.position = hit.point;

                                hookModels[hookModels.Count - 1].transform.position= hit.point;
                                hookModels[hookModels.Count - 1].transform.parent = hooks[hooks.Count - 1].transform;

                                //Reassign hook properties
                                spring.Reset();
                                spring.SetVelocity(speed);
                                hooks[hooks.Count - 1].GetComponent<FixedJoint>().connectedBody = hit.transform.gameObject.GetComponent<Rigidbody>();
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().autoConfigureConnectedAnchor = false;
                                hooks[hooks.Count - 1].GetComponent<SpringJoint>().connectedAnchor = Vector3.zero;

                                //Set rope width
                                ropes[ropes.Count - 1].startWidth = endThickness;
                                ropes[ropes.Count - 1].endWidth = endThickness;

                                //Knock back when hooked
                                hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>().AddForce(ray.direction * latchOnImpulse * 0.2f, ForceMode.Impulse);

                                isOptimizing = true;
                                    
                                hooked = false;
                                holdingBalloon = false;

                                //Audio
                                audioSource.PlayOneShot(grapplingSound);
                            }
                        }
                    }
                }
            }
        }
        

        //Create balloons
        void CreateBalloons()
        {
             if(Input.GetKeyDown("b") && !dependencies.isInspecting)
            {
                if(hooked && !holdingBalloon)
                {
                    //Create new hook latch and balloon object
                    hookLatches.Add(new GameObject("HookLatch"));
                    balloons.Add(Instantiate(balloonModel, hookLatches[hooks.Count - 1].transform.position, Quaternion.identity));
                    balloons[balloons.Count - 1].AddComponent<Rigidbody>().mass = 5f;
                    balloons[balloons.Count - 1].GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                    balloons[balloons.Count - 1].transform.localScale = Vector3.zero;
                    balloons[balloons.Count - 1].transform.position = spawnPoint.position;
                    hookLatches[hookLatches.Count - 1].transform.position = balloons[balloons.Count - 1].transform.position;

                    //Remove hook start point model
                    Destroy(hookModels[hookModels.Count - 1].gameObject);
                    hookModels.RemoveAt(hookModels.Count - 1);
                            
                    //Add and set joint parameters
                    spring.Reset();
                    spring.SetVelocity(speed);
                    balloons[balloons.Count - 1].AddComponent<FixedJoint>().connectedBody = hookLatches[hookLatches.Count - 1].AddComponent<Rigidbody>();
                    Destroy(player.GetComponent<SpringJoint>());
                    hooks[hooks.Count - 1].AddComponent<SpringJoint>().connectedBody = hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>();
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().autoConfigureConnectedAnchor = false;
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().anchor = Vector3.zero;
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().connectedAnchor = Vector3.zero;
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().spring = retractStrength;
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().damper = 50;

                    //Set rope width
                    ropes[ropes.Count - 1].startWidth = endThickness;
                    ropes[ropes.Count - 1].endWidth = endThickness;

                    //Enable rope collider
                    ropeColliders[ropeColliders.Count - 1].GetComponent<BoxCollider>().enabled = true;
                        
                    hooked = false;
                    executeHookSwing = false;
                    hookRelease = false;
                    mouseDownTimer = 0;

                    //Audio
                    balloons[balloons.Count - 1].GetComponent<AudioSource>().PlayOneShot(balloonInflateSound);
                }

                else if(!hooked && !holdingBalloon)
                {
                    hooked = true;
                    holdingBalloon = true;

                    //Create new hook object
                    hooks.Add(new GameObject("Hook"));
                    hooks[hooks.Count - 1].transform.position = spawnPoint.position;

                    //Hook start point model
                    hookModels.Add(Instantiate(hookModel, spawnPoint.position, Quaternion.identity));
                    hookModels[hookModels.Count - 1].transform.parent = spawnPoint.transform;

                    //Create new hook latch and balloon object
                    hookLatches.Add(new GameObject("HookLatch"));
                    balloons.Add(Instantiate(balloonModel, hookLatches[hooks.Count - 1].transform.position, Quaternion.identity));
                    balloons[balloons.Count - 1].AddComponent<Rigidbody>().mass = 5f;
                    balloons[balloons.Count - 1].GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                    balloons[balloons.Count - 1].transform.localScale = Vector3.zero;
                    balloons[balloons.Count - 1].transform.position = spawnPoint.position;
                    hookLatches[hookLatches.Count - 1].transform.position = balloons[balloons.Count - 1].transform.position;
                            
                    //Set hook rope values
                    ropes.Add(hooks[hooks.Count - 1].AddComponent<LineRenderer>());
                    ropes[ropes.Count - 1].material = ropeMaterial;
                    ropes[ropes.Count - 1].startWidth = startThickness;
                    ropes[ropes.Count - 1].endWidth = endThickness;
                    ropes[ropes.Count - 1].numCornerVertices = 2;
                    ropes[ropes.Count - 1].numCapVertices = 10;
                    ropes[ropes.Count - 1].textureMode = LineTextureMode.Tile;
                    ropes[ropes.Count - 1].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    ropes[ropes.Count - 1].receiveShadows = false;
                            
                    //Add and set joint parameters
                    spring.Reset();
                    spring.SetVelocity(5);
                    ropes[hooks.Count - 1].positionCount = segments + 1;
                    hooks[hooks.Count - 1].AddComponent<FixedJoint>().connectedBody = player;
                    balloons[balloons.Count - 1].AddComponent<FixedJoint>().connectedBody = hookLatches[hookLatches.Count - 1].AddComponent<Rigidbody>();
                    hooks[hooks.Count - 1].AddComponent<SpringJoint>().connectedBody = hookLatches[hookLatches.Count - 1].GetComponent<Rigidbody>();
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().anchor = Vector3.zero;
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().connectedAnchor = Vector3.zero;
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().damper = 50f;
                    hooks[hooks.Count - 1].GetComponent<SpringJoint>().spring = retractStrength;

                    //Add collider for rope cutting
                    ropeColliders.Add(new GameObject("RopeCollider"));
                    ropeColliders[ropeColliders.Count - 1].transform.parent = hooks[hooks.Count - 1].transform;
                    ropeColliders[ropeColliders.Count - 1].AddComponent<BoxCollider>().size = new Vector3(0.1f, 0, 0.1f);
                    ropeColliders[ropeColliders.Count - 1].GetComponent<BoxCollider>().isTrigger = true;
                    ropeColliders[ropeColliders.Count - 1].GetComponent<BoxCollider>().enabled = true;

                    //Set previous rope quality to 2 if not already
                    if(ropes.Count > 1)
                    {
                        if(ropes[ropes.Count - 2].positionCount > 2)
                        {
                                ropes[ropes.Count - 2].positionCount = 2;
                        }
                    }

                    //Audio
                    balloons[balloons.Count - 1].GetComponent<AudioSource>().PlayOneShot(balloonInflateSound);
                }
            }
        }


        //Inflate/Deflate and rise balloons
        void InflateDeflateBalloons()
        {
            if(balloons.Count != 0)
            {
                foreach(GameObject balloon in balloons) 
                {
                    if(balloon.GetComponent<FixedJoint>())
                    {
                        //Set scale, speed and add force
                        float rise = 0;
                        balloon.transform.localScale = Vector3.Lerp(balloon.transform.localScale, new Vector3(size, size, size), inflateSpeed * Time.deltaTime);
                        rise = Mathf.Lerp(rise, riseForce, inflateSpeed);

                        //Rise force
                        balloon.GetComponent<Rigidbody>().AddForce(Vector3.up * rise);

                        if(balloon.GetComponent<FixedJoint>().connectedBody == null)
                        {
                            Destroy(balloon.GetComponent<FixedJoint>());

                            //Audio
                            balloon.GetComponent<AudioSource>().PlayOneShot(balloonDeflateSound);
                        }
                    }

                    else
                    {
                        //Set scale, speed and add force
                        float rise = 0;
                        balloon.transform.localScale = Vector3.Lerp(balloon.transform.localScale, Vector3.zero, deflateSpeed * Time.deltaTime);
                        rise = Mathf.Lerp(rise, riseForce / 2, deflateSpeed);

                        //Rise force
                        balloon.GetComponent<Rigidbody>().AddForce(Vector3.up * rise);

                        //Destroy balloon when deflated
                        if(balloon.transform.localScale.magnitude < 0.1f)
                        {
                            Destroy(balloon.gameObject);

                            if(balloons.Contains(balloon))
                            {
                                balloons.Remove(balloon);
                                return;
                            }
                        }
                    }
                }
            }
        }


        //Retract hooked objects
        void RetractHooks()
        {
            //Set player hook swing strength
            if(executeHookSwing &&player.GetComponent<SpringJoint>() && player.GetComponent<SpringJoint>().spring != playerRetractStrength)
            {
                player.GetComponent<SpringJoint>().spring = playerRetractStrength;
            }

            //Set player hook retract strength
            if(Input.GetMouseButtonDown(2) && !dependencies.isInspecting)
            {
                if(player.GetComponent<SpringJoint>() != null)
                {
                    player.GetComponent<SpringJoint>().spring = playerRetractStrength;
                }

                //Set all other hook and latched retract strengths
                foreach(GameObject hookJoints in hooks) 
                {
                    if(hookJoints.GetComponent<SpringJoint>() && hookJoints.GetComponent<SpringJoint>().connectedBody != player)
                    {
                        hookJoints.GetComponent<SpringJoint>().spring = retractStrength;
                    }
                }

                if(hooks.Count > 0)
                {
                    //Audio
                    audioSource.PlayOneShot(retractSound);
                }
            }
        }


        //Cut Ropes
        void CutRopes()
        {
            //Destroy player hooks upon hold release
            if(hookRelease && hooked)
            {
                hookRelease = false;

                if(hooks.Count > 0)
                {
                    Destroy(player.GetComponent<SpringJoint>());
                    Destroy(hooks[hooks.Count - 1].gameObject);
                    hooks.RemoveAt(hooks.Count - 1);
                }

                if(hookLatches.Count > 0 && holdingBalloon)
                {
                    Destroy(hookLatches[hookLatches.Count - 1].gameObject);
                    hookLatches.RemoveAt(hookLatches.Count - 1);
                    holdingBalloon = false;
                }

                if(ropeColliders.Count > 0)
                {
                    Destroy(ropeColliders[ropeColliders.Count - 1].gameObject);
                    ropeColliders.RemoveAt(ropeColliders.Count - 1);
                }

                if(hookModels.Count > 0)
                {
                    Destroy(hookModels[hookModels.Count - 1].gameObject);
                    hookModels.RemoveAt(hookModels.Count - 1);
                }

                if(ropes.Count > 0)
                {
                    ropes.RemoveAt(ropes.Count - 1);
                }
                        
                hooked = false;
                hookRelease = false;

                //Audio
                audioSource.PlayOneShot(releaseSound);
            }

            //Remove specific hooks
            if(Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftControl) && !dependencies.isInspecting)
            {
                //If attached to player
                if(hooked)
                {
                    if(hooks.Count > 0)
                    {
                        Destroy(player.GetComponent<SpringJoint>());
                        Destroy(hooks[hooks.Count - 1].gameObject);
                        hooks.RemoveAt(hooks.Count - 1);
                    }

                    if(hookLatches.Count > 0 && holdingBalloon)
                    {
                        //Remove balloon first
                        Destroy(balloons[balloons.Count - 1].gameObject);
                        balloons.RemoveAt(balloons.Count - 1);

                        //Remove latch
                        Destroy(hookLatches[hookLatches.Count - 1].gameObject);
                        hookLatches.RemoveAt(hookLatches.Count - 1);

                        holdingBalloon = false;
                    }

                    if(ropeColliders.Count > 0)
                    {
                        Destroy(ropeColliders[ropeColliders.Count - 1].gameObject);
                        ropeColliders.RemoveAt(ropeColliders.Count - 1);
                    }

                    if(hookModels.Count > 0)
                    {
                        Destroy(hookModels[hookModels.Count - 1].gameObject);
                        hookModels.RemoveAt(hookModels.Count - 1);
                    }

                    if(ropes.Count > 0)
                    {
                        ropes.RemoveAt(ropes.Count - 1);
                    }
                        
                    hooked = false;

                    //Audio
                    audioSource.PlayOneShot(releaseSound);
                }

                //At index and are not attached to player
                else if(!hooked && Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Ignore Raycast")))) 
                {
                    if(hit.collider.isTrigger)
                    {
                        int index = GameObjectToIndex(hit.collider.gameObject);
                        
                        Destroy(hooks[index].gameObject);
                        Destroy(hookLatches[index].gameObject);
                        Destroy(ropeColliders[index].gameObject);

                        hooks.RemoveAt(index);
                        hookLatches.RemoveAt(index);
                        ropes.RemoveAt(index);
                        ropeColliders.RemoveAt(index);

                        if(hooks.Count == 0)
                        {
                            hooked = false;
                        }

                        if(holdingBalloon)
                        {
                            holdingBalloon = false;
                        }

                        //Audio
                        audioSource.PlayOneShot(releaseSound);
                    }

                    //Clean up the hook model list if missing after the models get destroyed
                    for(var i = hookModels.Count - 1; i >= 0; i--)
                    {
                        if(hookModels[i] == null)
                        {
                            hookModels.RemoveAt(i);
                        }
                    }
                }
            }

            //Destroy everything created and clear all lists
            if(Input.GetKeyDown("r") && !dependencies.isInspecting)
            {
                hooked = false;

                //Destroy joints and objects
                if(hooks.Count > 0)
                {
                    if(player.GetComponent<SpringJoint>())
                    {
                        Destroy(player.GetComponent<SpringJoint>());
                    }

                    foreach(GameObject hookObjects in hooks)
                    {
                        Destroy(hookObjects);
                    }
                        
                    foreach(GameObject hookLatchObjects in hookLatches)
                    {
                         Destroy(hookLatchObjects);
                    }

                    foreach(GameObject ropeColliderObjects in ropeColliders)
                    {
                         Destroy(ropeColliderObjects);
                    }
                        
                    foreach(GameObject balloonObjects in balloons)
                    {
                        Destroy(balloonObjects);
                    }

                    foreach(GameObject hookModelObjects in hookModels)
                    {
                        Destroy(hookModelObjects);
                    }
                    
                    //Clear all lists
                    hooks.Clear();
                    hookModels.Clear();
                    hookLatches.Clear();
                    ropes.Clear();
                    ropeColliders.Clear();
                    balloons.Clear();

                    if(holdingBalloon)
                    {
                        holdingBalloon = false;
                    }

                    //Audio
                    audioSource.PlayOneShot(releaseSound);
                }
            }
        }


        //Draw ropes
        void DrawRopes()
        {
            if(ropes.Count != 0 && ropes.Count == hooks.Count)
            {
                //Loop and set the respective start and end draw positions of each rope
                for(int i = 0; i < ropes.Count; i++) 
                {
                    //From the player
                    if(player.GetComponent<SpringJoint>() != null && player.GetComponent<SpringJoint>().connectedBody == hooks[i].GetComponent<Rigidbody>() && !holdingBalloon)
                    {
                        //Set spring properties
                        spring.SetDamper(damper);
                        spring.SetStrength(springStrength);
                        spring.Update(Time.deltaTime);

                        var up = Quaternion.LookRotation((hooks[i].transform.position - spawnPoint.position).normalized) * Vector3.up;

                        var currentGrapplePosition = Vector3.zero;
                        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, hooks[i].transform.position, 12f * Time.deltaTime);

                        for (var t = 0; t < segments + 1; t++) 
                        {
                            var delta = t / (float) segments;

                            var right = Quaternion.LookRotation((hooks[i].transform.position - spawnPoint.position).normalized) * Vector3.right;

                            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                                         affectCurve.Evaluate(delta) +
                                         right * waveHeight * Mathf.Cos(delta * waveCount * Mathf.PI) * spring.Value *
                                         affectCurve.Evaluate(delta);
                            
                            if(ropes[i].positionCount > 2)
                            {
                                ropes[i].SetPosition(t, Vector3.Lerp(spawnPoint.position, hooks[i].transform.position, delta) + offset);
                            }
                        }
                    }

                    //From the player and holding balloon
                    if(hooks[i].GetComponent<FixedJoint>() != null && hooks[i].GetComponent<FixedJoint>().connectedBody == player && holdingBalloon)
                    {
                        //Set spring properties
                        spring.SetDamper(damper);
                        spring.SetStrength(springStrength);
                        spring.Update(Time.deltaTime);

                        var up = Quaternion.LookRotation((hookLatches[i].transform.position - spawnPoint.position + new Vector3(0, 1, 0)).normalized) * Vector3.up;

                        var currentGrapplePosition = Vector3.zero;
                        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, hookLatches[i].transform.position, 12f * Time.deltaTime);

                        for (var t = 0; t < segments + 1; t++) 
                        {
                            var delta = t / (float) segments;

                            var right = Quaternion.LookRotation((hookLatches[i].transform.position - spawnPoint.position + new Vector3(0, 1, 0)).normalized) * Vector3.right;

                            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                                         affectCurve.Evaluate(delta) +
                                         right * waveHeight * Mathf.Cos(delta * waveCount * Mathf.PI) * spring.Value *
                                         affectCurve.Evaluate(delta);
                            
                            if(ropes[i].positionCount > 2)
                            {
                                ropes[i].SetPosition(t, Vector3.Lerp(spawnPoint.position, hookLatches[i].transform.position, delta) + offset);
                            }
                        }
                    }
                    
                    //From hook to latch
                    else if(hooks[i].GetComponent<SpringJoint>() != null && hooks[i].GetComponent<SpringJoint>().connectedBody != player && ropes[i].positionCount > 2 && !holdingBalloon)
                    {
                        //Set spring properties
                        spring.SetDamper(damper);
                        spring.SetStrength(springStrength);
                        spring.Update(Time.deltaTime);

                        var up = Quaternion.LookRotation((hooks[i].transform.position - hookLatches[i].transform.position).normalized) * Vector3.up;

                        var currentGrapplePosition = Vector3.zero;
                        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, hooks[i].transform.position, 12f * Time.deltaTime);

                        for (var t = 0; t < segments + 1; t++) 
                        {
                            var delta = t / (float) segments;
                            var right = Quaternion.LookRotation((hooks[i].transform.position - hookLatches[i].transform.position).normalized) * Vector3.right;

                            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                                         affectCurve.Evaluate(delta) +
                                         right * waveHeight * Mathf.Cos(delta * waveCount * Mathf.PI) * spring.Value *
                                         affectCurve.Evaluate(delta);
                            
                            if(ropes[i].positionCount > 2 && i == ropes.Count || i == ropes.Count - 1)
                            {
                                ropes[i].SetPosition(t, Vector3.Lerp(hookLatches[i].transform.position, hooks[i].transform.position, delta) + offset);
                            }
                        }

                        //Set rope segments to 2 (start and end) after spring visuals
                        if(isOptimizing)
                        {
                                StartCoroutine(delay());
                                IEnumerator delay()
                                {
                                    yield return new WaitForSeconds(1);
                                    if(ropes.Count > 1)
                                    {
                                        if(i != ropes.Count)
                                        {
                                            ropes[ropes.Count - 2].positionCount = 2;
                                        }
                                    }

                                    isOptimizing = false;
                                }
                        }
                    }

                    //Set rope start and end positions after spring
                    else if(hooks[i].GetComponent<SpringJoint>() != null && hooks[i].GetComponent<SpringJoint>().connectedBody != player && ropes[i].positionCount == 2)
                    {
                            ropes[i].SetPosition(0, hooks[i].transform.position);
                            ropes[i].SetPosition(1, hookLatches[i].transform.position);
                    }

                    //Set rope collider size and position
                    if(ropeColliders.Count > 0 && hooks[i].GetComponent<SpringJoint>() != null)
                    {
                        ropeColliders[i].transform.position = hooks[i].transform.position;
                        ropeColliders[i].transform.LookAt(hooks[i].GetComponent<SpringJoint>().connectedBody.transform.position);
                        ropeColliders[i].GetComponent<BoxCollider>().size = new Vector3(0.1f, 0.1f, Vector3.Distance(hooks[i].transform.position, hooks[i].GetComponent<SpringJoint>().connectedBody.transform.position));
                        float worldZCenter = Vector3.Distance(hooks[i].GetComponent<SpringJoint>().connectedBody.transform.position, hooks[i].transform.position) / 2;
                        ropeColliders[i].GetComponent<BoxCollider>().center =  new Vector3(0f, 0f, worldZCenter);
                    }
                }
            }
        }


        //Rope collider Index checker for cutting
        int GameObjectToIndex(GameObject ropeColliderList)
        {
            for (int i = 0; i < ropeColliders.Count; i++)
            {
                //Check if A rope collider is in the List
                if (ropeColliders[i] == ropeColliderList)
                {
                    //Return the current index
                    return i;
                }
            }
            //Return if nothing
            return -1;
        }
    }
}
