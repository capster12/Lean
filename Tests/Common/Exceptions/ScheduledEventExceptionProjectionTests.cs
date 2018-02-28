﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using QuantConnect.Exceptions;
using QuantConnect.Scheduling;

namespace QuantConnect.Tests.Common.Exceptions
{
    [TestFixture]
    public class ScheduledEventExceptionProjectionTests
    {
        [Test]
        [TestCase(typeof(Exception), ExpectedResult = false)]
        [TestCase(typeof(KeyNotFoundException), ExpectedResult = false)]
        [TestCase(typeof(DivideByZeroException), ExpectedResult = false)]
        [TestCase(typeof(InvalidOperationException), ExpectedResult = false)]
        [TestCase(typeof(ScheduledEventException), ExpectedResult = true)]
        public bool CanProjectReturnsTrueForOnlyScheduledEventExceptionType(Type exceptionType)
        {
            var exception = CreateExceptionFromType(exceptionType);
            return new ScheduledEventExceptionProjection().CanProject(exception);
        }

        [Test]
        [TestCase(typeof(Exception), true)]
        [TestCase(typeof(KeyNotFoundException), true)]
        [TestCase(typeof(DivideByZeroException), true)]
        [TestCase(typeof(InvalidOperationException), true)]
        [TestCase(typeof(ScheduledEventException), false)]
        public void ProjectThrowsForNonScheduledEventExceptionTypes(Type exceptionType, bool expectThrow)
        {
            var exception = CreateExceptionFromType(exceptionType);
            var projection = new ScheduledEventExceptionProjection();
            var constraint = expectThrow ? (IResolveConstraint)Throws.Exception : Throws.Nothing;
            Assert.That(() => projection.Project(exception), constraint);
        }

        [Test]
        public void PrependsScheduledEventName()
        {
            var id = Guid.NewGuid();
            var name = id.ToString("D");
            var message = id.ToString("N");
            var exception = new ScheduledEventException(name, message, null);
            var projected = new ScheduledEventExceptionProjection().Project(exception);

            var expectedProjectedMessage = $"In Scheduled Event '{name}', {message}";
            Assert.AreEqual(expectedProjectedMessage, projected.Message);
        }

        [Test]
        public void WrapsScheduledEventExceptionInnerException()
        {
            var inner = new Exception();
            var exception = new ScheduledEventException("name", "message", inner);
            var projected = new ScheduledEventExceptionProjection().Project(exception);
            Assert.AreEqual(inner, projected.InnerException);
        }

        private Exception CreateExceptionFromType(Type type)
        {
            if (type == typeof(ScheduledEventException))
            {
                var inner = new Exception("Sample inner message");
                return new ScheduledEventException(Guid.NewGuid().ToString(), "Sample error message", inner);
            }

            return (Exception)Activator.CreateInstance(type);
        }
    }
}
