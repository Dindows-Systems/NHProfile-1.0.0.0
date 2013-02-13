using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Iesi.Collections;
using Iesi.Collections.Generic;
using NHibernate;
using System.Xml;
using System.Configuration;
using NHibernate.Profile;

namespace NHibernate.Profile.Tests
{
    [TestFixture]
    public class NHibernateMapperTester : NHProfileProvider
    {

        [SetUp]
        public virtual void SetUp()
        {
        }

        [Test]
        public void CanPrepareDataForSaving()
        {
            string allNames = string.Empty;
            string stringValues = string.Empty;
            byte[] binaryValues = null;
            bool userIsAuthenticated = true;

            SettingsPropertyValueCollection properties = new SettingsPropertyValueCollection();

            ////// ADDING INTEGER
            SettingsProperty property1 = new SettingsProperty("TestInteger");
            property1.Attributes["AllowAnonymous"] = false;
            property1.PropertyType = typeof(int);
            SettingsPropertyValue value1 = new SettingsPropertyValue(property1);
            value1.PropertyValue = 1;
            properties.Add(value1);

            ////// ADDING STRING
            SettingsProperty property2 = new SettingsProperty("TestString");
            property2.Attributes["AllowAnonymous"] = false;
            property2.PropertyType = typeof(string);
            SettingsPropertyValue value2 = new SettingsPropertyValue(property2);
            value2.PropertyValue = "this is test string!";
            properties.Add(value2);

            ////// ADDING DATETIME
            SettingsProperty property3 = new SettingsProperty("TestDateTime");
            property3.Attributes["AllowAnonymous"] = false;
            property3.PropertyType = typeof(DateTime);
            property3.SerializeAs = SettingsSerializeAs.Binary;
            SettingsPropertyValue value3 = new SettingsPropertyValue(property3);
            value3.PropertyValue = new DateTime(2009,6,11,11,25,45);
            properties.Add(value3);

            PrepareDataForSaving(ref allNames, ref stringValues, ref binaryValues, true, properties, userIsAuthenticated);

            Assert.That(allNames,Is.EqualTo("TestInteger:S:0:1:TestString:S:1:20:TestDateTime:B:0:78:"));
            Assert.That(stringValues,Is.EqualTo(string.Format("1this is test string!")));
            Assert.That(binaryValues, Is.EqualTo(new byte [] { 0, 1, 0, 0, 0, 255, 255, 255, 255, 1, 0, 0, 0, 0, 0, 0, 0, 4, 1, 0, 0, 0, 15, 83, 121, 115, 116, 101, 109, 46, 68, 97, 116, 101, 84, 105, 109, 101, 2, 0, 0, 0, 5, 116, 105, 99, 107, 115, 8, 100, 97, 116, 101, 68, 97, 116, 97, 0, 0, 9, 16, 128, 194, 244, 126, 158, 184, 203, 8, 128, 194, 244, 126, 158, 184, 203, 8, 11 }));

        }

        [Test]
        public void CanParseDataFromDatabase()
        {
            string allNames = string.Empty;
            string stringValues = string.Empty;
            byte[] binaryValues = null;
            bool userIsAuthenticated = true;

            SettingsPropertyValueCollection properties = new SettingsPropertyValueCollection();

            ////// ADDING INTEGER
            SettingsProperty property1 = new SettingsProperty("TestInteger");
            property1.Attributes["AllowAnonymous"] = false;
            property1.PropertyType = typeof(int);
            SettingsPropertyValue value1 = new SettingsPropertyValue(property1);
            value1.PropertyValue = 1;
            properties.Add(value1);

            ////// ADDING STRING
            SettingsProperty property2 = new SettingsProperty("TestString");
            property2.Attributes["AllowAnonymous"] = false;
            property2.PropertyType = typeof(string);
            SettingsPropertyValue value2 = new SettingsPropertyValue(property2);
            value2.PropertyValue = "this is test string!";
            properties.Add(value2);

            ////// ADDING DATETIME
            SettingsProperty property3 = new SettingsProperty("TestDateTime");
            property3.Attributes["AllowAnonymous"] = false;
            property3.PropertyType = typeof(DateTime);
            property3.SerializeAs = SettingsSerializeAs.Binary;
            SettingsPropertyValue value3 = new SettingsPropertyValue(property3);
            value3.PropertyValue = new DateTime(2009, 6, 11, 11, 25, 45);
            properties.Add(value3);

            PrepareDataForSaving(ref allNames, ref stringValues, ref binaryValues, true, properties, userIsAuthenticated);

            string[] names = null;
            
            names = allNames.Split(':');
            
            Assert.That(names[0], Is.EqualTo("TestInteger"));
            Assert.That(names[4], Is.EqualTo("TestString"));
            Assert.That(names[8], Is.EqualTo("TestDateTime"));

            if (names != null && names.Length > 0)
            {
                ParseDataFromDB(names, stringValues, binaryValues, properties);
            }

            Assert.That(properties["TestInteger"].PropertyValue, Is.EqualTo(1));
            Assert.That(properties["TestString"].PropertyValue, Is.EqualTo("this is test string!"));
            Assert.That(properties["TestDateTime"].PropertyValue.ToString(), Is.EqualTo(new DateTime(2009, 6, 11, 11, 25, 45).ToString()));


        }

    }

}
