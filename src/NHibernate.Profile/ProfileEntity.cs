using System;

namespace NHibernate.Profile
{
    public class ProfileEntity
    {
        public virtual string UserName { get; set; }
        public virtual string PropertyNames { get; set; }
        public virtual string PropertyValuesString { get; set; }
        public virtual byte[] PropertyValuesBinary { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }

        public override bool Equals(object obj) {
            ProfileEntity compareTo = obj as ProfileEntity;

            if (ReferenceEquals(this, compareTo))
                return true;

            return compareTo != null && GetType().Equals(compareTo.GetType()) &&
                (compareTo.UserName == this.UserName);
        }

        public override int GetHashCode() {
            return UserName.GetHashCode();
        }
    }
}
