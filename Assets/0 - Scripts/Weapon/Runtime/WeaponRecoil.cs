using UnityEngine;

public struct BallisticRecoil
{
    public float vertical;
    public float horizontal;
}

public class WeaponRecoil : MonoBehaviour
{
    private WeaponController weaponController;

    [SerializeField]
    private Transform recoilTransform;

    [SerializeField]
    private float recoilReturnSpeed = 10f;

    [SerializeField]
    private float recoilSnappiness = 20f;

    private Vector3 targetPosition;
    private Vector3 currentPosition;

    private Quaternion targetRotation;
    private Quaternion currentRotation;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private float currentRecoil;

    private float currentVerticalRecoil;
    private float currentHorizontalRecoil;

    public Transform RecoilTransform => recoilTransform;
    public float CurrentRecoil => currentRecoil;

    private void Awake()
    {
        weaponController = GetComponentInChildren<WeaponController>();

        targetPosition = Vector3.zero;
        currentPosition = Vector3.zero;

        targetRotation = Quaternion.identity;
        currentRotation = Quaternion.identity;

        originalPosition = recoilTransform.localPosition;
        originalRotation = recoilTransform.localRotation;
    }

    private void Update()
    {
        if (weaponController.CurrentState != WeaponState.Firing)
        {
            currentRecoil = Mathf.MoveTowards(
                currentRecoil,
                0f,
                weaponController.WeaponData.recoilRecovery * Time.deltaTime
            );

            currentHorizontalRecoil = Mathf.MoveTowards(
                currentHorizontalRecoil,
                0f,
                weaponController.WeaponData.recoilRecovery * Time.deltaTime
            );
        }

        currentPosition = Vector3.Lerp(
            currentPosition,
            targetPosition,
            recoilSnappiness * Time.deltaTime
        );

        currentRotation = Quaternion.Slerp(
            currentRotation,
            targetRotation,
            recoilSnappiness * Time.deltaTime
        );

        targetPosition = Vector3.Lerp(
            targetPosition,
            Vector3.zero,
            recoilReturnSpeed * Time.deltaTime
        );

        targetRotation = Quaternion.Slerp(
            targetRotation,
            Quaternion.identity,
            recoilReturnSpeed * Time.deltaTime
        );

        recoilTransform.localPosition =
            originalPosition + currentPosition;

        recoilTransform.localRotation =
            originalRotation * currentRotation;
    }

    public BallisticRecoil AddRecoil(float amount)
    {
        float damping = GetRecoilDamping();

        currentHorizontalRecoil += Random.Range(
            -weaponController.WeaponData.recoilHorizontal,
            weaponController.WeaponData.recoilHorizontal
        );

        currentHorizontalRecoil = Mathf.Clamp(
            currentHorizontalRecoil,
            -weaponController.WeaponData.maxRecoil,
            weaponController.WeaponData.maxRecoil
        );

        BallisticRecoil ballisticRecoil = new BallisticRecoil
        {
            vertical = currentRecoil,
            horizontal = currentHorizontalRecoil
        };

        currentRecoil += amount * damping;

        if (currentRecoil > weaponController.WeaponData.maxRecoil)
        {
            currentRecoil = weaponController.WeaponData.maxRecoil;
        }

        ApplyVerticalRecoil(ballisticRecoil.vertical);
        ApplyHorizontalRecoil(currentHorizontalRecoil);
        ApplyBackwardRecoil();

        return ballisticRecoil;
    }

    private float GetRecoilDamping()
    {
        float normalizedRecoil =
            currentRecoil / weaponController.WeaponData.maxRecoil;

        return weaponController.WeaponData.recoilDamping.Evaluate(
            normalizedRecoil
        );
    }

    private void ApplyVerticalRecoil(float recoil)
    {
        targetRotation *= Quaternion.Euler(
            -recoil,
            0f,
            0f
        );
    }

    private void ApplyHorizontalRecoil(float recoil)
    {
        targetRotation *= Quaternion.Euler(
            0f,
            recoil,
            0f
        );
    }

    private void ApplyBackwardRecoil()
    {
        float recoil =
            weaponController.WeaponData.recoilBack;

        targetPosition += Vector3.back * recoil;
    }
}