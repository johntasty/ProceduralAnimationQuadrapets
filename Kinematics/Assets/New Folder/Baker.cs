using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpellcastStudios.MeshCombiner;

public class Baker : MonoBehaviour
{
    public GameObject myRig;
    public Mesh[] meshes;
    public Material material;
    // Start is called before the first frame update
    void OnValidate()
    {
        GameObject rig = GameObject.Instantiate(myRig);

        //Find the "packed" renderer, which has bone info
        SkinnedMeshRenderer rigRenderer = rig.GetComponentInChildren<SkinnedMeshRenderer>();
        Transform[] bones = rigRenderer.bones;
        Transform rootBone = rigRenderer.rootBone;

        //Pack! (In this case, by overwriting the already existing renderer found on the rig)
        SpellcastStudios.MeshCombiner.CombineFast(rigRenderer, rootBone, material, bones, meshes);
    }

    // Update is called once per frame

}
