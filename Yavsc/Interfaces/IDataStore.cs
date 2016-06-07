using System.Threading.Tasks;

namespace Yavsc.Interfaces {

    public interface IDataStore<T> {

        Task 	StoreAsync (string key, T value);

        Task 	DeleteAsync (string key);

        Task<T>	GetAsync (string key);

        Task 	ClearAsync ();

    }
}