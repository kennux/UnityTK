using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test
{
    public class OrderedEventTests
    {
        [Test]
        public void OrderedEventTest()
        {
            // Arrange
            OrderedEvent evt = new OrderedEvent();
            int q1 = 100, q2 = 200;
            int firstCalled = -1, secondCalled = -1;

            evt.Register(() =>
            {
                if (firstCalled == -1)
                {
                    firstCalled = 1;
                }
                else if (secondCalled == -1)
                {
                    secondCalled = 1;
                }
            }, q1);

            evt.Register(() =>
            {
                if (firstCalled == -1)
                {
                    firstCalled = 2;
                }
                else if (secondCalled == -1)
                {
                    secondCalled = 2;
                }
            }, q2);

            // Act
            evt.Invoke();

            // Assert
            Assert.AreEqual(1, firstCalled);
            Assert.AreEqual(2, secondCalled);
        }
    }
}