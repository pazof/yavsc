
using System.Collections.Generic;

namespace Yavsc.Models
 { 

 public interface IBlogspotRepository
    {
        void Add(Blog item);
        IEnumerable<Blog> GetAll();
        Blog Find(string key);
        Blog Remove(string key);
        void Update(Blog item);
    }

}
