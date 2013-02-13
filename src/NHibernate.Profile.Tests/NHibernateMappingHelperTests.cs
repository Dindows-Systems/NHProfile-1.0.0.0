using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml;
using NUnit.Framework.SyntaxHelpers;
using System.Collections.Specialized;


namespace NHibernate.Profile.Tests
{
    [TestFixture]
    public class NHibernateMappingHelperTests
    {
        [SetUp]
        public virtual void SetUp()
        {
        }

        [Test]
        public void CanGenerateProfileMapping()
        {
            XmlDocument docGenerated = NHibernateMappingHelper.GenerateProfileMapping(null);

            XmlDocument docPremade = new XmlDocument();
            docPremade.InnerXml =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<hibernate-mapping namespace=\"NHibernate.Profile\" assembly=\"NHibernate.Profile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" xmlns=\"urn:nhibernate-mapping-2.2\">" +
                "<class name=\"ProfileEntity\" table=\"Profile\">" +
                "<id name=\"UserName\" column=\"UserName\" type=\"String\" length=\"200\">" +
                "<generator class=\"assigned\" /></id>" +
                "<property name=\"PropertyNames\" column=\"PropertyNames\" type=\"String\" unique=\"false\" not-null=\"true\"/>" +
                "<property name=\"PropertyValuesString\" column=\"PropertyValuesString\" type=\"String\" unique=\"false\" not-null=\"true\"/>" +
                "<property name=\"PropertyValuesBinary\" column=\"PropertyValuesBinary\" type=\"BinaryBlob\" unique=\"false\" not-null=\"true\"/>" +
                "<property name=\"LastActivityDate\" column=\"LastActivityDate\" type=\"DateTime\" unique=\"false\" not-null=\"true\" />" +
                "<property name=\"LastUpdateDate\" column=\"LastUpdateDate\" type=\"DateTime\" unique=\"false\" not-null=\"true\" />" +
                "</class>" +
                "</hibernate-mapping>";

            Assert.That(docGenerated, Is.Not.Null);
            Assert.That(docGenerated.InnerXml, Is.EqualTo(docPremade.InnerXml));

            NameValueCollection config = new NameValueCollection();
            config.Add("UserNameColumn", "NameColumn");
            config.Add("UserNameType", "String");
            config.Add("UserNameLength", "1000");
            config.Add("PropertyNamesName", "PNames");
            config.Add("PropertyNamesColumn", "PNamesColumn");
            config.Add("PropertyNamesType", "String");
            config.Add("PropertyValuesStringName", "PVString");
            config.Add("PropertyValuesStringColumn", "PVStringColumn");
            config.Add("PropertyValuesStringType", "String");
            config.Add("PropertyValuesBinaryName", "PVBinary");
            config.Add("PropertyValuesBinaryColumn", "PVBinaryColumn");
            config.Add("PropertyValuesBinaryType", "BinaryBlob");
            config.Add("LastActivityDateName", "LADate");
            config.Add("LastActivityDateColumn", "LADateColumn");
            config.Add("LastActivityDateType", "DateTime");
            config.Add("LastUpdateDateName", "LUDate");
            config.Add("LastUpdateDateColumn", "LUDateColumn");
            config.Add("LastUpdateDateType", "DateTime");

            XmlDocument  docGenerated2 = NHibernateMappingHelper.GenerateProfileMapping(config);
            XmlDocument docPremade2 = new XmlDocument();
            docPremade2.InnerXml =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<hibernate-mapping namespace=\"NHibernate.Profile\" assembly=\"NHibernate.Profile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" xmlns=\"urn:nhibernate-mapping-2.2\">" +
                "<class name=\"ProfileEntity\" table=\"Profile\">" +
                "<id name=\"UserName\" column=\"NameColumn\" type=\"String\" length=\"1000\">" +
                "<generator class=\"assigned\" /></id>" +
                "<property name=\"PNames\" column=\"PNamesColumn\" type=\"String\" unique=\"false\" not-null=\"true\"/>" +
                "<property name=\"PVString\" column=\"PVStringColumn\" type=\"String\" unique=\"false\" not-null=\"true\"/>" +
                "<property name=\"PVBinary\" column=\"PVBinaryColumn\" type=\"BinaryBlob\" unique=\"false\" not-null=\"true\"/>" +
                "<property name=\"LADate\" column=\"LADateColumn\" type=\"DateTime\" unique=\"false\" not-null=\"true\" />" +
                "<property name=\"LUDate\" column=\"LUDateColumn\" type=\"DateTime\" unique=\"false\" not-null=\"true\" />" +
                "</class>" +
                "</hibernate-mapping>";

            Assert.That(docGenerated2, Is.Not.Null);
            Assert.That(docGenerated2.InnerXml, Is.EqualTo(docPremade2.InnerXml));
        }
    }
}
