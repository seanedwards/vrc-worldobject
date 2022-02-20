using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using System.Text.RegularExpressions;

namespace Tests
{
    public class WorldObject
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/vrc-worldobject/World Constraint.prefab");

        [Test]
        public void WorldConstraintPasses()
        {

            Assert.AreEqual("World Constraint", prefab.name);
            GameObject testObject;

            // Container
            testObject = prefab.transform.Find("Container").gameObject;
            Assert.AreEqual(testObject.transform.position, Vector3.zero);

            // XMover
            testObject = prefab.transform.Find("XMover").gameObject;
            MoverTests(testObject);

            //XFineMover
            testObject = testObject.transform.Find("XFineMover").gameObject;
            Assert.AreEqual("XFineMover", testObject.name);
            MoverTests(testObject);

            //YMover
            testObject = testObject.transform.Find("YMover").gameObject;
            Assert.AreEqual("YMover", testObject.name);
            MoverTests(testObject);

            //YFineMover
            testObject = testObject.transform.Find("YFineMover").gameObject;
            Assert.AreEqual("YFineMover", testObject.name);
            MoverTests(testObject);

            //ZMover
            testObject = testObject.transform.Find("ZMover").gameObject;
            Assert.AreEqual("ZMover", testObject.name);
            MoverTests(testObject);

            //ZFineMover
            testObject = testObject.transform.Find("ZFineMover").gameObject;
            Assert.AreEqual("ZFineMover", testObject.name);
            MoverTests(testObject);

        }

        void MoverTests(GameObject mover)
        {
            Assert.AreEqual(Vector3.zero, mover.transform.position);
            Assert.AreEqual(Quaternion.identity, mover.transform.rotation);
            Assert.AreEqual(Vector3.one, mover.transform.localScale);
        }


        [Test]
        public void WorldObjectControllerPasses()
        {
            RuntimeAnimatorController animationController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/vrc-worldobject/WorldObjectController.controller");
            Assert.AreEqual("WorldObjectController", animationController.name);
            Assert.AreEqual(15, animationController.animationClips.Length);

            GameObject avatar = new GameObject("TestAvatar");
            GameObject worldObject = GameObject.Instantiate(prefab, avatar.transform);
            

            foreach (AnimationClip clip in animationController.animationClips)
            {
                TestContext.Out.WriteLine(clip.name);
                Match match = Regex.Match(clip.name, "^(X|Y|Z|R)(\\+|-)(\\d+|Fine)?$");
                Assert.IsTrue(match.Success);
                
                string axis = match.Groups[0].Value;
                string sign = match.Groups[1].Value;
                string qual = match.Groups[2].Value;

                float offset = (qual == "Fine") ? 3.9063f : 1000f;
                offset = (sign == "-") ? -offset : offset;

                switch (axis)
                {
                    case "X":
                        clip.SampleAnimation(avatar, 0);
                        Assert.AreEqual(new Vector3(offset, 0, 0), worldObject.transform.Find($"X{qual}Mover").position);
                        break;
                    case "Y":
                        clip.SampleAnimation(avatar, 0);
                        Assert.AreEqual(new Vector3(0, offset, 0), worldObject.transform.Find($"Y{qual}Mover").position);
                        break;
                    case "Z":
                        clip.SampleAnimation(avatar, 0);
                        Assert.AreEqual(new Vector3(0, 0, offset), worldObject.transform.Find($"Z{qual}Mover").position);
                        break;

                    case "R":
                        float r = float.Parse(qual);

                        clip.SampleAnimation(avatar, 0);
                        Assert.AreEqual(Quaternion.Euler(0, (sign == "-") ? -r : r, 0), worldObject.transform.Find("Rotater").rotation);
                        break;
                }
                
            }
        }

        [Test]
        public void WorldObjectParametersPasses()
        {
            VRCAvatarParameterDriver parameters = AssetDatabase.LoadAssetAtPath<VRCAvatarParameterDriver>("Assets/vrc-worldobject/WorldObjectParameters.asset");
            //Assert.AreEqual("WorldObjectParameters", parameters.name);


            // Use the Assert class to test conditions
        }
    }
}
