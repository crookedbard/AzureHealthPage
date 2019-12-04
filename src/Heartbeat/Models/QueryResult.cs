using System.Collections.Generic;

namespace Heartbeat.Models
{
    public class QueryResult
    {
        public List<Table> Tables { get; set; }

        public class Column
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }

        public class Table
        {
            public string Name { get; set; }
            public List<Column> Columns { get; set; }
            public List<List<object>> Rows { get; set; }
        }
    }
}
