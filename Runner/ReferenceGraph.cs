using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSoftware.DgmlTools;
using OpenSoftware.DgmlTools.Analyses;
using OpenSoftware.DgmlTools.Builders;
using OpenSoftware.DgmlTools.Model;
using ReferenceMapper;

namespace Runner
{
    class ReferenceGraph
    {
        internal static DirectedGraph References2Dgml(List<Member> members)
        {
            var builder = new DgmlBuilder(new HubNodeAnalysis(), new CategoryColorAnalysis())
            {
                NodeBuilders = new[]
                           {
                    new NodeBuilder<Member>(m => new Node{
                        Category = "VM",
                        Label = m.Name,
                        Id = m.Name,
                        CategoryRefs = new List<CategoryRef>()
                    }),
                },
                LinkBuilders = new[]
                           {
                    new LinksBuilder<Member>(m => m.References.Select(r => new Link{Source = m.Name, Target = r }))
                },
                CategoryBuilders = new[] { new CategoryBuilder<Member>(m => new Category {Id = "VM", Label = "VM" }) },
                StyleBuilders = new StyleBuilder[]
                {
                    new StyleBuilder<Node>(n => new Style()),
                    new StyleBuilder<Link>(n => new Style())
                }
            };
            return builder.Build(members);
        }
    }
}
