using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReferenceMapper
{
    public class Method : IEquatable<Method>
    {
        public string Name;
        public string[] References;
        public string[] Attributes;

        public string InterfaceName { get; }
        public bool IsGenerated { get; internal set; }

        public Method()
        {

        }

        public Method(IMethodSymbol symbol)
        {
            Name = symbol.GetFullName();
            Attributes = symbol.GetAttributes().Select(a => a.ToString()).ToArray();

            var interfaceSymbol = symbol.ContainingType
                        .AllInterfaces
                        .SelectMany(@interface => @interface.GetMembers().OfType<IMethodSymbol>())
                        .FirstOrDefault(method => symbol.Equals(symbol.ContainingType.FindImplementationForInterfaceMember(method)));

            InterfaceName = interfaceSymbol?.GetFullName();

            var parentSymbol = symbol.ContainingSymbol;

            var parentAttributes = parentSymbol.GetAttributes().Select(a => a.ToString()).ToList();

            IsGenerated = parentAttributes.Any(att => att.Contains("CompilerGenerated"));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Method);
        }

        internal bool IsReferencedBy(string reference) => 
            reference.Equals(Name) || reference.Equals(InterfaceName);

        public bool Equals(Method other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator ==(Method member1, Method member2)
        {
            return EqualityComparer<Method>.Default.Equals(member1, member2);
        }

        public static bool operator !=(Method member1, Method member2)
        {
            return !(member1 == member2);
        }

        public override string ToString() => Name;
    }
}
