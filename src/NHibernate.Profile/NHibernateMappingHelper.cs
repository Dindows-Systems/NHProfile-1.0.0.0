using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

namespace NHibernate.Profile
{
    /// <summary>
    /// Helper class for creating NHibernate XML mapping documents.
    /// </summary>
    public class NHibernateMappingHelper
    {
        /// <summary>
        /// HBM property attributes
        /// </summary>
        private static string[] HBMAttributeNames = new string[6] { "Name", "Column", "Type", "Length", "Unique", "Not-null" };

        /// <summary>
        /// Default NHibernate mapping values
        /// </summary>
        private static string[,] ProfileEntityFields = new string[6, 6] {
            // Field                 Column                  Type          Length Unique  Not-null
            {"UserName",             "UserName",             "String",     "200", "",      ""},
            {"PropertyNames",        "PropertyNames",        "String",     "",    "false", "true"},
            {"PropertyValuesString", "PropertyValuesString", "String",     "",    "false", "true"},
            {"PropertyValuesBinary", "PropertyValuesBinary", "BinaryBlob", "",    "false", "true"},
            {"LastActivityDate",     "LastActivityDate",     "DateTime",   "",    "false", "true"},
            {"LastUpdateDate",       "LastUpdateDate" ,      "DateTime",   "",    "false", "true"}
        };

        /// <summary>
        /// Default table name
        /// </summary>
        private static string DefaultTableName = "Profile";

        /// <summary>
        /// Generates NHibernate XML mapping document
        /// </summary>
        /// <param name="config">Provider config section</param>
        /// <returns>NHibernate XML mapping document</returns>
        public static XmlDocument GenerateProfileMapping(NameValueCollection config) {

            string tableName = config != null ? config["TableName"] : null;
            if (null == tableName || tableName.Trim().Length == 0)
                tableName = DefaultTableName;

            XmlElement classNode;
            XmlDocument doc = NHibernateMappingHelper.CreateClassMappingDocument(typeof(ProfileEntity), tableName, out classNode);

            for (int i = 0; i < ProfileEntityFields.GetLength(1); i++) {
                IDictionary<string, string> attributes = new Dictionary<string, string>();

                for (int j = 0; j < HBMAttributeNames.Length; j++) {
                    string configKey = ProfileEntityFields[i, 0] + HBMAttributeNames[j];
                    string value = config != null ? config[configKey] : null;
                    if (null == value || value.Trim().Length == 0)
                        value = ProfileEntityFields[i, j];

                    if (!string.IsNullOrEmpty(value))
                        attributes[HBMAttributeNames[j].ToLower()] = value;
                }

                XmlElement fieldNode;

                if (i == 0) {
                    // creates the id and generator
                    fieldNode = NHibernateMappingHelper.CreateElementWithAttributes(doc, "id", attributes);

                    attributes.Clear();
                    attributes["class"] = "assigned";
                    XmlElement generatorNode = NHibernateMappingHelper.CreateElementWithAttributes(doc, "generator", attributes);
                    fieldNode.AppendChild(generatorNode);
                }
                else {
                    // create property
                    fieldNode = NHibernateMappingHelper.CreateElementWithAttributes(doc, "property", attributes);
                }

                classNode.AppendChild(fieldNode);
            }

            return doc;
        }

        /// <summary>
        /// Creates xml document for mapping a class.
        /// </summary>
        /// <param name="entityClass">Type of the entity class to map.</param>
        /// <param name="tableName">Name of the table to map.</param>
        /// <param name="classNode">The class node with the name and table attributes set.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public static XmlDocument CreateClassMappingDocument(System.Type entityClass, string tableName, out XmlElement classNode) {
            IDictionary<string, string> attributes = new Dictionary<string, string>();

            XmlDocument doc = new XmlDocument();
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(decl);

            // creates the root node
            attributes["namespace"] = entityClass.Namespace;
            attributes["assembly"] = entityClass.Assembly.FullName;
            XmlElement rootNode = CreateElementWithAttributes(doc, "hibernate-mapping", attributes);
            doc.AppendChild(rootNode);

            // creates a class
            attributes.Clear();
            attributes["name"] = entityClass.Name;
            attributes["table"] = tableName;
            classNode = CreateElementWithAttributes(doc, "class", attributes);
            rootNode.AppendChild(classNode);

            return doc;
        }

        /// <summary>Creates an Xml element with the specified attributes.</summary>
        /// <param name="doc">The XmlDocument where the element will be added.</param>
        /// <param name="elementName">Name of the element to create.</param>
        /// <param name="attributes">The attributes to add to the element.</param>
        /// <returns>The Xml element with the specified attributes.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public static XmlElement CreateElementWithAttributes(XmlDocument doc, string elementName, IDictionary<string, string> attributes) {
            XmlElement element = doc.CreateElement(elementName, "urn:nhibernate-mapping-2.2");
            foreach (KeyValuePair<string, string> pair in attributes) {
                XmlAttribute attribute = doc.CreateAttribute(pair.Key);
                attribute.Value = pair.Value;
                element.Attributes.Append(attribute);
            }

            return element;
        }
    }
}