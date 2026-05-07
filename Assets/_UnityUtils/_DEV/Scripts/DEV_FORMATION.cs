#if DOTWEEN
using DG.Tweening;
#elif PRIME_TWEEN
using PrimeTween;
#endif
using System.Collections.Generic;
using UnityEngine;

public class DEV_FORMATION : MonoBehaviour
{
    public List<Transform> childTransforms;
    public int baseChildCircle = 8;
    public float offsetCircle = 1f;

    public bool CENTER_CHILD = true;

    public FORMATION_ROTATION rotationType;
    public FORMATION_TYPE formationType;

    public bool USE_LOCAL_POS = false;

    [Button]
    public void DEV_FORMATION_GROUP_TWEEN()
    {
        if (childTransforms.Count == 0) return;

        if (formationType == FORMATION_TYPE.HORIZONTAL)
        {
            for (int i = 0; i < childTransforms.Count; i++)
            {
                Vector3 position = new Vector3(i * offsetCircle, 0, 0);
#if DOTWEEN
                childTransforms[i].DOLocalMove(position, 0.5f);
#elif PRIME_TWEEN
                Tween.LocalPosition(childTransforms[i], position, .5f);
#endif

            }
            return;
        }

#if DOTWEEN
        childTransforms[0].DOLocalMove(Vector3.zero, .5f);
#elif PRIME_TWEEN
        Tween.LocalPosition(childTransforms[0], Vector3.zero, .5f);
#endif
        

        int itemsInCurrentCircle = 0;
        float radius = offsetCircle;
        float fullCircleAngle = Mathf.PI * 2;
        float circleChildCount = baseChildCircle;
        float angleStep = fullCircleAngle / baseChildCircle;

        for (int i = 1; i < childTransforms.Count; i++)
        {
            if (itemsInCurrentCircle >= circleChildCount)
            {
                itemsInCurrentCircle = 0;
                radius += offsetCircle;
                circleChildCount *= 2;
                angleStep = fullCircleAngle / circleChildCount;
            }

            float angle = itemsInCurrentCircle * angleStep;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

#if DOTWEEN
            childTransforms[i].DOLocalMove(position, .5f);
#elif PRIME_TWEEN
            Tween.LocalPosition(childTransforms[i], position, .5f);
#endif
            
            itemsInCurrentCircle++;
        }
    }

    [Button]
    public void DEV_FORMATION_GROUP_POS()
    {
        if (childTransforms.Count == 0) return;

        if (formationType == FORMATION_TYPE.HORIZONTAL)
        {
            for (int i = 0; i < childTransforms.Count; i++)
            {
                Vector3 pos = new Vector3(i * offsetCircle, 0, 0);
                if (USE_LOCAL_POS)
                {
                    childTransforms[i].localPosition = pos;
                }
                else
                {
                    childTransforms[i].position = pos;
                }

                switch (rotationType)
                {
                    case FORMATION_ROTATION.LOOK_FORWARD:
                        childTransforms[i].rotation = Quaternion.LookRotation(Vector3.forward);
                        break;
                    case FORMATION_ROTATION.LOOK_BACK:
                        childTransforms[i].rotation = Quaternion.LookRotation(Vector3.back);
                        break;
                }
            }
            return;
        }

        if (CENTER_CHILD)
        {
            if (USE_LOCAL_POS)
            {
                childTransforms[0].localPosition = Vector3.zero;
            }
            else
            {
                childTransforms[0].position = Vector3.zero;
            }
            
        }

        int itemsInCurrentCircle = 0;
        float radius = offsetCircle;
        float fullCircleAngle = Mathf.PI * 2;
        float circleChildCount = baseChildCircle;
        float angleStep = fullCircleAngle / baseChildCircle;

        for (int i = CENTER_CHILD ? 1 : 0; i < childTransforms.Count; i++)
        {
            if (itemsInCurrentCircle >= circleChildCount)
            {
                itemsInCurrentCircle = 0;
                radius += offsetCircle;
                circleChildCount *= 2;
                angleStep = fullCircleAngle / circleChildCount;
            }

            float angle = itemsInCurrentCircle * angleStep;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            if (USE_LOCAL_POS)
            {
                childTransforms[i].localPosition = position;
            }
            else
            {
                childTransforms[i].position = position;
            }
            

            switch (rotationType)
            {
                case FORMATION_ROTATION.LOOK_FORWARD:
                    childTransforms[i].rotation = Quaternion.LookRotation(position, Vector3.up);
                    break;
                case FORMATION_ROTATION.LOOK_BACK:
                    childTransforms[i].rotation = Quaternion.LookRotation(-position, Vector3.up);
                    break;
            }

            itemsInCurrentCircle++;
        }
    }

    public enum FORMATION_ROTATION
    {
        NONE,
        LOOK_FORWARD,
        LOOK_BACK,
    }

    public enum FORMATION_TYPE
    {
        CIRCLE,
        HORIZONTAL
    }
}
