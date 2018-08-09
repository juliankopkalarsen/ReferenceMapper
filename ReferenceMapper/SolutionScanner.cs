using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis;

namespace ReferenceMapper
{
    public class SolutionScanner
    {
        public string[] GetUnreferencedMethods(string solutionpath)
        {
            var msWorkspace = MSBuildWorkspace.Create();

            var solution = msWorkspace.OpenSolutionAsync(solutionpath).Result;

            //var docs = solution.Projects
            //.SelectMany(p => p.Documents);

            var proj = solution.Projects.First();

            var compilation = proj.GetCompilationAsync().Result;
            
            var definedMembers = GetMemberNames(compilation.Assembly.GlobalNamespace).ToList();

            var referencedMembers = proj.Documents.SelectMany(doc => GetReferences(doc)).ToArray();

            var unreferenced = definedMembers.Except(referencedMembers).ToArray();
            var internalRef = definedMembers.Intersect(referencedMembers);
            return unreferenced;
        }


        public static IList<Member> FindUnreferenced(IEnumerable<Member> members) =>
            FindUnreferenced(members, m => m.Name.Contains("static") && m.Name.Contains(".Main("));


        public static IList<Member> FindUnreferenced(IEnumerable<Member> members, Func<Member, bool> except)
        {
            var references = members
                .SelectMany(m => m.References).Distinct().ToList();

            return members
                .Where(m => !except(m))
                .Where(m => !references.Contains(m.Name)).ToList();

        }

        public static IEnumerable<Member> GetMembers(string solutionPath)
        {
            var msWorkspace = MSBuildWorkspace.Create();

            if (solutionPath.EndsWith(".sln"))
            {
                return msWorkspace
                    .OpenSolutionAsync(solutionPath)
                    .Result
                    .Projects
                    .SelectMany(p => GetMembersFromProject(p))
                    .Where(m => m != null);
            }
            else if (solutionPath.EndsWith(".csproj"))
            {
                return GetMembersFromProject(msWorkspace
                    .OpenProjectAsync(solutionPath)
                    .Result);
            }
            else
            {
                throw new ArgumentException("path was not to a solution or project");
            }
        }

        private static IEnumerable<Member> GetMembersFromProject(Project project)
        { 
            var compilation = project.GetCompilationAsync().Result;

            var trees = compilation.SyntaxTrees;
            
            return trees.SelectMany(t => GetMembers(t, compilation.GetSemanticModel(t)));
        }

        private static IEnumerable<Member> GetMembers(SyntaxTree t, SemanticModel semanticModel)
        {
            var memberNodes = t.GetRoot().DescendantNodes().OfType<BaseMethodDeclarationSyntax>();
            return memberNodes.Select(m => GetMembers(m, semanticModel));
        }

        private static Member GetMembers(BaseMethodDeclarationSyntax m, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(m);
            var name = SymbolString(symbol);

            if (name == null)
            {
                return null;
            }
            var mem = new Member { Name = name };

            var refs = new List<string>();

            var invokes = m.SyntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invoke in invokes)
            {
                var info = semanticModel.GetSymbolInfo(invoke);

                ISymbol memberSymbol;
                if (info.Symbol != null)
                {
                    memberSymbol = info.Symbol;
                }
                else if (info.CandidateSymbols.Length > 0)
                {
                    memberSymbol = info.CandidateSymbols[0];
                }
                else
                {
                    memberSymbol = null;
                }
         
                if (memberSymbol == null) continue;
                var str = SymbolString(memberSymbol);
                if (str != null)
                    refs.Add(str);
            }

            mem.References = refs.Distinct().ToArray();

            return mem;
        }

        private static string SymbolString(ISymbol member)
        {
            if (member is IMethodSymbol method)
                return $"{(method.IsStatic ? "static " : "")}{method.ReturnType} {method.ToDisplayString()}";

            if (member is IFieldSymbol field)
                return $"{field.Type} {field.ToDisplayString()}";

            if (member is IPropertySymbol prop)
                return $"{prop.Type} {prop.ToDisplayString()}";

            return member.ToDisplayString();
        }

