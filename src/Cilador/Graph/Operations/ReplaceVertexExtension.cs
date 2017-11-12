using Cilador.Graph.Core;
using Cilador.Graph.Factory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cilador.Graph.Operations
{
    public static class ReplaceVertexExtension
    {
        public static ICilGraph ReplaceVertex(this ICilGraph source, object originalVertex, ICilGraph newVertexGraph, object newVertex)
        {
            return source.ReplaceVertices(v => v == originalVertex, newVertexGraph, newVertex);
        }

        public static ICilGraph ReplaceVertex(this ICilGraph source, object originalVertex, object newVertex)
        {
            return source.ReplaceVertices(v => v == originalVertex, newVertex);
        }

        public static ICilGraph ReplaceVertices(this ICilGraph source, Func<object, bool> predicate, ICilGraph newVertexGraph, object newVertex)
        {
            var newSource = source == newVertexGraph ? source : source.Merge(newVertexGraph);
            return newSource.ReplaceVertices(predicate, newVertex);
        }

        public static ICilGraph ReplaceVertices(this ICilGraph source, Func<object, bool> predicate, object newVertex)
        {
            var fullGraph = new CilGraph(
                source.Vertices.Select(v => predicate(v) ? newVertex : v),
                source.ParentChildEdges.Select(e => e.ReplaceVertices(predicate, newVertex, (from, to) => new ParentChildCilEdge(to, from))),
                source.SiblingEdges.Select(e => e.ReplaceVertices(predicate, newVertex, (from, to) => new SiblingCilEdge(to, from))),
                source.DependencyEdges.Select(e => e.ReplaceVertices(predicate, newVertex, (from, to) => new DependencyCilEdge(from, to))));

            return fullGraph;
        }

        private static TCilEdge ReplaceVertices<TCilEdge>(
            this TCilEdge source,
            Func<object, bool> predicate,
            object newVertex,
            Func<object, object, TCilEdge> edgeFactory)
            where TCilEdge : ICilEdge
        {
            var fromMatches = predicate(source.From);
            var toMatches = predicate(source.To);

            return
                fromMatches || toMatches ?
                edgeFactory(fromMatches ? newVertex : source.From, toMatches ? newVertex : source.To) :
                source;
        }
    }
}
