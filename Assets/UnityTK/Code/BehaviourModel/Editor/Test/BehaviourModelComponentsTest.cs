using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.BehaviourModel.Editor.Test
{
    public class BehaviourModelComponentsTest
    {
        [Test]
        public void ActivityTest()
        {
            bool isActive = false, startCondition = false, stopCondition = false;
            bool onJustStart = false, onJustStop = false, onJustFailStart = false, onJustFailStop = false;

            Activity activity = new Activity();
            activity.RegisterActivityGetter(() => isActive);
            activity.RegisterStartCondition(() => startCondition);
            activity.RegisterStopCondition(() => stopCondition);
            activity.onFailStart += () => { onJustFailStart = true; };
            activity.onFailStop += () => { onJustFailStop = true; };
            activity.onStart += () => { onJustStart = true; isActive = true; };
            activity.onStop += () => { onJustStop = true; isActive = false; };

            // Test fail start
            onJustFailStart = false;
            Assert.IsFalse(activity.TryStart());
            Assert.IsTrue(onJustFailStart);
            Assert.IsFalse(activity.IsActive());
            Assert.IsFalse(activity.CanStart());

            // Test success start
            onJustStart = false;
            startCondition = true;
            Assert.IsTrue(activity.CanStart());
            Assert.IsTrue(activity.TryStart());
            Assert.IsTrue(onJustStart);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test fail stop
            onJustFailStop = false;
            Assert.IsFalse(activity.CanStop());
            Assert.IsFalse(activity.TryStop());
            Assert.IsTrue(onJustFailStop);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test success start
            stopCondition = true;
            onJustStop = false;
            Assert.IsTrue(activity.CanStop());
            Assert.IsTrue(activity.TryStop());
            Assert.IsTrue(onJustStop);
            Assert.IsFalse(isActive);
            Assert.IsFalse(activity.IsActive());
            _ActivityTTest();
        }
        
        private void _ActivityTTest()
        {
            bool isActive = false, startCondition = false, stopCondition = false;
            bool onJustStart = false, onJustStop = false, onJustFailStart = false, onJustFailStop = false;
            int p = 123;

            Activity<int> activity = new Activity<int>();
            activity.RegisterActivityGetter(() => isActive);
            activity.RegisterStartCondition((test) => { Assert.AreEqual(p, test); return startCondition; });
            activity.RegisterStopCondition(() => stopCondition);
            activity.onFailStart += (test) => { Assert.AreEqual(p, test); onJustFailStart = true; };
            activity.onFailStop += () => { onJustFailStop = true; };
            activity.onStart += (test) => { Assert.AreEqual(p, test); onJustStart = true; isActive = true; };
            activity.onStop += () => { onJustStop = true; isActive = false; };

            // Test fail start
            onJustFailStart = false;
            Assert.IsFalse(activity.TryStart(p));
            Assert.IsTrue(onJustFailStart);
            Assert.IsFalse(activity.IsActive());
            Assert.IsFalse(activity.CanStart(p));

            // Test success start
            onJustStart = false;
            startCondition = true;
            Assert.IsTrue(activity.CanStart(p));
            Assert.IsTrue(activity.TryStart(p));
            Assert.IsTrue(onJustStart);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test fail stop
            onJustFailStop = false;
            Assert.IsFalse(activity.CanStop());
            Assert.IsFalse(activity.TryStop());
            Assert.IsTrue(onJustFailStop);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test success start
            stopCondition = true;
            onJustStop = false;
            Assert.IsTrue(activity.CanStop());
            Assert.IsTrue(activity.TryStop());
            Assert.IsTrue(onJustStop);
            Assert.IsFalse(isActive);
            Assert.IsFalse(activity.IsActive());
        }

        [Test]
        public void MessageEventTest()
        {
            int p = 123;
            bool wasCalled = false;
            MessageEvent msgEvt = new MessageEvent();
            msgEvt.handler += () => { wasCalled = true; };

            msgEvt.Fire();
            Assert.IsTrue(wasCalled);
            wasCalled = false;

            MessageEvent<int> msgEvtT = new MessageEvent<int>();
            msgEvtT.handler += (test) => { Assert.AreEqual(p, test); wasCalled = true; };

            msgEvtT.Fire(p);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void ModelPropertyTest()
        {
            int val = 0;
            ModelProperty<int> property = new ModelProperty<int>();
            property.onSetValue += (v) => { val = v; };
            property.SetGetter(() => val);

            val = 123;
            Assert.AreEqual(val, property.Get());

            property.Set(1337);
            Assert.AreEqual(1337, val);

            // Test overriding
            int val2 = 321;
            property.SetGetter(() => val2);
            Assert.AreEqual(val2, property.Get());
        }

        [Test]
        public void ModifiableValueTest()
        {
            ModifiableInt integer = new ModifiableInt(100);

            Assert.AreEqual(100, integer.Get());

            integer.AddOverrideEvaluator((val) => val * 2, 1);
            Assert.AreEqual(200, integer.Get());

            integer.AddOverrideEvaluator((val) => val * 2, 3);
            Assert.AreEqual(400, integer.Get());

            integer.AddOverrideEvaluator((val) => val * 3, 2);
            Assert.AreEqual(1200, integer.Get());
        }

        [Test]
        public void AttemptEventTest()
        {
            bool condition = false, onFireCalled = false, onFailCalled = false;
            AttemptEvent attempt = new AttemptEvent();
            attempt.RegisterCondition(() => condition);
            attempt.onFire += () => { onFireCalled = true; };
            attempt.onFail += () => { onFailCalled = true; };

            // Test fail
            onFailCalled = false;
            Assert.IsFalse(attempt.Can());
            Assert.IsFalse(attempt.Try());
            Assert.IsTrue(onFailCalled);

            // Test success
            onFireCalled = false;
            condition = true;
            Assert.IsTrue(attempt.Can());
            Assert.IsTrue(attempt.Try());
            Assert.IsTrue(onFireCalled);

            _AttemptEventTTest();
            _AttemptEventT1T2Test();
        }

        private void _AttemptEventTTest()
        {
            int p = 123;
            bool condition = false, onFireCalled = false, onFailCalled = false;
            AttemptEvent<int> attempt = new AttemptEvent<int>();
            attempt.RegisterCondition((test) => { Assert.AreEqual(p, test); return condition; });
            attempt.onFire += (test) => { Assert.AreEqual(p, test); onFireCalled = true; };
            attempt.onFail += (test) => { Assert.AreEqual(p, test); onFailCalled = true; };

            // Test fail
            onFailCalled = false;
            Assert.IsFalse(attempt.Can(p));
            Assert.IsFalse(attempt.Try(p));
            Assert.IsTrue(onFailCalled);

            // Test success
            onFireCalled = false;
            condition = true;
            Assert.IsTrue(attempt.Can(p));
            Assert.IsTrue(attempt.Try(p));
            Assert.IsTrue(onFireCalled);
        }

        private void _AttemptEventT1T2Test()
        {
            int p = 123, p2 = 1337;
            bool condition = false, onFireCalled = false, onFailCalled = false;
            AttemptEvent<int, int> attempt = new AttemptEvent<int, int>();
            attempt.RegisterCondition((test, test2) => { Assert.AreEqual(p, test); Assert.AreEqual(p2, test2); return condition; });
            attempt.onFire += (test, test2) => { Assert.AreEqual(p, test); Assert.AreEqual(p2, test2); onFireCalled = true; };
            attempt.onFail += (test, test2) => { Assert.AreEqual(p, test); Assert.AreEqual(p2, test2); onFailCalled = true; };

            // Test fail
            onFailCalled = false;
            Assert.IsFalse(attempt.Can(p, p2));
            Assert.IsFalse(attempt.Try(p, p2));
            Assert.IsTrue(onFailCalled);

            // Test success
            onFireCalled = false;
            condition = true;
            Assert.IsTrue(attempt.Can(p, p2));
            Assert.IsTrue(attempt.Try(p, p2));
            Assert.IsTrue(onFireCalled);
        }
    }
}