        private IEnumerable<string> GetReferences(Document doc)
        {
            var model = doc.GetSemanticModelAsync().Result;

            var invokes = doc.GetSyntaxRootAsync().Result.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invoke in invokes)
            {
                var member = model.GetSymbolInfo(invoke).Symbol;
                var str = SymbolString(member);
                if (str != null)
                    yield return str;
            }
        }

        private static IEnumerable<Member> GetMembers(INamespaceSymbol namespaceSymbol)
        {
            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                foreach (var identifier in GetMembers(type))
                {
                    yield return identifier;
                }
            }

            foreach (var childNs in namespaceSymbol.GetNamespaceMembers())
            {
                foreach (var identifier in GetMembers(childNs))
                {
                    yield return identifier;
                }
            }
        }

        private static IEnumerable<Member> GetMembers(INamedTypeSymbol type)
        {
            foreach (var member in type.GetMembers())
            {
                var name = SymbolString(member);
                if (name != null)
                {
                    var def = member.OriginalDefinition;

                    var refs = new[] { "sdf" };

                    yield return new Member {Name = name, References = refs };
                }
            }

            foreach (var nested in type.GetTypeMembers())
            {
                foreach (var item in GetMembers(nested))
                {
                    yield return item;
                }
            }
        }



        private IEnumerable<string> GetMemberNames(INamespaceSymbol namespaceSymbol)
        {
            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                foreach (var identifier in GetMemberNames(type))
                {
                    yield return identifier;
                }
            }

            foreach (var childNs in namespaceSymbol.GetNamespaceMembers())
            {
                foreach (var identifier in GetMemberNames(childNs))
                {
                    yield return identifier;
                }
            }
        }



        private IEnumerable<string> GetMemberNames(INamedTypeSymbol type)
        {
            foreach (var member in type.GetMembers())
            {
                var str = SymbolString(member);
                if (str != null)
                    yield return str;
            }

            foreach (var nested in type.GetTypeMembers())
            {
                foreach (var item in GetMemberNames(nested))
                {
                    yield return item;
                }
            }
        }

        private void ReportReferences(Document doc, SemanticModel model)
        {
            var methodInvocation = doc.GetSyntaxRootAsync().Result.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
            var member = model.GetSymbolInfo(methodInvocation).Symbol;

            Console.WriteLine(SymbolString(member));
        }

        private static void ReportMethods(INamespaceSymbol namespaceSymbol)
        {
            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                ReportMethods(type);
            }

            foreach (var childNs in namespaceSymbol.GetNamespaceMembers())
            {
                ReportMethods(childNs);
            }
        }

        private static void ReportMethods(INamedTypeSymbol type)
        {
            foreach (var member in type.GetMembers())
            {
                Console.WriteLine(SymbolString(member));
            }

            foreach (var nested in type.GetTypeMembers())
            {
                ReportMethods(nested);
            }
        }

        private static string ToIdentifierString(InvocationExpressionSyntax method)
        {
            var b = new StringBuilder();
            //b.Append(method.Modifiers.Aggregate("", (s, m) => s + " " + m.ValueText) + " ");
            //b.Append(method.ReturnType.ToString() + " ");

            var classDec = method.Parent as TypeDeclarationSyntax;
            var namespDec = classDec.Parent as NamespaceDeclarationSyntax;

            string nameSP = namespDec?.Name?.ToString() ?? "";
            string className = classDec.Identifier.ValueText;

            b.Append(nameSP + '.');
            b.Append(className + '.');
            //b.Append(method.Identifier.ValueText);
            //if (method.TypeParameterList != null && method.TypeParameterList.Parameters.Count() > 0)
            //{
            //    b.Append("<" + method.TypeParameterList.Parameters.Aggregate("", (s, t) => s + ", " + t.Identifier.ValueText) + ">");
            //}
            //b.Append("(" +
            //    method.ParameterList.Parameters
            //    .Aggregate("", (s, p) => s.Length == 0 ? p.Type.ToString() : s + ", " + p.Type.ToString()) +
            //    ")"
            //    );
            return b.ToString();
        }

        private static string ToIdentifierString(MethodDeclarationSyntax method)
        {
            var b = new StringBuilder();
            //b.Append(method.Modifiers.Aggregate("", (s, m) => s + " " + m.ValueText) + " ");
            b.Append(method.ReturnType.ToString() + " ");

            var classDec = method.Parent as TypeDeclarationSyntax;
            var namespDec = classDec.Parent as NamespaceDeclarationSyntax;

            string nameSP = namespDec?.Name?.ToString() ?? "";
            string className = classDec.Identifier.ValueText;

            b.Append(nameSP + '.');
            b.Append(className + '.');
            b.Append(method.Identifier.ValueText);
            if (method.TypeParameterList != null && method.TypeParameterList.Parameters.Count() > 0)
            {
                b.Append("<" + method.TypeParameterList.Parameters.Aggregate("", (s, t) => s + ", " + t.Identifier.ValueText) + ">");
            }
            b.Append("(" +
                method.ParameterList.Parameters
                .Aggregate("", (s, p) => s.Length == 0 ? p.Type.ToString() : s + ", " + p.Type.ToString()) +
                ")"
                );
            return b.ToString();
        }
    }
}
