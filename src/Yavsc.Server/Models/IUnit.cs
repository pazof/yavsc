namespace Yavsc.Models {
    public interface IUnit<VType> {
          string Name { get; }
          bool CanConvertFrom(IUnit<VType> other);
         VType ConvertFrom (IUnit<VType> other, VType orgValue) ;
    }
}
