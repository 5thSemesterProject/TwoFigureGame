using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

[System.Serializable]
public class ActorData
{
    public GameObject actor;

    [HideInInspector]public CharacterType characterType;

    public ActorData(GameObject actor,Transform transformRef,CharacterType characterType)
    {
        this.actor = actor;
        this.characterType = characterType;
    }

    public void PrepareActorForScene()
    {   
        //Toogle Visibility
        var skinnedMeshRenderers = actor.GetComponentsInChildren<SkinnedMeshRenderer>();
        /*foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        if (skinnedMeshRenderer)
            skinnedMeshRenderer.enabled = false;*/
        
        //Disable Movement Script
        var movement = actor.GetComponentInChildren<Movement>();
        if (movement)
            movement.enabled = false;
    }

}

public class CutsceneHandler
{
    ActorData[] actorDatas;

    PlayableDirector playableDirector;

    public CutsceneHandler(ActorData[] actorDatas, PlayableDirector playableDirector)
    {
        this.actorDatas = actorDatas;
        this.playableDirector = playableDirector;
    }

    public PlayableDirector GetPlayableDirector()
    {
        return playableDirector;
    }

    public ActorData GetActorData(CharacterType characterType)
    {
        for (int i = 0; i < actorDatas.Length; i++)
        {
            if (actorDatas[i].characterType == characterType)
                return actorDatas[i];
        }
        
        return null;
    }

    public void LerpPosition(Transform playable, Transform actor, float speed=0.1f)
    {
         playableDirector.GetComponent<Cutscene>().StartCoroutine(_LerpPosition(playable,actor,speed));
    }

    IEnumerator _LerpPosition(Transform source, Transform target, float speed)
    {
        float tolerance=0.01f;
        float t = 0;
        while (Vector3.Distance(source.position,target.position)>tolerance)
        {
            t +=Time.deltaTime*speed;
            source.position = Vector3.Lerp(source.position,target.position,t);
            yield return null;
        }

        while (true)
        {
            source.position = target.position;
            yield return null;
        }
    }

    public void LerpBones(Transform playableRigRoot, Transform actorRigRoot)
    {   
        var playableRig = playableRigRoot.GetComponentsInChildren<Rig>()[1];
        playableRigRoot = playableRigRoot.GetComponentsInChildren<MultipleTagsTool>()[0].transform;
        actorRigRoot = actorRigRoot.GetComponentsInChildren<MultipleTagsTool>()[0].transform;

        Transform[] playableBones = playableRigRoot.GetComponentsInChildren<Transform>();
        Transform[] actorBones = actorRigRoot.GetComponentsInChildren<Transform>();
        MultiRotationConstraint[] playableRotationConstraints =GetMultiRotationConstraint(playableRig.transform);

        //Set Weight to zero
        playableRig.weight = 0;

        //Add Constraints
        for (int i = 0; i < playableRotationConstraints.Length; i++)
        {
            AddConstraint(playableBones[i],playableRotationConstraints[i],actorBones[i]);
        }

        //Build Rig
        RigBuilder rigBuilder = playableRig.GetComponentInParent<RigBuilder>();
        if (rigBuilder != null)
            rigBuilder.Build();

        playableDirector.GetComponent<Cutscene>().StartCoroutine(_LerpBone(playableRig));
    }

    private MultiRotationConstraint[] GetMultiRotationConstraint(Transform transform)
    {
        List<MultiRotationConstraint> allrotations = new List<MultiRotationConstraint>();
        var temp = transform.GetComponentsInChildren<Transform>();
        for (int i = 1; i < temp.Length; i++)
        {
            if (temp[i].TryGetComponent(out MultiRotationConstraint multiRotationConstraint))
            {
                allrotations.Add(multiRotationConstraint);
            }
            else
            {
                allrotations.Add(temp[i].AddComponent<MultiRotationConstraint>());
            }
        }
        return allrotations.ToArray();
    }

    void AddConstraint(Transform constrainedObj,MultiRotationConstraint rotationConstraint, Transform correspondingBone)
    {
        rotationConstraint.data.constrainedObject = constrainedObj;
        WeightedTransformArray array = new WeightedTransformArray { new WeightedTransform(correspondingBone,1)};
        rotationConstraint.data.sourceObjects = array;
    }

    IEnumerator _LerpBone(Rig playableRig,float speed = 0.1f)
    {
        float tolerance=0.01f;
        float t = 0;
        while (Mathf.Abs(1-t)>tolerance)
        {
            t +=Time.deltaTime*speed;
            playableRig.weight = t;
            yield return null;
        }
    }


    public void StopBlendingRotationAndPosition()
    {
        playableDirector.GetComponent<Cutscene>().StopAllCoroutines();
    }



}
