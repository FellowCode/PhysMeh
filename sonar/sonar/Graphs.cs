using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sonar
{
    public class Graphs
    {
        
        List<Graph> graphs;
        public int Count;

        public Graphs(Graph[] graphs)
        {
            this.graphs = new List<Graph>(graphs);
            Count = this.graphs.Count;
        }

        public Graphs(List<Graph> graphs)
        {
            this.graphs = graphs;
        }

        public Graph getGraph(int index)
        {
            return graphs[index];
        }

        public void addGraph(Graph graph)
        {
            graphs.Add(graph);
        }
    }

    public class Graph
    {
        List<GraphPoint> graph;

        public Graph()
        {
            graph = new List<GraphPoint>();
        }

        public void AddPoint(GraphPoint point)
        {
            graph.Add(point);
        }

        public void AddPoint(long x, double y)
        {
            graph.Add(new GraphPoint(x, y));
        }

        public GraphPoint getPoint(int index)
        {
            return graph[index];
        }

        public int Count()
        {
            return graph.Count;
        }

    }

    public class GraphPoint
    {
        long x;
        double y;

        public GraphPoint(long x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public long GetX()
        {
            return x;
        }

        public double GetY()
        {
            return y;
        }
    }
}
