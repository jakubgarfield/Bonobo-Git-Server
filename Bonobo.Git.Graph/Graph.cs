using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;

namespace Bonobo.Git.Graph
{
    [DataServiceKey("Id")]
    public class Graph
    {       
        private Repository repository;
        private IList<GraphNode> nodes;
        private IList<GraphLink> links;

        public string Name { get; set; }
        public string Id { get; set; }

        public IEnumerable<GraphNode> Nodes
        {
            get
            {
                if (nodes == null) GenerateGraph();
                return nodes;
            }
        }
        
        public IEnumerable<GraphLink> Links
        {
            get
            {
                if (links == null) GenerateGraph();
                return links;
            }
        }

        public Graph(Repository repository)
        {
            this.repository = repository;
            this.Name = repository.Name;
            this.Id = repository.Id;
        }

        private void GenerateGraph()
        {
            if (repository == null) return;

            nodes = new List<GraphNode>();
            links = new List<GraphLink>();
            var lanes = new List<string>();

            int i = 0;

            var commits = repository.Commits.ToList();
            var refs = repository.Refs.ToArray();

            foreach (var commit in commits)
            {
                var id = commit.Id;
                var tags = from r in refs
                           where r.Type == "tags" && r.Id == id
                           select r.ToString();

                var branches = from r in refs
                           where r.Type == "heads" && r.Id == id
                           select r.ToString();
                
                var children = from c in commits
                               where c.ParentIds.Contains(id)
                               select c;


                var lane = -1;
                if (children.Count() > 1)
                {
                    lanes.Clear();
                }
                else 
                {
                    var child = children.Where(c=>c.ParentIds.IndexOf(id)==0)
                                        .Select(c=>c.Id).FirstOrDefault();

                    lane = lanes.IndexOf(child);
                }

                if (lane < 0)
                {
                    lanes.Add(id);
                    lane = lanes.Count - 1;
                }
                else
                {
                    lanes[lane] = id;
                }
                
                var node = new GraphNode 
                { 
                    X = lane, Y = i++, Id = id, Message = commit.Message,
                    CommitterName = commit.CommitterName, CommitDateRelative = commit.CommitDateRelative,
                    Tags = string.Join(",", tags),
                    Branches = string.Join(",", branches),
                };

                nodes.Add(node);

                foreach (var ch in children)
                {
                    var cnode = (from n in nodes
                                 where n.Id == ch.Id
                                 select n).FirstOrDefault();
                    if (cnode != null)
                    {
                        links.Add(new GraphLink
                        {
                            X1 = cnode.X,
                            Y1 = cnode.Y,
                            X2 = node.X,
                            Y2 = node.Y,
                            Id = id
                        });
                    }
                }
            }
        }
    }
}