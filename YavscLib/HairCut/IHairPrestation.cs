namespace YavscLib
{
    public interface IHairPrestation
    {
         long Id { get; set; }
         int Length { get; set; }
         int Gender { get; set; }
         bool Cut { get; set; }

         int Dressing { get; set; }
         int Tech { get; set; }

         bool Shampoo { get; set; }

         long[] Taints { get; set; }

         bool Cares { get; set; }
    }
}
