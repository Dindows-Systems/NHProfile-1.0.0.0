using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NHibernate.Profile;
using NUnit.Framework.SyntaxHelpers;
using Iesi.Collections;
using Iesi.Collections.Generic;

namespace NHibernate.Profile.Tests
{
    [TestFixture]
    public class ProfileEntityTests
    {
        [SetUp]
        public virtual void SetUp()
        {
            
        }

        [Test]
        public void CanCreate()
        {
            DateTime currentDateTime = DateTime.Now;

            ProfileEntity profile = new ProfileEntity()
            {
                UserName = "testuser",
                PropertyNames = "TestValue:S:0:1:",
                PropertyValuesString = "2",
                LastActivityDate = currentDateTime,
                LastUpdateDate = currentDateTime
            };

            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.UserName, Is.EqualTo("testuser"));
            Assert.That(profile.PropertyValuesString, Is.EqualTo("2"));
            Assert.That(profile.LastActivityDate, Is.EqualTo(currentDateTime));
            Assert.That(profile.LastUpdateDate, Is.EqualTo(currentDateTime));

        }

        [Test]
        public void CanCompare()
        {
            DateTime currentDateTime = DateTime.Now;

            ProfileEntity profile1 = new ProfileEntity()
            {
                UserName = "testuser",
                PropertyNames = "TestValue:S:0:1:",
                PropertyValuesString = "2",
                LastActivityDate = currentDateTime,
                LastUpdateDate = currentDateTime
            };

            ProfileEntity profile2 = new ProfileEntity()
            {
                UserName = "testuser",
                PropertyNames = "TestValue:S:0:1:",
                PropertyValuesString = "2",
                LastActivityDate = currentDateTime,
                LastUpdateDate = currentDateTime
            };

            Assert.That(profile1, Is.EqualTo(profile2));

            profile2.LastActivityDate = currentDateTime.AddDays(1);
            Assert.That(profile1, Is.EqualTo(profile2));
            profile2.LastActivityDate = profile1.LastActivityDate;
            Assert.That(profile1, Is.EqualTo(profile2));

            profile2.LastUpdateDate = currentDateTime.AddDays(1);
            Assert.That(profile1, Is.EqualTo(profile2));
            profile2.LastUpdateDate = profile1.LastUpdateDate;
            Assert.That(profile1, Is.EqualTo(profile2));

            profile2.PropertyNames = "zz";
            Assert.That(profile1, Is.EqualTo(profile2));
            profile2.PropertyNames = profile1.PropertyNames;
            Assert.That(profile1, Is.EqualTo(profile2));

            profile2.PropertyValuesString = "zz";
            Assert.That(profile1, Is.EqualTo(profile2));
            profile2.PropertyValuesString = profile1.PropertyValuesString;
            Assert.That(profile1, Is.EqualTo(profile2));

            profile2.PropertyValuesBinary = new byte[] { 1, 2, 3 };
            Assert.That(profile1, Is.EqualTo(profile2));
            profile2.PropertyValuesBinary = profile1.PropertyValuesBinary;
            Assert.That(profile1, Is.EqualTo(profile2));

            profile2.UserName = "user";
            Assert.That(profile1, Is.Not.EqualTo(profile2));
            profile2.UserName = profile1.UserName;
            Assert.That(profile1, Is.EqualTo(profile2));
        }

    }
}
