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
    public bool attacking;
    public float loadFactor = 0f;
    public AnimationCurve loadCurve;

    private float grounded = 0f;
    private float attackDelay = 0f;
    
    [Header("Equipement")]
    public WeaponSlot weapon;
    public SecondSlot secondHand;
    public ShieldSlot shield;
    public BackpackSlot backpack;
    public HeadSlot head;
    public BodySlot body;

    [Header("Interaction")]
    public bool interacting;
    public float collectCooldown = 0.8f;
    public float pickCooldown = 2f;
    public InteractionType.Type interactionType;
    public GameObject currentInteractor = null;
    public InteractionJuicer interactionJuicer;
    public RessourceContainer ressourceContainer;
    public InventoryViewer inventoryViewer;
    public InteractionHelper interactionHelper;

    private bool needEquipementAnimaionUpdate = false;
    private float interactionDelay = 0f;
    public float interactionConfirmedDelay = 0f;
    private RaycastHit[] scanResults = new RaycastHit[20];
    private int scanLength;
    private Dictionary<string, string> interactionConditionList = new Dictionary<string, string>();

    [Header("Sounds")]
    public List<AudioClip> wearHead;
    public List<AudioClip> wearBody;
    public List<AudioClip> movementHeavy;
    public AudioClip backpackClear;
    public AudioClip effortSound;
    private AudioSource audiosource;
    
    [Header("Debug")]
    public Transform interactionBox;

    private CharacterController controller;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;
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

        ressourceContainer.AddItem("Gold", 10);
        //ressourceContainer.AddItem("Iron", 10);
        //ressourceContainer.AddItem("Stone", 10);
        //ressourceContainer.AddItem("Wood", 10);
        //ressourceContainer.AddItem("Wheat", 10);
        if (inventoryViewer.visible)
            inventoryViewer.UpdateContent();
    }
    
    void Update()
    {
        float speedFactor = Input.GetKey(KeyCode.LeftShift) && loadFactor > 0.75f ? 2 : 1;
        direction = Vector3.zero;

        // action or attack
        if (Input.GetMouseButtonDown(0) && attackDelay <= 0)
        {
            animator.SetTrigger("attack");
            attackDelay = attackCooldown;
            attacking = true;
            audiosource.clip = effortSound;
            audiosource.Play();

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
            GameObject go = scanResults[0].collider.gameObject;
            InteractionType[] interactions = go.GetComponents<InteractionType>();
            foreach (InteractionType interaction in interactions)
            {
                Interact(interaction.type, go);
            }
            if (needEquipementAnimaionUpdate)
                AnimationParameterRefresh();
        }
        interacting = currentInteractor && (interactionDelay > 0f || interactionConfirmedDelay > 0f);
        animator.SetBool("interaction", interacting);

        // interaction time integral
        if (interacting && Input.GetKey(KeyCode.Space) && interactionConfirmedDelay > 0f)
        {
            interactionConfirmedDelay += Time.deltaTime;
            if (interactionConfirmedDelay >= pickCooldown)
            {
                InteractionConfirmed();
                interactionConfirmedDelay = 0f;
            }
        }
        else interactionConfirmedDelay = 0f;
        interactionJuicer.loadingRate = interactionConfirmedDelay / pickCooldown;

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
        else if(interacting)
        {
            direction = (currentInteractor.transform.position - transform.position).normalized;
        }
        if (direction.x != 0f || direction.z != 0f)
        {
            Quaternion goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, (attacking | interacting ? aimingAttackSpeed : aimingSpeed) * Time.deltaTime);
        }

        if ((direction.x != 0f || direction.z != 0f) && speedFactor >= 2 && Random.Range(0,4) == 0)
        {
            particles.Emit(emitParams, 1);
        }

        // update timers
        if (attackDelay > 0)
            attackDelay -= Time.deltaTime;
        if (interactionDelay > 0 && !Input.GetKey(KeyCode.Space))
            interactionDelay -= Time.deltaTime;
    }
    private void LateUpdate()
    {
        Vector3 size = new Vector3(controller.radius * 1.1f, 0.5f * controller.height, controller.radius * 1.1f);
        Vector3 position = transform.TransformPoint(controller.center);
        scanLength = Physics.BoxCastNonAlloc(position, size, Vector3.forward, scanResults, Quaternion.identity, 1f, 1 << LayerMask.NameToLayer("Interaction"));

        if (scanLength > 0)
            interactionJuicer.hovered = scanResults[0].collider.gameObject;
        else interactionJuicer.hovered = null;
        inventoryViewer.storage = (interactionJuicer.hovered != null && interactionJuicer.hovered.GetComponent<RessourceContainer>() != null);

        interactionBox.position = position;
        interactionBox.localScale = 2*size;
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
                interactionType = type;
                currentInteractor = interactor;
                success = true;
                interactionConfirmedDelay = 0.001f;
                break;

            case InteractionType.Type.storeRessources:
                if (ressourceContainer.load != 0)
                {
                    interactionType = type;
                    currentInteractor = interactor;
                    success = true;
                    interactionConfirmedDelay = 0.001f;
                }
                else
                {
                    interactionConditionList.Clear();
                    interactionConditionList.Add("Container", "empty");
                    interactionHelper.UpdateContent(interactionConditionList);
                    success = false;
                }
                break;

            // standart ressources collection
            case InteractionType.Type.collectStone:
            case InteractionType.Type.collectIron:
            case InteractionType.Type.collectGold:
            case InteractionType.Type.collectCrystal:
            case InteractionType.Type.collectWood:
            case InteractionType.Type.collectWheet:
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
    private void InteractionConfirmed()
    {
        switch (interactionType)
        {
            // pick an item
            case InteractionType.Type.pickableBackpack:
            case InteractionType.Type.pickableBody:
            case InteractionType.Type.pickableHead:
            case InteractionType.Type.pickableSecond:
            case InteractionType.Type.pickableShield:
            case InteractionType.Type.pickableWeapon:
                pickableInteraction(interactionType, currentInteractor);
                break;

            // standart ressources collection -> no confirmed action
            case InteractionType.Type.collectStone:
            case InteractionType.Type.collectIron:
            case InteractionType.Type.collectGold:
            case InteractionType.Type.collectCrystal:
            case InteractionType.Type.collectWood:
                break;

            // store all ressources
            case InteractionType.Type.storeRessources:
                storeAllInteraction(interactionType, currentInteractor);
                break;

            // error
            default:
                Debug.LogWarning("no interaction defined for this type " + interactionType.ToString());
                break;
        }
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
        float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load + backpack.equipedItem.load;
        loadFactor = loadCurve.Evaluate(0.1f * f);
        animator.SetFloat("loadFactor", loadFactor);

        // load clips
        AnimationClip[] clips = Arsenal.Instance.GetAnimationClip(ref weapon.equipedItem, ref secondHand.equipedItem, ref shield.equipedItem, 
                                                                  ref body.equipedItem, ref head.equipedItem, ref backpack.equipedItem);
        clipOverrides["idle"] = clips[0];
        clipOverrides["walk"] = clips[1];
        clipOverrides["run"] = clips[2];
        clipOverrides["attack"] = clips[3];
        animatorOverrideController.ApplyOverrides(clipOverrides);

        needEquipementAnimaionUpdate = false;
    }
    private void InitiateRessourceInteraction(InteractionType.Type type, GameObject interactor)
    {
        interactionDelay = collectCooldown;
        animator.SetTrigger("interact");
        currentInteractor = interactor;
        interactionType = type;
    }
    private bool collectRessourceInteraction(InteractionType.Type type, GameObject interactor)
    {
        bool success = true;
        ComputeInteractionConditions(type);
        foreach(KeyValuePair<string, string> entry in interactionConditionList)
        {
            if(entry.Value != "ok")
            {
                success = false;
                break;
            }
        }
        
        if(!success)
            interactionHelper.UpdateContent(interactionConditionList);
        else
            InitiateRessourceInteraction(type, interactor);
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

                if(ToolDictionary.Instance.toolTypes.ContainsKey(weapon.equipedItem.toolFamily))
                {
                    List<AudioClip> sounds = ToolDictionary.Instance.Get(weapon.equipedItem.toolFamily).collectionSound;
                    audiosource.clip = sounds[Random.Range(0, sounds.Count)];
                    audiosource.Play();
                }
            }
        }
        else if (type == InteractionType.Type.pickableBackpack)
        {
            BackpackItem item = interactor.GetComponent<BackpackItem>();
            if (item && backpack.Equip(item.type))
                success = true;
            needEquipementAnimaionUpdate = true;

            if (ToolDictionary.Instance.toolTypes.ContainsKey(backpack.equipedItem.toolFamily))
            {
                List<AudioClip> sounds = ToolDictionary.Instance.Get(backpack.equipedItem.toolFamily).collectionSound;
                audiosource.clip = sounds[Random.Range(0, sounds.Count)];
                audiosource.Play();
            }
        }
        else if (type == InteractionType.Type.pickableHead)
        {
            HeadItem item = interactor.GetComponent<HeadItem>();
            if (item && head.Equip(item.type))
            {
                success = true;
                needEquipementAnimaionUpdate = true;
                int index = Mathf.Clamp((int)HeadItem.getCategory(head.equipedItem.type), 0, wearHead.Count - 1);
                audiosource.clip = wearHead[index];
                audiosource.Play();
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
                needEquipementAnimaionUpdate = true;
                int index = Mathf.Clamp((int)BodyItem.getCategory(body.equipedItem.type), 0, wearBody.Count - 1);
                audiosource.clip = wearBody[index];
                audiosource.Play();
            }
        }

        return success;
    }
    private bool storeAllInteraction(InteractionType.Type type, GameObject interactor)
    {
        RessourceContainer storehouse = interactor.GetComponent<RessourceContainer>();
        if(storehouse)
        {
            List<string> emptySlots = new List<string>();
            foreach (KeyValuePair<string, int> entry in ressourceContainer.inventory)
            {
                if(storehouse.acceptedResources.Contains(entry.Key) || storehouse.acceptedResources.Count == 0)
                {
                    storehouse.AddItem(ResourceDictionary.Instance.Get(entry.Key).name, entry.Value);
                    if (inventoryViewer.visible)
                        inventoryViewer.UpdateContent();
                    backpack.equipedItem.load = 0.3f;
                    float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load + backpack.equipedItem.load;
                    loadFactor = loadCurve.Evaluate(0.1f * f);
                    animator.SetFloat("loadFactor", loadFactor);
                    emptySlots.Add(entry.Key);
                }
            }

            if (emptySlots.Count != 0)
            {
                foreach (string slot in emptySlots)
                    ressourceContainer.inventory.Remove(slot);
                ressourceContainer.UpdateContent();
                if (inventoryViewer.visible)
                    inventoryViewer.UpdateContent();
                audiosource.clip = backpackClear;
                audiosource.Play();
            }
            else
            {
                interactionConditionList.Clear();
                foreach(string acceptance in storehouse.acceptedResources)
                    interactionConditionList.Add(acceptance, "nor");
                interactionHelper.UpdateContent(interactionConditionList);
            }
        }
        return true;
    }

    // Callbacks and coroutine
    public void AttackEnd()
    {
        attacking = false;
    }
    public void InteractionTick()
    {
        interactionDelay = collectCooldown;
        if(currentInteractor && interactionType == InteractionType.Type.collectWood)
        {
            CommonRessourceCollectionResolve();
            interactionJuicer.treeInteractor = currentInteractor;
        }
        else if(InteractionType.isCollectingMinerals(interactionType))
        {
            CommonRessourceCollectionResolve();
        }
        else if (currentInteractor && interactionType == InteractionType.Type.collectWheet)
        {
            CommonRessourceCollectionResolve();
        }
    }
    private void CommonRessourceCollectionResolve()
    {
        // play sound, and juice, and update inventory
        List<AudioClip> sounds = ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(interactionType)).collectionSound;
        AudioClip soundFx = sounds[Random.Range(0, sounds.Count)];
        audiosource.clip = soundFx;
        if(soundFx)
            audiosource.Play();
        int gain = Random.Range(1, 4);
        interactionJuicer.treeInteractor = currentInteractor;
        interactionJuicer.LaunchGainAnim("+" + gain.ToString(), interactionType);
        
        // update inventory and compute load
        ressourceContainer.AddItem(ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(interactionType)).name, gain);
        if (inventoryViewer.visible)
            inventoryViewer.UpdateContent();
        backpack.equipedItem.load = 0.3f + 0.3f * ressourceContainer.load;
        float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load + backpack.equipedItem.load;
        loadFactor = loadCurve.Evaluate(0.1f * f);
        animator.SetFloat("loadFactor", loadFactor);

        // decrement interactor ressource count
        CollectData data = currentInteractor.GetComponent<CollectData>();
        data.ressourceCount--;
        if (data.ressourceCount <= 0)
        {
            Destroy(currentInteractor.transform.parent.gameObject);
            interactionDelay = 0;
            interacting = false;
            currentInteractor = null;
        }

        // stop interaction loop if needed
        if(!ressourceContainer.HasSpace())
        {
            ComputeInteractionConditions(interactionType);
            interactionHelper.UpdateContent(interactionConditionList);
            interactionDelay = 0;
            animator.SetBool("interaction", false);
        }
    }
    private void ComputeInteractionConditions(InteractionType.Type type)
    {
        interactionConditionList.Clear();
        string[] equiped = { backpack.equipedItem.toolFamily , weapon.equipedItem.toolFamily };
        ResourceType resource = ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(type));
        foreach (string tool in resource.collectionTools)
        {
            string status = "nok";
            foreach (string s in equiped)
            {
                if(s == tool)
                {
                    if (tool == "Container")
                        status = ressourceContainer.HasSpace() ? "ok" : "full";
                    else status = "ok";
                }
            }
            interactionConditionList.Add(tool, status);
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
