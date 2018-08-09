using System;
using System.Collections.Generic;

namespace ReferenceMapper
{
    public class Member : IEquatable<Member>
    {
        public string Name;
        public string[] References;
        public string[] Attributes;

        public bool IsGenerated { get; internal set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Member);
        }

        public bool Equals(Member other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator ==(Member member1, Member member2)
        {
            return EqualityComparer<Member>.Default.Equals(member1, member2);
        }

        public static bool operator !=(Member member1, Member member2)
        {
            return !(member1 == member2);
        }

        public override string ToString() => Name;
    }
}
