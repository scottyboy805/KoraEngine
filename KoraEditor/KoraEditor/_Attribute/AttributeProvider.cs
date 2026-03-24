using System.Reflection;

namespace KoraEditor
{
    internal sealed class AttributeProvider<T> where T : Attribute
    {
        // Type
        private struct MemberAttribute
        {
            public MemberInfo Member;
            public T Attribute;
        }

        // Private
        private readonly List<MemberAttribute> cachedAttributes = new();

        // Constructor
        public AttributeProvider()
        {
            RefreshAttributes();
        }

        // Methods
        public IEnumerable<(T, Type)> GetTypeAttributes()
        {
            return cachedAttributes
                .Where(ma => ma.Member is Type)
                .Select(ma => (ma.Attribute, (Type)ma.Member));
        }

        public IEnumerable<(T, MethodInfo)> GetMethodAttributes()
        {
            return cachedAttributes
                .Where(ma => ma.Member is MethodInfo)
                .Select(ma => (ma.Attribute, (MethodInfo)ma.Member));
        }

        public void RefreshAttributes()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    // Check type
                    if(type.IsDefined(typeof(T), inherit: true))
                    {
                        T attribute = (T)Attribute.GetCustomAttribute(type, typeof(T), inherit: true);
                        cachedAttributes.Add(new MemberAttribute { Member = type, Attribute = attribute });
                    }

                    foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                    {
                        if (Attribute.IsDefined(member, typeof(T)))
                        {
                            T attribute = (T)Attribute.GetCustomAttribute(member, typeof(T));
                            cachedAttributes.Add(new MemberAttribute { Member = member, Attribute = attribute });
                        }
                    }
                }
            }
        }
    }
}
