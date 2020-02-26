using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantController : MonoBehaviour
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
    public RessourceContainer ressourceContainer;

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
        
        ressourceContainer.AddItem("Wheat", 10);
    }

    private void LateUpdate()
    {
        Vector3 size = new Vector3(controller.radius * 1.1f, 0.5f * controller.height, controller.radius * 1.1f);
        Vector3 position = transform.TransformPoint(controller.center);
        scanLength = Physics.BoxCastNonAlloc(position, size, Vector3.forward, scanResults, Quaternion.identity, 1f, 1 << LayerMask.NameToLayer("Interaction"));
        
        interactionBox.position = position;
        interactionBox.localScale = 2 * size;
    }

    private void AnimationParameterRefresh()
    {
        // weapon code for attack animations
        if (weapon.equipedItem.type != WeaponItem.Type.None)
            animator.SetInteger("weapon", weapon.equipedItem.animationCode);
        else if (secondHand.equipedItem.type != SecondItem.Type.None)
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
        foreach (KeyValuePair<string, string> entry in interactionConditionList)
        {
            if (entry.Value != "ok")
            {
                success = false;
                break;
            }
        }

        if (!success)
            Debug.Log("NPC is doing weird stuff !! (" + gameObject.name + ")");
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

                if (ToolDictionary.Instance.toolTypes.ContainsKey(weapon.equipedItem.toolFamily))
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
        if (storehouse)
        {
            List<string> emptySlots = new List<string>();
            foreach (KeyValuePair<string, int> entry in ressourceContainer.inventory)
            {
                if (storehouse.acceptedResources.Contains(entry.Key) || storehouse.acceptedResources.Count == 0)
                {
                    storehouse.AddItem(ResourceDictionary.Instance.Get(entry.Key).name, entry.Value);
                    emptySlots.Add(entry.Key);
                }
            }

            if (emptySlots.Count != 0)
            {
                foreach (string slot in emptySlots)
                    ressourceContainer.inventory.Remove(slot);
                ressourceContainer.UpdateContent();
                RecomputeLoadFactor();
                audiosource.clip = backpackClear;
                audiosource.Play();
            }
            else
            {
                if (ressourceContainer.inventory.Count == 0)
                {
                    Debug.LogWarning("Impossible to be here, pb in container load computing");
                }
                else
                {
                    Debug.Log("NPC is doing weird stuff !! (" + gameObject.name + ")");
                }
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
        if (currentInteractor && interactionType == InteractionType.Type.collectWood)
        {
            CommonRessourceCollectionResolve();
        }
        else if (InteractionType.isCollectingMinerals(interactionType))
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
        if (soundFx)
            audiosource.Play();
        int gain = Random.Range(1, 4);

        // update inventory and compute load
        ressourceContainer.AddItem(ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(interactionType)).name, gain);
        RecomputeLoadFactor();

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
        if (!ressourceContainer.HasSpace())
        {
            Debug.Log("NPC is doing weird stuff !! (" + gameObject.name + ")");
            interactionDelay = 0;
            animator.SetBool("interaction", false);
        }
    }

    // helper
    public void RecomputeLoadFactor()
    {
        backpack.equipedItem.load = 0.3f + 0.3f * ressourceContainer.RecomputeLoad();
        float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load + backpack.equipedItem.load;
        loadFactor = loadCurve.Evaluate(0.1f * f);
        animator.SetFloat("loadFactor", loadFactor);
    }
}
