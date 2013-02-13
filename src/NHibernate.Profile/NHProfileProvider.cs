using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Profile;
using System.Collections.Specialized;
using System.Web.Hosting;
using NHibernate;
using NHibernate.Cfg;
using System.Configuration;
using System.Globalization;
using System.Collections;
using System.Web;
using System.IO;
using System.Web.Security;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using NHibernate.Criterion;

namespace NHibernate.Profile
{
    /// <summary>
    /// NHibernate-based profile provider
    /// </summary>
    public class NHProfileProvider : ProfileProvider
    {
        /// <summary>
        /// NHibernate session
        /// </summary>
        private ISessionFactory sessionFactory;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.</param>
        public override void Initialize(string name, NameValueCollection config) {
            NHibernate.Cfg.Configuration cfg = new NHibernate.Cfg.Configuration();
            cfg.Configure();

            XmlDocument doc = NHibernateMappingHelper.GenerateProfileMapping(config);
            cfg.AddDocument(doc);

            sessionFactory = cfg.BuildSessionFactory();

            base.Initialize(name, config);
        }

        ~NHProfileProvider() {
            if (sessionFactory != null)
                sessionFactory.Close();
        }

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate) {

            int totalRecords;
            ProfileInfoCollection infos = GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, 0, 0, out totalRecords);
            return DeleteProfiles(infos);
        }

        /// <summary>
        /// Delete profiles by UserNames
        /// </summary>
        /// <param name="UserNames">array of usernames</param>
        /// <returns>Amount of deleted profiles</returns>
        public override int DeleteProfiles(string[] UserNames) {
            int result = 0;

            ISession session = sessionFactory.OpenSession();

            ITransaction tx = null;
            try {
                tx = session.BeginTransaction();

                foreach (var userName in UserNames) {
                    ProfileEntity profile = GetByUserName(session, userName);
                    if (profile != null) {
                        session.Delete(profile);
                        result++;
                    }
                }
                tx.Commit();
            }
            catch {
                if (null != tx)
                    tx.Rollback();
            }

            session.Close();

            return result;
        }

        /// <summary>
        /// Delete profiles by ProfileInfoCollection
        /// </summary>
        /// <param name="Profiles">collection of profiles</param>
        /// <returns>Amount of deleted profiles</returns>
        public override int DeleteProfiles(ProfileInfoCollection Profiles) {
            List<string> UserNamesList = new List<string>();

            foreach (ProfileInfo info in Profiles)
                UserNamesList.Add(info.UserName);

            return DeleteProfiles(UserNamesList.ToArray());
        }

        /// <summary>
        /// Check — is user authenticated?
        /// </summary>
        /// <returns></returns>
        bool isAnonymous() {
            HttpContext current = HttpContext.Current;

            if (current != null) {
                if (current.Request.IsAuthenticated) {
                    return false;
                }
            }
            return true;
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName
        (
            ProfileAuthenticationOption authenticationOption, 
            string usernameToMatch, 
            DateTime userInactiveSinceDate, 
            int pageIndex, 
            int pageSize, 
            out int totalRecords
         ) {

            ISession session = sessionFactory.OpenSession();

            ProfileInfoCollection infos = new ProfileInfoCollection();

            // get all profiles
            ICriteria criteria1 = session.CreateCriteria(typeof(ProfileEntity));
            if (!string.IsNullOrEmpty(usernameToMatch.Trim()))
                criteria1.Add(Expression.InsensitiveLike("UserName", usernameToMatch, MatchMode.Anywhere));
            criteria1.Add(Expression.Le("LastActivityDate", userInactiveSinceDate));

            ICriteria rowCountCriteria = CriteriaTransformer.TransformToRowCount(criteria1);

            criteria1 = criteria1.SetFirstResult(pageIndex * pageSize)
                        .SetMaxResults(pageSize);

            IList<ProfileEntity> profiles = criteria1.List<ProfileEntity>();

            foreach (ProfileEntity profile in profiles)
            {
                int length = 0;
                if (profile.PropertyValuesBinary != null)
                    length += profile.PropertyValuesBinary.Length;
                if (profile.PropertyValuesString != null)
                    length += profile.PropertyValuesString.Length;
                infos.Add(new ProfileInfo(profile.UserName, this.isAnonymous(), profile.LastActivityDate, profile.LastActivityDate, profile.PropertyNames.Length + length));
            }

            totalRecords = rowCountCriteria.List<int>()[0];

            session.Close();
            return infos;
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
            return FindInactiveProfilesByUserName(authenticationOption, usernameToMatch, DateTime.Now, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords) {
           
            ProfileInfoCollection infos = FindInactiveProfilesByUserName(authenticationOption, "", userInactiveSinceDate, pageIndex, pageIndex, out totalRecords);
            return infos;
        }

        /// <summary>
        /// Get all profiles
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns>collection of profiles</returns>
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords) {
            ISession session = sessionFactory.OpenSession();

            ProfileInfoCollection infos = new ProfileInfoCollection();

            // get all profiles
            ICriteria criteria1 = session.CreateCriteria(typeof(ProfileEntity))
            .SetFirstResult(pageIndex * pageSize)
            .SetMaxResults(pageSize);
            IList<ProfileEntity> profiles = criteria1.List<ProfileEntity>();

            foreach (ProfileEntity profile in profiles) {
                int length = 0;
                if (profile.PropertyValuesBinary != null)
                    length += profile.PropertyValuesBinary.Length;
                if (profile.PropertyValuesString != null)
                    length += profile.PropertyValuesString.Length;

                infos.Add(new ProfileInfo(profile.UserName, this.isAnonymous(), profile.LastActivityDate, profile.LastActivityDate, profile.PropertyNames.Length + length));
            }

            ICriteria rowCountCriteria = CriteriaTransformer.TransformToRowCount(session.CreateCriteria(typeof(ProfileEntity)));
            totalRecords = rowCountCriteria.List<int>()[0];

            session.Close();

            return infos;
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate) {
            int totalRecords;
            GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, 0, 0, out totalRecords);
            return totalRecords;
        }

        /// <summary>
        /// Application name is ignored, this function is here just for compatibilty
        /// </summary>
        public override string ApplicationName {
            get { return ""; }
            set { ; }
        }

        /// <summary>
        /// Parse values as strings or binary objects
        /// </summary>
        /// <param name="names">array of value names</param>
        /// <param name="values">values as one long string</param>
        /// <param name="buf">binary values as array of bytes</param>
        /// <param name="properties">collection of properties</param>
        protected void ParseDataFromDB(string[] names, string values, byte[] buf, SettingsPropertyValueCollection properties) {
            if (((names != null) && (values != null)) && ((buf != null) && (properties != null))) {
                try {
                    for (int i = 0; i < (names.Length / 4); i++) {
                        string str = names[i * 4];
                        SettingsPropertyValue value2 = properties[str];
                        if (value2 != null) {
                            int startIndex = int.Parse(names[(i * 4) + 2], CultureInfo.InvariantCulture);
                            int length = int.Parse(names[(i * 4) + 3], CultureInfo.InvariantCulture);
                            if ((length == -1) && !value2.Property.PropertyType.IsValueType) {
                                value2.PropertyValue = null;
                                value2.IsDirty = false;
                                value2.Deserialized = true;
                            }
                            if (((names[(i * 4) + 1] == "S") && (startIndex >= 0)) && ((length > 0) && (values.Length >= (startIndex + length)))) {
                                value2.SerializedValue = values.Substring(startIndex, length);
                            }
                            if (((names[(i * 4) + 1] == "B") && (startIndex >= 0)) && ((length > 0) && (buf.Length >= (startIndex + length)))) {
                                byte[] dst = new byte[length];
                                Buffer.BlockCopy(buf, startIndex, dst, 0, length);
                                value2.SerializedValue = dst;
                            }
                        }
                    }
                }
                catch {
                }
            }
        }

        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="context">Profile settings context</param>
        /// <param name="properties">collection of properties</param>
        /// <returns></returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties) {
            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();

            if (properties.Count == 0)
                return svc;

            string[] names = null;
            string values = null;

            //Create the default structure of the properties
            foreach (SettingsProperty prop in properties) {
                if (prop.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                    if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                        prop.SerializeAs = SettingsSerializeAs.String;
                    else
                        prop.SerializeAs = SettingsSerializeAs.Xml;

                svc.Add(new SettingsPropertyValue(prop));
            }

            ISession session = sessionFactory.OpenSession();

            ProfileEntity dbProperties = GetByUserName(session, (string)context["UserName"]);

            if (dbProperties != null) {
                names = dbProperties.PropertyNames.Split(':');
                values = dbProperties.PropertyValuesString;

                if (names != null && names.Length > 0) {
                    ParseDataFromDB(names, values, dbProperties.PropertyValuesBinary, svc);
                }

                dbProperties.LastActivityDate = DateTime.Now;

                ITransaction tx = null;
                try {
                    tx = session.BeginTransaction();
                    session.Update(dbProperties);
                    tx.Commit();
                }
                catch {
                    if (null != tx) tx.Rollback();
                    throw;
                }
            }

            session.Close();

            return svc;
        }

        /// <summary>
        /// Set property values
        /// </summary>
        /// <param name="sc">profile settings context</param>
        /// <param name="properties">collection of properties</param>
        public override void SetPropertyValues(System.Configuration.SettingsContext sc, SettingsPropertyValueCollection properties) {
            string UserName = (string)sc["UserName"];
            bool userIsAuthenticated = (bool)sc["IsAuthenticated"];

            ISession session = sessionFactory.OpenSession();
            ITransaction tx = null;
            try {
                tx = session.BeginTransaction();


                if (!string.IsNullOrEmpty(UserName) && properties.Count > 0) {
                    string allNames = string.Empty;
                    string allValues = string.Empty;
                    byte[] buf = null;
                    PrepareDataForSaving(ref allNames, ref allValues, ref buf, true, properties, userIsAuthenticated);
                    if (!string.IsNullOrEmpty(allNames)) {
                        bool isNew = false;
                        ProfileEntity profile = GetByUserName(session, UserName);
                        if (null == profile) {
                            isNew = true;
                            profile = new ProfileEntity();
                            profile.UserName = UserName;
                        }

                        profile.PropertyNames = allNames;
                        profile.PropertyValuesBinary = buf;
                        profile.PropertyValuesString = allValues;
                        profile.LastActivityDate = DateTime.Now;
                        profile.LastUpdateDate = DateTime.Now;

                        if (isNew)
                            session.Save(profile);
                        else
                            session.Update(profile);

                        tx.Commit();
                    }
                }
            }
            catch {
                if (null != tx) tx.Rollback();
                throw;
            }
            finally {
                session.Close();
            }
        }

        /// <summary>
        /// Serialize data values to make it compatible with database structure
        /// </summary>
        /// <param name="allNames"></param>
        /// <param name="allValues"></param>
        /// <param name="buf"></param>
        /// <param name="binarySupported"></param>
        /// <param name="properties"></param>
        /// <param name="userIsAuthenticated"></param>
        protected void PrepareDataForSaving(ref string allNames, ref string allValues, ref byte[] buf, bool binarySupported, SettingsPropertyValueCollection properties, bool userIsAuthenticated) {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            MemoryStream stream = binarySupported ? new MemoryStream() : null;
            try {
                bool flag = false;
                foreach (SettingsPropertyValue value2 in properties) {
                    if (value2.IsDirty && (userIsAuthenticated || ((bool)value2.Property.Attributes["AllowAnonymous"]))) {
                        flag = true;
                        break;
                    }
                }
                if (!flag) {
                    return;
                }
                foreach (SettingsPropertyValue value3 in properties) {
                    if ((!userIsAuthenticated && !((bool)value3.Property.Attributes["AllowAnonymous"])) || (!value3.IsDirty && value3.UsingDefaultValue)) {
                        continue;
                    }
                    int length = 0;
                    int position = 0;
                    string str = null;
                    if (value3.Deserialized && (value3.PropertyValue == null)) {
                        length = -1;
                    }
                    else {
                        object serializedValue = value3.SerializedValue;
                        if (serializedValue == null) {
                            length = -1;
                        }
                        else {
                            if (!(serializedValue is string) && !binarySupported) {
                                serializedValue = Convert.ToBase64String((byte[])serializedValue);
                            }
                            if (serializedValue is string) {
                                str = (string)serializedValue;
                                length = str.Length;
                                position = builder2.Length;
                            }
                            else {
                                byte[] buffer = (byte[])serializedValue;
                                position = (int)stream.Position;
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Position = position + buffer.Length;
                                length = buffer.Length;
                            }
                        }
                    }
                    builder.Append(value3.Name + ":" + ((str != null) ? "S" : "B") + ":" + position.ToString(CultureInfo.InvariantCulture) + ":" + length.ToString(CultureInfo.InvariantCulture) + ":");
                    if (str != null) {
                        builder2.Append(str);
                    }
                }
                if (binarySupported) {
                    buf = stream.ToArray();
                }
            }
            finally {
                if (stream != null) {
                    stream.Close();
                }
            }
            allNames = builder.ToString();
            allValues = builder2.ToString();
        }

        /// <summary>Gets a Profile instance by the user name.</summary>
        /// <param name="userName">Name of the user to retrieve.</param>
        /// <returns>The retrieved Profile instance or null if it wasn't found.</returns>
        protected ProfileEntity GetByUserName(ISession session, string userName) {
            ICriteria criteria = session.CreateCriteria(typeof(ProfileEntity));
            criteria.Add(Expression.Eq("UserName", userName));
            return criteria.UniqueResult<ProfileEntity>();
        }
    }
}
