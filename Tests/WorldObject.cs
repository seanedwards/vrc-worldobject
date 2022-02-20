using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;

namespace Tests
{
    public class WorldObject
    {


        // A Test behaves as an ordinary method
        [Test]
        public void WorldConstraintPasses()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/vrc-worldobject/World Constraint.prefab");

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


        // A Test behaves as an ordinary method
        [Test]
        public void WorldObjectControllerPasses()
        {
            RuntimeAnimatorController animationController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/vrc-worldobject/WorldObjectController.controller");
            Assert.AreEqual("WorldObjectController", animationController.name);


            // Use the Assert class to test conditions
        }

        // A Test behaves as an ordinary method
        [Test]
        public void WorldObjectParametersPasses()
        {
            VRCAvatarParameterDriver parameters = AssetDatabase.LoadAssetAtPath<VRCAvatarParameterDriver>("Assets/vrc-worldobject/WorldObjectParameters.asset");
            //Assert.AreEqual("WorldObjectParameters", parameters.name);


            // Use the Assert class to test conditions
        }
    }
}
