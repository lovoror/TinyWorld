using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement and machine states")]
    public float aimingSpeed = 4;
    public float aimingAttackSpeed = 20;
    public float runSpeed = 4;
    public float gravity = 8;
    public float attackCooldown = 0.8f;
    private float attackDelay = 0f;
    public bool attacking;
    public float loadFactor = 0f;
    public AnimationCurve loadCurve;
    private float grounded = 0f;

    [Header("Equipement slots")]
    public WeaponSlot weapon;
    public SecondSlot secondHand;
    public ShieldSlot shield;
    public BackpackSlot backpack;
    public HeadSlot head;
    public BodySlot body;

    [Header("Interaction data")]
    public bool interacting;
    private RaycastHit[] scanResults = new RaycastHit[20];
    private int scanLength;
    public float interactCooldown = 0.8f;
    private float interactionDelay = 0f;
    public AnimationCurve woodCuttingAnimation;
    public InteractionType.Type interactionType;
    private bool needEquipementAnimaionUpdate = false;
    private IEnumerator woodCuttingCoroutine;

    [Header("Sound data")]
    public List<AudioClip> chopWoodClips;
    private AudioSource audiosource;
    public List<AudioClip> collectMineralsClips;

    [Header("Debug")]
    public Transform interactionBox;
    public GameObject currentInteractor;

    private CharacterController controller;
    private Animator animator;
    protected AnimatorOverrideController animatorOverrideController;
    protected AnimationClipOverrides clipOverrides;
    private ParticleSystem particles;

    private Vector3 direction = Vector3.zero;
    private Vector3 target;
    private ParticleSystem.EmitParams emitParams;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;
        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
        
        particles = GetComponent<ParticleSystem>();
        attacking = false;
        interacting = false;
        emitParams = new ParticleSystem.EmitParams();
        audiosource = GetComponent<AudioSource>();

        weapon.Equip(weapon.equipedItem.type, true);
        secondHand.Equip(secondHand.equipedItem.type, true);
        head.Equip(head.equipedItem.type, true);
        shield.Equip(shield.equipedItem.type, true);
        body.Equip(body.equipedItem.type, true);
        backpack.Equip(backpack.equipedItem.type, true);

        AnimationParameterRefresh();
    }


    void Update()
    {
        float speedFactor = Input.GetKey(KeyCode.LeftShift) ? 2 : 1;
        direction = Vector3.zero;

        // action or attack
        if (Input.GetKeyDown(KeyCode.Mouse0) && attackDelay <= 0)
        {
            animator.SetTrigger("attack");
            attackDelay = attackCooldown;
            attacking = true;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                target = hit.point;
            }
        }

        // interaction
        else if(scanLength != 0 && Input.GetKeyDown(KeyCode.Space) && !interacting)
        {
            for(int i=0; i<scanLength; i++)
            {
                GameObject go = scanResults[i].collider.gameObject;
                InteractionType[] interactions = go.GetComponents<InteractionType>();
                foreach(InteractionType interaction in interactions)
                {
                    Interact(interaction.type, go);
                }
                if(needEquipementAnimaionUpdate)
                    AnimationParameterRefresh();
            }
        }
        interacting = interactionDelay > 0f;
        animator.SetBool("interaction", interacting);

        // movement
        if ((controller.isGrounded || grounded < 0.2f) && !attacking && !interacting)
        {
            // compute direction
            if (Input.GetKey(KeyCode.Z))
                direction = new Vector3(0, 0, 1);
            else if (Input.GetKey(KeyCode.S))
                direction = new Vector3(0, 0, -1);
            if (Input.GetKey(KeyCode.D))
                direction += new Vector3(1, 0, 0);
            else if (Input.GetKey(KeyCode.Q))
                direction += new Vector3(-1, 0, 0);
            direction.Normalize();

            // compute animation parameters
            if (direction.x == 0f && direction.z == 0f)
                animator.SetFloat("run", 0f);
            else
                animator.SetFloat("run", speedFactor);
            animator.SetFloat("loadFactor", loadFactor * speedFactor);

            // update position
            direction = speedFactor * loadFactor * runSpeed * direction;
            direction = direction.x * Camera.main.transform.right + direction.z * Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            grounded = 0f;
        }
        else grounded += Time.deltaTime;

        // move 
        direction.y = -gravity * Time.deltaTime;
        controller.Move(direction * Time.deltaTime);

        // aiming
        if (attacking)
        {
            direction = (target - transform.position).normalized;
        }
        if (direction.x != 0f || direction.z != 0f)
        {
            Quaternion goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, (attacking ? aimingAttackSpeed : aimingSpeed) * Time.deltaTime);
        }

        if ((direction.x != 0f || direction.z != 0f) && Input.GetKey(KeyCode.LeftShift) && Random.Range(0,4) == 0)
        {
            particles.Emit(emitParams, 1);
        }

        // update timers
        if (attackDelay > 0)
            attackDelay -= Time.deltaTime;
        if (interactionDelay > 0 && !Input.GetKey(KeyCode.Space))
            interactionDelay -= Time.deltaTime;
    }

    private bool Interact(InteractionType.Type type, GameObject interactor)
    {
        bool success = false;
        
        switch(type)
        {
            // pick an item
            case InteractionType.Type.pickableBackpack:
            case InteractionType.Type.pickableBody:
            case InteractionType.Type.pickableHead:
            case InteractionType.Type.pickableSecond:
            case InteractionType.Type.pickableShield:
            case InteractionType.Type.pickableWeapon:
                success = pickableInteraction(type, interactor);
                break;

            // standart ressources collection
            case InteractionType.Type.collectStone:
            case InteractionType.Type.collectIron:
            case InteractionType.Type.collectGold:
            case InteractionType.Type.collectCrystal:
            case InteractionType.Type.collectWood:
                success = collectRessourceInteraction(type, interactor);
                break;

            // error
            default:
                Debug.LogWarning("no interaction defined for this type " + type.ToString());
                success = false;
                break;
        }
        return success;
    }

    
    private void LateUpdate()
    {
        Vector3 size = new Vector3(controller.radius * 1.1f, 0.5f * controller.height, controller.radius * 1.1f);
        Vector3 position = transform.TransformPoint(controller.center);
        scanLength = Physics.BoxCastNonAlloc(position, size, Vector3.forward, scanResults, Quaternion.identity, 1f, 1 << 8);

        interactionBox.position = position;
        interactionBox.localScale = 2*size;
    }

    private void AnimationParameterRefresh()
    {
        // weapon code for attack animations
        if (weapon.equipedItem.type != WeaponItem.Type.None)
            animator.SetInteger("weapon", weapon.equipedItem.animationCode);
        else if(secondHand.equipedItem.type != SecondItem.Type.None)
            animator.SetInteger("weapon", secondHand.equipedItem.animationCode);
        else animator.SetInteger("weapon", 0);

        // shield for run, and idle
        animator.SetBool("shield", shield.equipedItem.type != ShieldItem.Type.None);

        // compute load
        float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load;
        loadFactor = loadCurve.Evaluate(0.1f * f);
        animator.SetFloat("loadFactor", loadFactor);

        // load clips
        AnimationClip[] clips = Arsenal.Instance.GetAnimationClip(ref weapon.equipedItem, ref secondHand.equipedItem, ref shield.equipedItem, ref body.equipedItem, ref head.equipedItem, ref backpack.equipedItem);
        clipOverrides["idle"] = clips[0];
        clipOverrides["walk"] = clips[1];
        clipOverrides["run"] = clips[2];
        clipOverrides["attack"] = clips[3];
        animatorOverrideController.ApplyOverrides(clipOverrides);

        needEquipementAnimaionUpdate = false;
    }

    private bool collectRessourceInteraction(InteractionType.Type type, GameObject interactor)
    {
        bool success = false;
        if(type == InteractionType.Type.collectWood)
        {
            if (WeaponItem.isAxe(weapon.equipedItem.type))
            {
                interactionDelay = interactCooldown;
                animator.SetTrigger("interact");
                currentInteractor = interactor;
                interactionType = type;
            }
        }
        else
        {
            if(weapon.equipedItem.type == WeaponItem.Type.Pickaxe)
            {
                interactionDelay = interactCooldown;
                animator.SetTrigger("interact");
                currentInteractor = interactor;
                interactionType = type;
            }
        }
        return success;
    }

    private bool pickableInteraction(InteractionType.Type type, GameObject interactor)
    {
        bool success = false;
        if (type == InteractionType.Type.pickableWeapon)
        {
            WeaponItem item = interactor.GetComponent<WeaponItem>();
            if (item && weapon.Equip(item.type))
            {
                if (item.forbidSecond || secondHand.equipedItem.forbidWeapon)
                    secondHand.Equip(SecondItem.Type.None);
                if (item.forbidShield)
                    shield.Equip(ShieldItem.Type.None);
                success = true;
                needEquipementAnimaionUpdate = true;
            }
        }
        else if (type == InteractionType.Type.pickableBackpack)
        {
            BackpackItem item = interactor.GetComponent<BackpackItem>();
            if (item && backpack.Equip(item.type))
                success = true;
            needEquipementAnimaionUpdate = true;
        }
        else if (type == InteractionType.Type.pickableHead)
        {
            HeadItem item = interactor.GetComponent<HeadItem>();
            if (item && head.Equip(item.type))
            {
                success = true;
                //needEquipementAnimaionUpdate = true;
            }
        }
        else if (type == InteractionType.Type.pickableSecond)
        {
            SecondItem item = interactor.GetComponent<SecondItem>();
            if (item && secondHand.Equip(item.type))
            {
                if (item.forbidWeapon || weapon.equipedItem.forbidSecond)
                    weapon.Equip(WeaponItem.Type.None);
                if (item.forbidShield)
                    shield.Equip(ShieldItem.Type.None);
                success = true;
                needEquipementAnimaionUpdate = true;
            }
        }
        else if (type == InteractionType.Type.pickableShield)
        {
            ShieldItem item = interactor.GetComponent<ShieldItem>();
            if (item && shield.Equip(item.type))
            {
                if (weapon.equipedItem.forbidShield)
                    weapon.Equip(WeaponItem.Type.None);
                if (secondHand.equipedItem.forbidShield)
                    secondHand.Equip(SecondItem.Type.None);
                success = true;
                needEquipementAnimaionUpdate = true;
            }
        }
        else if (type == InteractionType.Type.pickableBody)
        {
            BodyItem item = interactor.GetComponent<BodyItem>();
            if (item && body.Equip(item.type))
            {
                success = true;
                //needEquipementAnimaionUpdate = true;
            }
        }
        return success;
    }

    // Callbacks and coroutine
    public void AttackEnd()
    {
        attacking = false;
    }
    public void InteractionTick()
    {
        interactionDelay = interactCooldown;
        if(currentInteractor && interactionType == InteractionType.Type.collectWood)
        {
            woodCuttingCoroutine  = WoodCuttingAnimation(currentInteractor);
            StartCoroutine(woodCuttingCoroutine);
            audiosource.clip = chopWoodClips[Random.Range(0, chopWoodClips.Count)];
            audiosource.Play();
        }
        else if(InteractionType.isCollectingMinerals(interactionType))
        {
            audiosource.clip = collectMineralsClips[Random.Range(0, collectMineralsClips.Count)];
            audiosource.Play();
        }
    }
    private IEnumerator WoodCuttingAnimation(GameObject interactor)
    {
        Transform tree = interactor.transform.parent.Find("Armature");
        Quaternion initial = tree.rotation;
        Vector3 v = (tree.position - transform.position).normalized;
        Quaternion q = Quaternion.AngleAxis(-15f, Vector3.Cross(v, Vector3.up).normalized) * initial;
        int speed = 30;
        for (int i = 0; i< speed; i++)
        {
            tree.rotation = Quaternion.Lerp(initial, q, woodCuttingAnimation.Evaluate((float)i / speed));
            yield return null;
        }
    }
}


public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}
