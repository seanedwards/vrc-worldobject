using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using System.Text.RegularExpressions;
using NUnit.Framework.Interfaces;
using UnityEngine.SceneManagement;
using System;

namespace Tests
{
    public class WorldObjectAvatarDiagnostics
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


        [UnityTest]
        public IEnumerator WorldObjectAnimationPasses()
        {
            // In this test we will find the avatar and the World Constraint object underneath it.
            // Then we will test each animation for the correct position values.

            yield return new EnterPlayMode();

            VRCAvatarDescriptor avatarDescriptor = GameObject.FindObjectOfType<VRCAvatarDescriptor>();
            GameObject avatar = avatarDescriptor.gameObject;
            Assert.IsNotNull(avatar);

            GameObject worldObject = avatar.transform.Find("World Constraint").gameObject;
            Assert.IsNotNull(worldObject);

            GameObject container = worldObject.transform.Find("Container").gameObject;
            Assert.IsNotNull(container);

            RuntimeAnimatorController animationController = avatarDescriptor.baseAnimationLayers[4].animatorController;

            int animCount = 0;

            foreach (AnimationClip clip in animationController.animationClips)
            {
                Match match = Regex.Match(clip.name, "^(X|Y|Z|R)(\\+|-)(\\d+|Fine)?$");
                if (!match.Success)
                {
                    // This is not one of our animations, nothing to test.
                    continue;
                }
                TestContext.Out.WriteLine($"Testing {clip.name}...");
                animCount++;
                
                string axis = match.Groups[1].Value;
                string sign = match.Groups[2].Value;
                string qual = match.Groups[3].Value;

                float offset = (qual == "Fine") ? 3.9063f : 1000f;
                offset = (sign == "-") ? -offset : offset;

                clip.SampleAnimation(avatar, 0);

                Vector3 pos = Vector3.zero;
                switch (axis)
                {
                    case "X":
                        pos = new Vector3(offset, 0, 0);
                        break;
                    case "Y":
                        pos = new Vector3(0, offset, 0);
                        break;
                    case "Z":
                        pos = new Vector3(0, 0, offset);
                        break;

                    case "R":
                        float r = float.Parse(qual);

                        float angle = Quaternion.Angle(Quaternion.Euler(0, (sign == "-") ? -r : r, 0), FindDeepChild(worldObject.transform, "Rotater").rotation);
                        Assert.AreEqual(0 , angle);
                        break;
                }

                if (axis != "R")
                {
                    float dist = Vector3.Distance(pos, FindDeepChild(worldObject.transform, $"{axis}{qual}Mover").position);
                    Assert.Less(dist, 0.0001);
                }
                yield return null; // Check the container position on the next frame
                //Assert.AreEqual(pos, container.transform.position);

            }

            Assert.AreEqual(15, animCount);

            yield return new ExitPlayMode();
        }

        static Dictionary<string, Tuple<bool, bool>> axisBinary = new Dictionary<string, Tuple<bool, bool>>
        {
            { "X", (false, false).ToTuple() },
            { "Y", (false, true).ToTuple() },
            { "Z", (true, false).ToTuple() },
            { "R", (true, true).ToTuple() },
        };

        [UnityTest]
        public IEnumerator WorldObjectParameterPasses()
        {
            yield return new EnterPlayMode();

            VRCAvatarDescriptor avatarDescriptor = GameObject.FindObjectOfType<VRCAvatarDescriptor>();
            GameObject avatar = avatarDescriptor.gameObject;
            Assert.IsNotNull(avatar);

            GameObject worldObject = avatar.transform.Find("World Constraint").gameObject;
            Assert.IsNotNull(worldObject);

            GameObject container = worldObject.transform.Find("Container").gameObject;
            Assert.IsNotNull(container);

            RuntimeAnimatorController animationController = avatarDescriptor.baseAnimationLayers[4].animatorController;
            var animator = avatar.GetComponent<Animator>();

            var worldAxis0 = Array.Find(avatarDescriptor.expressionParameters.parameters, param => param.name == "WorldAxis0");
            var worldAxis1 = Array.Find(avatarDescriptor.expressionParameters.parameters, param => param.name == "WorldAxis1");
            var worldAxisLock = Array.Find(avatarDescriptor.expressionParameters.parameters, param => param.name == "WorldAxisLock");
            var worldPosCoarse = Array.Find(avatarDescriptor.expressionParameters.parameters, param => param.name == "WorldPosCoarse");
            var worldPosFine = Array.Find(avatarDescriptor.expressionParameters.parameters, param => param.name == "WorldPosFine");

            foreach (var axis in axisBinary.Keys) {
                Transform moverTransform = FindDeepChild(worldObject.transform, (axis == "R") ? "Rotater" : $"{axis}Mover");

                var binary = axisBinary[axis];
                animator.SetBool(worldAxis0.name, binary.Item1);
                animator.SetBool(worldAxis1.name, binary.Item2);

                // Make sure locking works
                animator.SetBool(worldAxisLock.name, true);
                animator.SetFloat(worldPosCoarse.name, 1.0f);
                animator.SetFloat(worldPosFine.name, 1.0f);
                yield return null;

                Assert.AreEqual(0, Vector3.Distance(Vector3.zero, moverTransform.position));
                Assert.AreEqual(0, moverTransform.rotation.eulerAngles.y);

                // Now unlock and make sure the coarse position is correct.

                animator.SetFloat(worldPosFine.name, 0.0f);
                animator.SetBool(worldAxisLock.name, false);
                yield return null;

                if (axis == "R") {
                    Assert.AreEqual(180, moverTransform.rotation.eulerAngles.y);
                }
                else
                {
                    Assert.AreEqual(1000, Vector3.Distance(Vector3.zero, moverTransform.position));
                }


                animator.SetFloat(worldPosCoarse.name, 0.0f);
                animator.SetFloat(worldPosFine.name, 0.0f);
                yield return null;

            }


            yield return new ExitPlayMode();
        }

        [Test]
        public void WorldObjectParametersPasses()
        {
            VRCAvatarParameterDriver parameters = AssetDatabase.LoadAssetAtPath<VRCAvatarParameterDriver>("Assets/vrc-worldobject/WorldObjectParameters.asset");
            //Assert.AreEqual("WorldObjectParameters", parameters.name);


            // Use the Assert class to test conditions
        }



        static Transform FindDeepChild(Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName) return c;
                foreach (Transform t in c) queue.Enqueue(t);
            }
            return null;
        }
    }
}
