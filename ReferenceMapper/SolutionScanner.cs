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

        public static IList<Method> FindUnreferenced(IEnumerable<Method> members, Func<Method, bool> isRoot)
        {
            var membersList = members.ToList();

            var roots = membersList.Where(isRoot).ToList();

            var referencedMethods = roots
                .SelectMany(root => FindAllReferences(root, membersList))
                .Concat(roots)
                .ToList();

            return membersList.Where(m => !referencedMethods.Contains(m)).ToList();
        }

        private static IEnumerable<Method> FindAllReferences(Method root, List<Method> membersList)
        {
            foreach (var method in membersList)
            {
                foreach (var reference in root.References)
                {
                    if (method.IsReferencedBy(reference))
                    {
                        yield return method;
                        foreach (var item in FindAllReferences(method, membersList))
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        private static void RemoveReferences(Method root, Dictionary<string, Method> membersByName)
        {
            var toRemove = new List<Method>();
            foreach (var referenceName in root.References)
            {
                if (membersByName.ContainsKey(referenceName))
                {
                    toRemove.Add(membersByName[referenceName]);
                    membersByName.Remove(referenceName);
                }
            }
            foreach (var item in toRemove)
            {
                RemoveReferences(item, membersByName);
            }
        }

        public static IEnumerable<Method> GetMembers(string solutionPath)
        {
            var msWorkspace = MSBuildWorkspace.Create();

            if (solutionPath.EndsWith(".sln"))
            {
                return ReadSolution(solutionPath, msWorkspace);
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

        private static IEnumerable<Method> ReadSolution(string solutionPath, MSBuildWorkspace msWorkspace)
        {
            return msWorkspace
                    .OpenSolutionAsync(solutionPath)
                    .Result
                    .Projects
                    .SelectMany(p => GetMembersFromProject(p))
                    .Where(m => m != null);
        }

        private static IEnumerable<Method> GetMembersFromProject(Project project)
        { 
            var compilation = project.GetCompilationAsync().Result;

            var trees = compilation.SyntaxTrees;
            
            return trees.SelectMany(t => GetMembers(t, compilation.GetSemanticModel(t)));
        }

        private static IEnumerable<Method> GetMembers(SyntaxTree t, SemanticModel semanticModel)
        {
            var memberNodes = t.GetRoot().DescendantNodes().OfType<BaseMethodDeclarationSyntax>();
            return memberNodes.Select(m => GetMembers(m, semanticModel));
        }

        private static Method GetMembers(BaseMethodDeclarationSyntax m, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(m) as IMethodSymbol;


            var method = new Method(symbol);

            var refs = new List<string>();

            var invokes = m?.Body?.DescendantNodes()?.OfType<InvocationExpressionSyntax>() ?? new List<InvocationExpressionSyntax>();
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

            method.References = refs.Distinct().ToArray();

            return method;
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
    }
}
